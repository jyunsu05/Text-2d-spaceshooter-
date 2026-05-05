using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2.0f;       // 보스 하강 속도
    public float targetY = 1.3f;         // 도착 후 고정될 Y 좌표
    private bool isArrived = false;      // 목표 위치 도착 여부

    [Header("체력 설정")] 
    public float maxHp = 100f;           // 최대 체력
    public float currentHp;              // 현재 체력

    [Header("피격 연출 설정")]
    public float hitFeedbackDuration = 0.2f; // Boss_Hit 애니메이션 지속 시간

    private Animator animator;
    private Coroutine hitFeedbackRoutine;
    private Coroutine patternRoutine;
    private bool isDead = false;

    [Header("공격 패턴")]
    public GameObject enemyBulletPrefab; // 보스가 발사할 기본 총알 프리팹
    public float bulletSpeed = 3f;       // 총알 기본 속도 (BossBullet 스크립트 없을 때 폴백)
    public float firstPatternDelay = 1f; // 도착 후 첫 공격 대기 시간
    public float patternDelayMin = 1.0f; // 패턴 사이 최소 대기
    public float patternDelayMax = 2.0f; // 패턴 사이 최대 대기

    private ObjectManager objectManager; // ObjectManager 캐시

    [Header("패턴 2 (샷건) 세부 설정")]
    public float shotgunSpeedMultiplier = 0.7f; // 2번 패턴 총알 속도 배율 (기본보다 느리게)
    public float shotgunHalfAngle = 45f;        // 2번 패턴 펼침 각도 절반값 (클수록 더 넓게)

    [Header("패턴 3 (보스 기준 좌우 스윕) 세부 설정")]
    public float sweepPatternDuration = 2.2f;      // 3번 패턴 지속 시간
    public float sweepPatternInterval = 0.12f;     // 3번 패턴 발사 간격
    public float sweepPatternHalfAngle = 45f;      // 좌/우 최대 스윕 각도
    public float sweepPatternSpeedMultiplier = 0.9f; // 3번 패턴 총알 속도 배율
    public float sweepPatternCyclePerSec = 1.4f;   // 초당 좌->우->좌 스윕 왕복 속도

    private void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        objectManager = ObjectManager.Instance;

        // 보스 공격 루프 시작 (도착 전에는 내부에서 자동 대기)
        patternRoutine = StartCoroutine(PatternLoop());
    }

    private void Update()
    {
        // 목표 Y 좌표까지 하강
        if (!isArrived && transform.position.y > targetY)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }
        else
        {
            // 목표 지점 도착 처리 (한 번만 true)
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            isArrived = true;
        }
    }
    
    //데미지를 받는 함수
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHp -= damage;
        Debug.Log($"보스 체력 감소! 보스의 현재 체력:{currentHp}");

        // Hit 애니메이션 재생 (State 파라미터를 1로 설정)
        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
        }
        hitFeedbackRoutine = StartCoroutine(PlayHitFeedback());
        
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 피격 연출 코루틴
    // - State를 1로 설정하여 Boss_Hit 애니메이션 재생
    // - hitFeedbackDuration 시간 후 State를 0으로 복구하여 일반 애니메이션으로 돌아가기
    private IEnumerator PlayHitFeedback()
    {
        if (animator != null)
        {
            animator.SetInteger("State", 1); // Boss_Hit 애니메이션으로 전환
        }

        yield return new WaitForSeconds(Mathf.Max(0.01f, hitFeedbackDuration));

        if (animator != null)
        {
            animator.SetInteger("State", 0); // 일반 애니메이션으로 복구
        }

        hitFeedbackRoutine = null;
    }

    // 패턴 메인 루프
    // 1) 도착할 때까지 대기
    // 2) 4개 패턴 중 하나를 랜덤 선택
    // 3) 패턴 종료 후 랜덤 대기
    private IEnumerator PatternLoop()
    {
        while (!isArrived)
        {
            yield return null;
        }

        if (firstPatternDelay > 0f)
        {
            yield return new WaitForSeconds(firstPatternDelay);
        }

        while (true)
        {
            if (isDead)
            {
                yield break;
            }

            // 1,2,3,4 패턴 중 랜덤 선택
            int randomPattern = Random.Range(1, 5);

            string patternName = randomPattern switch
            {
                1 => "LinearFourShot",
                2 => "ShotgunSpread",
                3 => "FanSweepAttack",
                4 => "CircleFullAttack",
                _ => "Unknown"
            };
            Debug.Log($"[BossPattern] 실행 패턴: {randomPattern} ({patternName})");

            switch (randomPattern)
            {
                case 1: yield return StartCoroutine(LinearFourShot()); break;
                case 2: yield return StartCoroutine(ShotgunSpread()); break;
                case 3: yield return StartCoroutine(FanSweepAttack()); break;
                case 4: yield return StartCoroutine(CircleFullAttack()); break;
            }

            float wait = Random.Range(patternDelayMin, patternDelayMax);
            if (wait > 0f)
            {
                yield return new WaitForSeconds(wait);
            }
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Debug.Log("보스 처치!");

        if (patternRoutine != null)
        {
            StopCoroutine(patternRoutine);
            patternRoutine = null;
        }

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            hitFeedbackRoutine = null;
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (patternRoutine != null)
        {
            StopCoroutine(patternRoutine);
            patternRoutine = null;
        }

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            hitFeedbackRoutine = null;
        }
    }

    // 공통 총알 생성 함수
    // ObjectManager 풀에서 꺼내고, 풀이 꽉 찼을 때만 Instantiate 폴백
    private void SpawnBullet(Vector3 spawnPos, Vector2 direction, float speedMultiplier = 1f)
    {
        if (enemyBulletPrefab == null)
        {
            return;
        }

        // ObjectManager 풀에서 꺼내기 시도
        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        GameObject bullet = null;
        if (objectManager != null)
        {
            bullet = objectManager.MakeObj("EnemyBullet_0", spawnPos, Quaternion.identity);
        }

        // 풀이 꽉 찼거나 ObjectManager가 없으면 Instantiate 폴백
        if (bullet == null)
        {
            bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
        }

        Vector2 dir = direction == Vector2.zero ? Vector2.down : direction.normalized;

        // BossBullet 스크립트가 있으면 그 speed를 우선 사용
        BossBullet bossBullet = bullet.GetComponent<BossBullet>();
        float baseSpeed = bossBullet != null ? bossBullet.speed : bulletSpeed;
        float finalSpeed = baseSpeed * speedMultiplier;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * finalSpeed;
        }
    }

    // 1번 패턴: 일직선 발사 (홀수 → 40발 소모 시 종료)
    // 4발씩 가로 줄로 발사하며, 총 40발을 모두 쓸 때까지 반복
    private IEnumerator LinearFourShot()
    {
        int quota = 40;           // 홀수 패턴 총알 할당량
        int fired = 0;
        float gap = 0.7f;         // 총알 사이 가로 간격
        float waveInterval = 0.5f; // 웨이브 간격

        while (fired < quota)
        {
            // 한 줄에 4발씩 발사 (남은 할당량 이상 발사하지 않음)
            for (int i = 0; i < 4 && fired < quota; i++)
            {
                Vector3 spawnPos = transform.position + new Vector3((i - 1.5f) * gap, -0.5f, 0);
                SpawnBullet(spawnPos, Vector2.down);
                fired++;
            }

            if (fired < quota)
            {
                yield return new WaitForSeconds(waveInterval);
            }
        }

        yield return new WaitForSeconds(0.35f);
    }

    // 2번 패턴: 산탄(샷건) 발사 (짝수 → 50발 소모 시 종료)
    // 1회에 7발씩 퍼지게 발사하며, 총 50발을 모두 쓸 때까지 반복
    private IEnumerator ShotgunSpread()
    {
        int quota = 50;            // 짝수 패턴 총알 할당량
        int fired = 0;
        int bulletsPerWave = 7;    // 1회 발사 수
        float waveInterval = 0.5f; // 웨이브 간격

        while (fired < quota)
        {
            for (int i = 0; i < bulletsPerWave && fired < quota; i++)
            {
                float t = bulletsPerWave == 1 ? 0.5f : (float)i / (bulletsPerWave - 1);
                float angle = Mathf.Lerp(-shotgunHalfAngle, shotgunHalfAngle, t);
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
                SpawnBullet(transform.position + Vector3.down * 0.45f, dir, shotgunSpeedMultiplier);
                fired++;
            }

            if (fired < quota)
            {
                yield return new WaitForSeconds(waveInterval);
            }
        }

        yield return new WaitForSeconds(0.35f);
    }

    // 3번 패턴: 보스 기준 좌우 스윕 발사 (홀수 → 40발 소모 시 종료)
    // - 보스 위치에서 발사 각도를 좌우로 흔들면서 40발을 모두 쓸 때까지 반복
    private IEnumerator FanSweepAttack()
    {
        int quota = 40;  // 홀수 패턴 총알 할당량
        int fired = 0;
        float elapsed = 0f;

        while (fired < quota)
        {
            // PingPong으로 스윕 각도 계산 (좌 ↔ 우)
            float sweepT = Mathf.PingPong(elapsed * sweepPatternCyclePerSec, 1f);
            float angle = Mathf.Lerp(-sweepPatternHalfAngle, sweepPatternHalfAngle, sweepT);
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.down;

            SpawnBullet(transform.position + Vector3.down * 0.45f, dir, sweepPatternSpeedMultiplier);
            fired++;

            float wait = Mathf.Max(0.05f, sweepPatternInterval);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }

        yield return new WaitForSeconds(0.35f);
    }

    // 4번 패턴: 원형 전방위 발사 (짝수 → 50발 소모 시 종료)
    // - 보스 위치 기준 360도로 16발씩 발사하며, 총 50발을 모두 쓸 때까지 반복
    private IEnumerator CircleFullAttack()
    {
        int quota = 50;            // 짝수 패턴 총알 할당량
        int fired = 0;
        int bulletCount = 16;      // 1회 발사 수 (원형)
        float burstInterval = 0.5f; // 원형 발사 사이 대기 시간

        while (fired < quota)
        {
            for (int i = 0; i < bulletCount && fired < quota; i++)
            {
                float angle = i * (360f / bulletCount);
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
                SpawnBullet(transform.position, dir, 0.9f);
                fired++;
            }

            if (fired < quota)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        yield return new WaitForSeconds(0.35f);
    }
}
