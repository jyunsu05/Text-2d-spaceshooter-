using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;       // 소환할 적 프리팹
    public float firstSpawnDelay = 2f;   // 게임 시작 후 첫 소환 대기 시간
    public float minSpawnInterval = 4f;  // 최소 소환 간격
    public float maxSpawnInterval = 7f;  // 최대 소환 간격
}

// ──────────────────────────────────────────────────────────────
// SpawnManager
// 씬에 빈 오브젝트 하나에만 붙이면 됨
// SpawnPoint_0~8 오브젝트들을 자동으로 찾아서 스폰 관리
//
// [스테이지 파일 사용 시]
//   stageFile(TextAsset)을 인스펙터에서 연결하면 파일 기반 소환 실행
//   파일 형식 (한 줄 = 소환 1회):
//     delay초, 적타입(A/B/C), 스폰포인트인덱스(0~8)
//     예) 1, A, 0   → 1초 대기 후 Enemy_A를 포인트 0에서 소환
//
// [스테이지 파일 없을 시]
//   spawnEntries에 설정된 적들을 랜덤 포인트에서 무한 소환
// ──────────────────────────────────────────────────────────────
public class SpawnManager : MonoBehaviour
{
    [Header("스테이지 파일 (연결 시 파일 기반 소환 우선 실행)")]
    [SerializeField] private TextAsset stageFile;

    [Header("스테이지 파일 없을 때 사용할 랜덤 소환 설정")]
    [SerializeField] private EnemySpawnEntry[] spawnEntries;

    // 스폰포인트 인덱스(0~8) → SpawnPoint 오브젝트 빠른 조회용 딕셔너리
    private Dictionary<int, SpawnPoint> pointsByIndex = new Dictionary<int, SpawnPoint>();
    private List<SpawnPoint> allPoints = new List<SpawnPoint>(); // 랜덤 소환 폴백용
    private ObjectManager objectManager;

    void Start()
    {
        objectManager = ObjectManager.Instance;

        // 씬의 모든 SpawnPoint를 인덱스 번호로 등록
        SpawnPoint[] found = FindObjectsByType<SpawnPoint>();
        foreach (SpawnPoint point in found)
        {
            int idx = point.GetSpawnPointIndex();
            if (idx >= 0 && idx <= 8)
            {
                pointsByIndex[idx] = point;
                allPoints.Add(point);
            }
        }

        // 스테이지 파일이 연결되어 있으면 파일 기반 소환 실행
        if (stageFile != null)
        {
            StartCoroutine(RunStageFile());
        }
        else
        {
            // 파일 없으면 기존 랜덤 소환 방식으로 폴백
            foreach (EnemySpawnEntry entry in spawnEntries)
            {
                if (entry == null || entry.enemyPrefab == null) continue;
                StartCoroutine(SpawnLoopRandom(entry));
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 스테이지 파일 파싱 및 순차 소환
    // 파일 형식: "delay초, 적타입(A/B/C), 스폰포인트인덱스"
    // ──────────────────────────────────────────────────────────────
    private IEnumerator RunStageFile()
    {
        string[] lines = stageFile.text.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            // 빈 줄 또는 주석(# 시작) 무시
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(',');
            if (parts.Length < 3)
            {
                Debug.LogWarning($"[SpawnManager] 파싱 실패 (형식 오류): '{line}'");
                continue;
            }

            // delay 파싱
            if (!float.TryParse(parts[0].Trim(), out float delay))
            {
                Debug.LogWarning($"[SpawnManager] delay 파싱 실패: '{parts[0].Trim()}'");
                continue;
            }

            // 적 타입 파싱 (A/B/C → Enemy_A/Enemy_B/Enemy_C)
            string typeChar = parts[1].Trim().ToUpper();
            string enemyType;
            switch (typeChar)
            {
                case "A": enemyType = "Enemy_A"; break;
                case "B": enemyType = "Enemy_B"; break;
                case "C": enemyType = "Enemy_C"; break;
                default:
                    Debug.LogWarning($"[SpawnManager] 알 수 없는 적 타입: '{typeChar}'");
                    continue;
            }

            // 스폰포인트 인덱스 파싱
            if (!int.TryParse(parts[2].Trim(), out int pointIndex))
            {
                Debug.LogWarning($"[SpawnManager] 스폰포인트 인덱스 파싱 실패: '{parts[2].Trim()}'");
                continue;
            }

            // delay 대기 후 소환
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            SpawnEnemyByType(enemyType, pointIndex);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 타입 문자열 + 포인트 인덱스로 적 소환
    // ObjectManager 풀에서 꺼내고 이동 방향을 포인트에 맞게 설정
    // ──────────────────────────────────────────────────────────────
    private void SpawnEnemyByType(string enemyType, int pointIndex)
    {
        if (!pointsByIndex.TryGetValue(pointIndex, out SpawnPoint point))
        {
            Debug.LogWarning($"[SpawnManager] SpawnPoint_{pointIndex} 를 찾을 수 없습니다.");
            return;
        }

        Quaternion rotation = Quaternion.identity;
        if (pointIndex == 5 || pointIndex == 6)
            rotation = Quaternion.Euler(0f, 0f, 45f);
        else if (pointIndex == 7 || pointIndex == 8)
            rotation = Quaternion.Euler(0f, 0f, -45f);

        GameObject spawnedEnemy = null;
        if (objectManager != null)
            spawnedEnemy = objectManager.MakeObj(enemyType, point.transform.position, rotation);

        if (spawnedEnemy == null)
        {
            Debug.LogWarning($"[SpawnManager] ObjectManager에서 '{enemyType}' 풀 오브젝트를 가져오지 못했습니다.");
            return;
        }

        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        if (enemy == null) return;

        // 포인트 번호에 따라 이동 방향 설정
        if (pointIndex == 5 || pointIndex == 6)
            enemy.SetMoveDirection(new Vector2(1f, -1f));
        else if (pointIndex == 7 || pointIndex == 8)
            enemy.SetMoveDirection(new Vector2(-1f, -1f));
        else
            enemy.SetMoveDirection(Vector2.down);
    }

    // ──────────────────────────────────────────────────────────────
    // 스테이지 파일 없을 때 폴백: 랜덤 포인트에서 무한 소환
    // ──────────────────────────────────────────────────────────────
    private IEnumerator SpawnLoopRandom(EnemySpawnEntry entry)
    {
        if (entry.firstSpawnDelay > 0f)
            yield return new WaitForSeconds(entry.firstSpawnDelay);

        while (true)
        {
            if (allPoints.Count > 0)
            {
                SpawnPoint randomPoint = allPoints[Random.Range(0, allPoints.Count)];
                SpawnEnemyByPrefab(randomPoint, entry.enemyPrefab);
            }

            float delay = Random.Range(entry.minSpawnInterval, entry.maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    // 랜덤 소환 폴백용 - 프리팹 레퍼런스로 직접 소환
    private void SpawnEnemyByPrefab(SpawnPoint point, GameObject enemyPrefab)
    {
        if (enemyPrefab == null || point == null) return;

        int index = point.GetSpawnPointIndex();

        Quaternion rotation = Quaternion.identity;
        if (index == 5 || index == 6)
            rotation = Quaternion.Euler(0f, 0f, 45f);
        else if (index == 7 || index == 8)
            rotation = Quaternion.Euler(0f, 0f, -45f);

        GameObject spawnedEnemy = null;
        if (objectManager != null)
            spawnedEnemy = objectManager.MakeObjByPrefab(enemyPrefab, point.transform.position, rotation);

        if (spawnedEnemy == null)
            spawnedEnemy = Instantiate(enemyPrefab, point.transform.position, rotation);

        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        if (enemy == null) return;

        if (index == 5 || index == 6)
            enemy.SetMoveDirection(new Vector2(1f, -1f));
        else if (index == 7 || index == 8)
            enemy.SetMoveDirection(new Vector2(-1f, -1f));
        else
            enemy.SetMoveDirection(Vector2.down);
    }
}
