using System.Collections;
using UnityEngine;

// ──────────────────────────────────────────────────────────────
// Enemy
// 적 공통 스크립트 (Enemy_A, Enemy_B, Enemy_C 전부 사용)
//
// 구현된 기능:
//   1. 아래 방향 이동 및 화면 밖 삭제
//   2. HP 시스템 - 플레이어 총알에 피격 시 데미지 적용
//   3. 피격 연출 - 0.1초 동안 피격 스프라이트로 교체 후 원상복귀
//   4. 사망 처리 - 중복 호출 방지 (isDead 플래그)
//   5. 아이템 드롭 - 사망 시 확률에 따라 Coin/Power/Boom 소환
//      None 30% / Coin 30% / Power 20% / Boom 20%
//   6. 총알 발사 (Enemy_C 전용)
//      - 화면 진입 후 shotInterval 간격으로 EnemyPoint_1/2에서 반복 발사
// ──────────────────────────────────────────────────────────────
public class Enemy : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // 인스펙터 설정값
    // ──────────────────────────────────────────────

    [SerializeField] private float fallSpeed = 3f;           // 하강 속도 (A=3, B=5, C=1.5)
    [SerializeField] private float destroyY = -6f;           // 이 Y 좌표 아래로 내려가면 삭제
    [SerializeField] private Vector2 moveDirection = Vector2.down; // 이동 방향
    [SerializeField] private GameObject enemyBulletPrefab;   // Enemy_C 전용 총알 프리팹
    [SerializeField] private float firstShotDelay = 1.2f;    // 첫 발사까지 대기 시간
    [SerializeField] private float shotInterval = 2f;        // 발사 간격 (초)
    [SerializeField] private int playerBulletDamage = 5;     // 플레이어 총알 기본 데미지 (Bullet 컴포넌트 우선)
    [SerializeField] private float hitFeedbackDuration = 0.1f; // 피격 스프라이트 유지 시간
    [SerializeField] private int maxHp = 3;                  // 최대 HP (인스펙터에서 적별로 설정)
    [SerializeField] private SpriteRenderer spriteRenderer;  // 스프라이트 렌더러
    [SerializeField] private Sprite[] sprites;               // [0]=기본 스프라이트, [1]=피격 스프라이트
    [SerializeField] private GameObject itemCoinPrefab;      // Coin 아이템 프리팹
    [SerializeField] private GameObject itemPowerPrefab;     // Power 아이템 프리팹
    [SerializeField] private GameObject itemBoomPrefab;      // Boom 아이템 프리팹

    // ──────────────────────────────────────────────
    // 내부 상태값
    // ──────────────────────────────────────────────

    private int currentHp;                  // 현재 HP
    private Transform enemyPoint1;          // Enemy_C 총알 발사 위치 1
    private Transform enemyPoint2;          // Enemy_C 총알 발사 위치 2
    private bool canShoot;                  // 총알 발사 가능 여부 (Enemy_C만 true)
    private Coroutine hitFeedbackRoutine;   // 피격 연출 코루틴 참조 (중복 실행 방지용)
    private Sprite normalSprite;            // 평상시 스프라이트
    private Sprite hitSprite;               // 피격 시 스프라이트
    private bool isDead = false;            // 사망 중복 처리 방지 플래그

    // ──────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────

    void Awake()
    {
        // HP 초기화
        currentHp = maxHp;

        // SpriteRenderer 자동 캐싱
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 기본/피격 스프라이트 초기화
        InitializeSprites();

        // Enemy_C 이름으로 시작하는 오브젝트만 총알 발사 가능
        canShoot = gameObject.name.StartsWith("Enemy_C");

        if (canShoot)
        {
            enemyPoint1 = transform.Find("EnemyPoint_1");
            enemyPoint2 = transform.Find("EnemyPoint_2");
        }
    }

    private void Start()
    {
        // Enemy_C가 아니거나 필요한 참조가 없으면 발사 루프 시작 안 함
        if (!canShoot || enemyBulletPrefab == null || enemyPoint1 == null || enemyPoint2 == null)
        {
            return;
        }

        StartCoroutine(ShootLoop());
    }

    // ──────────────────────────────────────────────
    // 매 프레임: 이동 및 화면 밖 삭제
    // ──────────────────────────────────────────────

    void Update()
    {
        // moveDirection 방향으로 fallSpeed 속도로 이동 (월드 좌표 기준)
        Vector3 movement = (Vector3)moveDirection.normalized * fallSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 화면 아래로 벗어나면 삭제
        if (transform.position.y <= destroyY)
        {
            Destroy(gameObject);
        }
    }

    // ──────────────────────────────────────────────
    // 이동 방향 설정 (SpawnManager에서 호출)
    // ──────────────────────────────────────────────

    public void SetMoveDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            moveDirection = Vector2.down;
            return;
        }

        moveDirection = direction.normalized;
    }

    // ──────────────────────────────────────────────
    // 피격 처리
    // - HP 감소 후 피격 연출 코루틴 실행
    // - HP가 0 이하면 사망 처리
    // ──────────────────────────────────────────────

    public void Hit(int damage = 1)
    {
        int previousHp = currentHp;
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        Debug.Log($"{gameObject.name} hit! Damage: {damage}, HP: {previousHp} -> {currentHp}");

        if (currentHp > 0)
        {
            // 연속 피격 시 이전 연출 취소 후 새로 시작
            if (hitFeedbackRoutine != null)
            {
                StopCoroutine(hitFeedbackRoutine);
            }

            hitFeedbackRoutine = StartCoroutine(PlayHitFeedback());
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // ──────────────────────────────────────────────
    // 충돌 감지
    // - playerBullet 태그와 충돌 시 데미지 적용
    // - Bullet 컴포넌트의 damage 값을 우선 사용
    //   (없으면 playerBulletDamage 기본값 사용)
    // ──────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("playerBullet"))
        {
            return;
        }

        // Bullet 컴포넌트가 있으면 해당 데미지 값 사용
        int appliedDamage = playerBulletDamage;
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            appliedDamage = bullet.damage;
        }

        int previousHp = currentHp;
        Hit(appliedDamage);
        Debug.Log($"({previousHp} / {maxHp}) = 플레이어 총알 : {other.gameObject.name}");
        Destroy(other.gameObject);
    }

    // ──────────────────────────────────────────────
    // 스프라이트 초기화
    // - sprites[0] = 기본 스프라이트
    // - sprites[1] = 피격 스프라이트
    // ──────────────────────────────────────────────

    private void InitializeSprites()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (sprites != null && sprites.Length > 0)
        {
            normalSprite = sprites[0];
            spriteRenderer.sprite = normalSprite;

            if (sprites.Length > 1)
            {
                hitSprite = sprites[1];
            }
        }
        else
        {
            // sprites 배열이 없으면 현재 붙어있는 스프라이트를 기본으로 사용
            normalSprite = spriteRenderer.sprite;
        }
    }

    // ──────────────────────────────────────────────
    // 피격 연출 코루틴
    // - hitFeedbackDuration(0.1초) 동안 피격 스프라이트로 교체
    // - 이후 원래 스프라이트로 복귀
    // - hitSprite가 없으면 빨간색 틴트로 대체
    // ──────────────────────────────────────────────

    private IEnumerator PlayHitFeedback()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        if (hitSprite != null)
        {
            spriteRenderer.sprite = hitSprite;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }

        yield return new WaitForSeconds(Mathf.Max(0.01f, hitFeedbackDuration));

        // 원래 스프라이트 및 색상으로 복귀
        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
        spriteRenderer.color = Color.white;
        hitFeedbackRoutine = null;
    }

    // ──────────────────────────────────────────────
    // 사망 처리
    // - isDead 플래그로 중복 호출 방지
    //   (파워3 총알 3개 동시 충돌 시 아이템 중복 드롭 방지)
    // - 아이템 드롭 후 오브젝트 삭제
    // ──────────────────────────────────────────────

    [Header("적 개별 점수 설정")]
    [Tooltip("이 적이 죽을 때 오를 점수")] 
    public int enemyScore = 100;

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        // Inspector에서 지정한 enemyScore만큼 점수 증가
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddScore(enemyScore);
            Debug.Log($"[점수] {gameObject.name} 처치 → +{enemyScore}점 / 현재 총점: {UIManager.Instance.player?.CurrentScore}");
        }

        DropItem();
        Destroy(gameObject);
    }

    // ──────────────────────────────────────────────
    // 아이템 드롭
    // - 사망 시 1회만 호출됨
    // - 확률 테이블:
    //     0.00 ~ 0.30 : None  (30%)
    //     0.30 ~ 0.60 : Coin  (30%)
    //     0.60 ~ 0.80 : Power (20%)
    //     0.80 ~ 1.00 : Boom  (20%)
    // ──────────────────────────────────────────────

    private void DropItem()
    {
        float rand = Random.value; // 0.0 ~ 1.0
        GameObject prefab = null;

        if (rand < 0.30f)           // 0% ~ 30% : None
        {
            return;
        }
        else if (rand < 0.60f)      // 30% ~ 60% : Coin
        {
            prefab = itemCoinPrefab;
        }
        else if (rand < 0.80f)      // 60% ~ 80% : Power
        {
            prefab = itemPowerPrefab;
        }
        else                        // 80% ~ 100% : Boom
        {
            prefab = itemBoomPrefab;
        }

        if (prefab != null)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    // ──────────────────────────────────────────────
    // Enemy_C 전용: 총알 반복 발사 코루틴
    // - firstShotDelay 후 화면 진입 대기
    // - 화면 안에 있는 동안 shotInterval 간격으로
    //   EnemyPoint_1, EnemyPoint_2에서 각 1발 발사
    // - 화면 밖으로 나가면 발사 중지
    // ──────────────────────────────────────────────

    private IEnumerator ShootLoop()
    {
        if (firstShotDelay > 0f)
        {
            yield return new WaitForSeconds(firstShotDelay);
        }

        // 화면에 진입할 때까지 대기
        while (!IsInsideMainCameraView())
        {
            yield return new WaitForSeconds(0.05f);
        }

        // 화면 안에 있는 동안 계속 발사
        while (IsInsideMainCameraView())
        {
            ShootFromPoint(enemyPoint1);
            ShootFromPoint(enemyPoint2);
            yield return new WaitForSeconds(shotInterval);
        }
    }

    // 지정한 발사 위치에서 적 총알 생성
    private void ShootFromPoint(Transform firePoint)
    {
        if (firePoint == null || enemyBulletPrefab == null)
        {
            return;
        }

        Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
    }

    // 현재 오브젝트가 메인 카메라 뷰포트 안에 있는지 확인
    private bool IsInsideMainCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return true;
        }

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.z > 0f
               && viewportPos.x >= 0f && viewportPos.x <= 1f
               && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }
}
