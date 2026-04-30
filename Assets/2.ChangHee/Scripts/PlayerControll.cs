using UnityEngine;

/// <summary>
/// 플레이어 이동, 발사, 화면 범위 제한, 파워업을 담당하는 컨트롤러
/// </summary>
public class PlayerControll : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // 인스펙터 설정값
    // ──────────────────────────────────────────────

    [Header("이동 설정")]
    [Tooltip("플레이어 이동 속도")]
    public float speed = 5f;

    [Header("총알 단계")]
    [Range(1, 3)]
    [Tooltip("현재 총알 파워 단계 (1~3)")]
    public int bulletPower = 1;

    [Header("총알 프리팹")]
    [Tooltip("기본(소형) 총알 프리팹")]
    public GameObject smallBulletPrefab;
    [Tooltip("대형 총알 프리팹 (파워3 중앙 발사용)")]
    public GameObject largeBulletPrefab;

    [Header("총알 발사 위치")]
    [Tooltip("총알이 생성될 기준 위치")]
    public Transform firePoint;

    [Header("파워2 좌우 간격")]
    [Tooltip("파워2일 때 좌우 총알 간격 (단위: 유닛)")]
    public float power2Offset = 0.3f;

    [Header("파워3 좌우 간격")]
    [Tooltip("파워3일 때 좌우 소형 총알 간격 (단위: 유닛)")]
    public float power3Offset = 0.4f;

    [Header("연사 설정")]
    [Tooltip("총알 발사 간격 (초). 낮을수록 빠른 연사")]
    public float shotInterval = 0.12f;

    // ──────────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────────

    private float nextShotTime;   // 다음 발사 가능 시각
    private Camera mainCamera;
    private float halfWidth;      // 스프라이트 가로 절반 크기 (화면 경계 계산용)
    private float halfHeight;     // 스프라이트 세로 절반 크기 (화면 경계 계산용)
    private Animator animator;

    // ──────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────

    void Start()
    {
        mainCamera = Camera.main;

        // 화면 경계 클램핑에 사용할 스프라이트 크기 캐싱
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfWidth  = sr.bounds.extents.x;
            halfHeight = sr.bounds.extents.y;
        }

        animator = GetComponent<Animator>();
    }

    // ──────────────────────────────────────────────
    // 매 프레임 업데이트
    // ──────────────────────────────────────────────

    void Update()
    {
        Move();
        ShootInput();
        ClampToScreen();
    }

    // ──────────────────────────────────────────────
    // 이동 처리
    // WASD / 방향키 입력을 받아 플레이어를 이동시키고
    // 애니메이터 State 파라미터를 갱신한다.
    //   0 = 정면, 1 = 좌, 2 = 우
    // ──────────────────────────────────────────────

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        if (animator == null) return;

        int state = moveX < 0 ? 1 : moveX > 0 ? 2 : 0;
        animator.SetInteger("State", state);
    }

    // ──────────────────────────────────────────────
    // 발사 입력 처리
    // 마우스 왼쪽 버튼을 누르고 있는 동안 shotInterval 간격으로 연사한다.
    // ──────────────────────────────────────────────

    void ShootInput()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + shotInterval;
        }
    }

    // ──────────────────────────────────────────────
    // 총알 생성 (파워 단계별)
    //   파워1 : 중앙에 소형 총알 1발
    //   파워2 : 중앙 기준 좌우에 소형 총알 2발 (간격: power2Offset)
    //   파워3 : 중앙에 대형 총알 1발 + 좌우에 소형 총알 2발 (간격: power3Offset)
    // ──────────────────────────────────────────────

    void Shoot()
    {
        switch (bulletPower)
        {
            case 1:
                // 중앙 소형 총알 1발
                Instantiate(smallBulletPrefab, firePoint.position, firePoint.rotation);
                break;

            case 2:
                // 좌우 소형 총알 2발
                SpawnBullet(smallBulletPrefab, -power2Offset);
                SpawnBullet(smallBulletPrefab,  power2Offset);
                break;

            case 3:
                // 중앙 대형 총알 + 좌우 소형 총알
                Instantiate(largeBulletPrefab, firePoint.position, firePoint.rotation);
                SpawnBullet(smallBulletPrefab, -power3Offset);
                SpawnBullet(smallBulletPrefab,  power3Offset);
                break;
        }
    }

    /// <summary>
    /// firePoint 기준으로 X 오프셋을 적용한 위치에 총알을 생성한다.
    /// </summary>
    /// <param name="prefab">생성할 총알 프리팹</param>
    /// <param name="xOffset">firePoint.x 기준 오프셋 (음수=왼쪽, 양수=오른쪽)</param>
    void SpawnBullet(GameObject prefab, float xOffset)
    {
        Vector3 spawnPos = firePoint.position + new Vector3(xOffset, 0f, 0f);
        Instantiate(prefab, spawnPos, firePoint.rotation);
    }

    // ──────────────────────────────────────────────
    // 파워업
    // 외부(아이템 등)에서 호출하면 bulletPower를 1 증가시킨다.
    // 최대 3단계까지만 증가한다.
    // ──────────────────────────────────────────────

    public void PowerUp()
    {
        bulletPower = Mathf.Min(bulletPower + 1, 3);
    }

    // ──────────────────────────────────────────────
    // 화면 경계 클램핑
    // 플레이어가 카메라 뷰포트 밖으로 나가지 않도록 위치를 제한한다.
    // ──────────────────────────────────────────────

    void ClampToScreen()
    {
        if (mainCamera == null) return;

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x + halfWidth,  max.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, min.y + halfHeight, max.y - halfHeight);
        transform.position = pos;
    }
}