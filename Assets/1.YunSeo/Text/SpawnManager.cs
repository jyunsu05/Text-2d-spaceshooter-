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
    [Header("SpawnPoint_0~4 에서 소환될 적 (Enemy_A, Enemy_C)")]
    [SerializeField] private EnemySpawnEntry[] groupA_Entries;

    [Header("SpawnPoint_5~8 에서 소환될 적 (Enemy_B 전용)")]
    [SerializeField] private EnemySpawnEntry[] groupB_Entries;

    private List<SpawnPoint> groupA_Points = new List<SpawnPoint>(); // 0~4
    private List<SpawnPoint> groupB_Points = new List<SpawnPoint>(); // 5~8

    void Start()
    {
        // 씬에 있는 모든 SpawnPoint를 찾아서 번호별로 그룹 분류
        SpawnPoint[] allPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint point in allPoints)
        {
            int index = point.GetSpawnPointIndex();

            if (index >= 0 && index <= 4)
                groupA_Points.Add(point);
            else if (index >= 5 && index <= 8)
                groupB_Points.Add(point);
        }

        // GroupA: 적 종류마다 코루틴 하나씩만 돌리고, 소환 위치는 매번 랜덤 선택
        foreach (EnemySpawnEntry entry in groupA_Entries)
        {
            if (entry == null || entry.enemyPrefab == null) continue;
            StartCoroutine(SpawnLoopRandom(groupA_Points, entry));
        }

        // GroupB: 각 포인트마다 독립 소환 (5,6 오른쪽 / 7,8 왼쪽 대각선 유지)
        foreach (SpawnPoint point in groupB_Points)
        {
            foreach (EnemySpawnEntry entry in groupB_Entries)
            {
                if (entry == null || entry.enemyPrefab == null) continue;
                StartCoroutine(SpawnLoop(point, entry, true));
            }
        }
    }

    // GroupA 전용: 매 소환마다 랜덤 스폰포인트 선택
    private IEnumerator SpawnLoopRandom(List<SpawnPoint> points, EnemySpawnEntry entry)
    {
        if (entry.firstSpawnDelay > 0f)
            yield return new WaitForSeconds(entry.firstSpawnDelay);

        while (true)
        {
            if (points.Count > 0)
            {
                SpawnPoint randomPoint = points[Random.Range(0, points.Count)];
                SpawnEnemy(randomPoint, entry.enemyPrefab, false);
            }

            float delay = Random.Range(entry.minSpawnInterval, entry.maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator SpawnLoop(SpawnPoint point, EnemySpawnEntry entry, bool isGroupB)
    {
        if (entry.firstSpawnDelay > 0f)
            yield return new WaitForSeconds(entry.firstSpawnDelay);

        while (true)
        {
            SpawnEnemy(point, entry.enemyPrefab, isGroupB);

            float delay = Random.Range(entry.minSpawnInterval, entry.maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnEnemy(SpawnPoint point, GameObject enemyPrefab, bool isGroupB)
    {
        if (enemyPrefab == null || point == null) return;

        GameObject spawnedEnemy = Instantiate(enemyPrefab, point.transform.position, Quaternion.identity);

        // groupB (5~8)에서만 Enemy_B 대각선 방향 적용
        if (!isGroupB) return;

        Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
        if (enemy == null) return;

        int index = point.GetSpawnPointIndex();

        if (index == 5 || index == 6)
            enemy.SetMoveDirection(new Vector2(1f, -1f));   // 오른쪽 대각선
        else if (index == 7 || index == 8)
            enemy.SetMoveDirection(new Vector2(-1f, -1f));  // 왼쪽 대각선
    }
}
