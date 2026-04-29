using UnityEngine;

// ──────────────────────────────────────────────
// PlayerControll
// 플레이어 이동 + 총알 3단계 발사 제어
//
// 발사 규칙:
// 1단계: CenterSpawn 위치에서 Left_Bullet 1발 발사
// 2단계: LeftSpawn / RightSpawn 위치에서 좌우 2발 발사
// 3단계: LeftSpawn / CenterSpawn / RightSpawn 위치에서 3발 발사
//
// 데미지 처리는 Bullet.cs 또는 팀 통합 시스템에서 처리한다.
// 이 스크립트는 "어떤 총알을 어디서 생성할지"만 담당한다.
// ──────────────────────────────────────────────
public class PlayerControll : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 5f;

    [Header("총알 단계")]
    [Range(1, 3)]
    public int bulletPower = 1;

    [Header("총알 프리팹")]
    public GameObject leftBulletPrefab;
    public GameObject centerBulletPrefab;
    public GameObject rightBulletPrefab;

    [Header("총알 발사 위치")]
    public Transform leftSpawn;
    public Transform centerSpawn;
    public Transform rightSpawn;

    [Header("연사 설정")]
    public float shotInterval = 0.12f;

    private float nextShotTime;

    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;
    private Animator animator;

    void Start()
    {
        mainCamera = Camera.main;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfWidth = sr.bounds.extents.x;
            halfHeight = sr.bounds.extents.y;
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        ShootInput();
        ClampToScreen();
    }

    // ──────────────────────────────────────────────
    // 플레이어 이동 처리
    // ──────────────────────────────────────────────
    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        if (animator != null)
        {
            if (moveX < 0)
            {
                animator.SetInteger("State", 1);
            }
            else if (moveX > 0)
            {
                animator.SetInteger("State", 2);
            }
            else
            {
                animator.SetInteger("State", 0);
            }
        }
    }

    // ──────────────────────────────────────────────
    // Space 키 발사 입력 처리
    // ──────────────────────────────────────────────
    void ShootInput()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + shotInterval;
        }
    }

    // ──────────────────────────────────────────────
    // 총알 단계별 발사 처리
    // ──────────────────────────────────────────────
    void Shoot()
    {
        if (bulletPower == 1)
        {
            // 1단계: 중앙 위치에서 Left_Bullet 1발
            Fire(leftBulletPrefab, centerSpawn);
        }
        else if (bulletPower == 2)
        {
            // 2단계: 좌우 2발
            Fire(leftBulletPrefab, leftSpawn);
            Fire(rightBulletPrefab, rightSpawn);
        }
        else if (bulletPower == 3)
        {
            // 3단계: 좌 / 중 / 우 3발
            Fire(leftBulletPrefab, leftSpawn);
            Fire(centerBulletPrefab, centerSpawn);
            Fire(rightBulletPrefab, rightSpawn);
        }
    }

    // ──────────────────────────────────────────────
    // 실제 총알 생성 함수
    // prefab = 만들 총알
    // spawnPoint = 총알이 나올 위치
    // ──────────────────────────────────────────────
    void Fire(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null)
        {
            Debug.LogWarning("총알 프리팹 또는 발사 위치가 연결되지 않았습니다.");
            return;
        }

        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }

    // ──────────────────────────────────────────────
    // 파워업 함수
    // 아이템을 먹었을 때 호출하면 bulletPower가 1단계 증가한다.
    // 최대 3단계까지만 증가한다.
    // ──────────────────────────────────────────────
    public void PowerUp()
    {
        bulletPower++;

        if (bulletPower > 3)
        {
            bulletPower = 3;
        }
    }

    // ──────────────────────────────────────────────
    // 화면 밖으로 나가지 않도록 제한
    // ──────────────────────────────────────────────
    void ClampToScreen()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 pos = transform.position;

        Vector3 minScreen = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreen = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        pos.x = Mathf.Clamp(pos.x, minScreen.x + halfWidth, maxScreen.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, minScreen.y + halfHeight, maxScreen.y - halfHeight);

        transform.position = pos;
    }
}