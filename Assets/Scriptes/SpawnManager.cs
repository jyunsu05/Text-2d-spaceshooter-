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

// 씬에 빈 오브젝트 하나에만 붙이면 됨
// SpawnPoint_0~8 오브젝트들을 자동으로 찾아서 스폰 관리
public class SpawnManager : MonoBehaviour
{
    [Header("SpawnPoint_0~8 전체에서 소환될 적들")]
    [SerializeField] private EnemySpawnEntry[] spawnEntries;

    private List<SpawnPoint> allPoints = new List<SpawnPoint>(); // 0~8 전체
    private ObjectManager objectManager;

    void Start()
    {
        objectManager = ObjectManager.Instance;

        // 씬에 있는 모든 SpawnPoint를 찾아서 리스트에 등록
        SpawnPoint[] found = FindObjectsByType<SpawnPoint>();

        foreach (SpawnPoint point in found)
        {
            int index = point.GetSpawnPointIndex();
            if (index >= 0 && index <= 8)
                allPoints.Add(point);
        }

        // 적 종류마다 코루틴 하나씩 실행, 매 소환마다 랜덤 포인트 선택
        foreach (EnemySpawnEntry entry in spawnEntries)
        {
            if (entry == null || entry.enemyPrefab == null) continue;
            StartCoroutine(SpawnLoopRandom(entry));
        }
    }

    // 매 소환마다 0~8 중 랜덤 포인트에서 적 생성
    private IEnumerator SpawnLoopRandom(EnemySpawnEntry entry)
    {
        if (entry.firstSpawnDelay > 0f)
            yield return new WaitForSeconds(entry.firstSpawnDelay);

        while (true)
        {
            if (allPoints.Count > 0)
            {
                SpawnPoint randomPoint = allPoints[Random.Range(0, allPoints.Count)];
                SpawnEnemy(randomPoint, entry.enemyPrefab);
            }

            float delay = Random.Range(entry.minSpawnInterval, entry.maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnEnemy(SpawnPoint point, GameObject enemyPrefab)
    {
        if (enemyPrefab == null || point == null) return;

        int index = point.GetSpawnPointIndex();

        // 이동 방향 및 소환 각도 결정
        Vector2 moveDir = Vector2.down;
        Quaternion rotation = Quaternion.identity;

        if (index == 5 || index == 6)
        {
            moveDir = new Vector2(1f, -1f);           // 오른쪽 대각선 이동
            rotation = Quaternion.Euler(0f, 0f, 45f); // 오른쪽으로 45도 기울기
        }
        else if (index == 7 || index == 8)
        {
            moveDir = new Vector2(-1f, -1f);           // 왼쪽 대각선 이동
            rotation = Quaternion.Euler(0f, 0f, -45f);  // 왼쪽으로 45도 기울기
        }

        GameObject spawnedEnemy = null;
        if (objectManager != null)
        {
            spawnedEnemy = objectManager.MakeObjByPrefab(enemyPrefab, point.transform.position, rotation);
        }

        if (spawnedEnemy == null)
        {
            spawnedEnemy = Instantiate(enemyPrefab, point.transform.position, rotation);
        }

        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        if (enemy == null) return;

        // 포인트 번호에 따라 이동 방향 명시 설정
        // 0~4: 위에서 아래로 직진, 5~6: 오른쪽 대각선, 7~8: 왼쪽 대각선
        if (index == 5 || index == 6)
            enemy.SetMoveDirection(new Vector2(1f, -1f));
        else if (index == 7 || index == 8)
            enemy.SetMoveDirection(new Vector2(-1f, -1f));
        else
            enemy.SetMoveDirection(Vector2.down); // 풀 재사용 시 이전 대각선 방향 초기화
    }
}
