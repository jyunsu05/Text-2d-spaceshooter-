using UnityEngine;

// ──────────────────────────────────────────────────────────────
// EnemyBullet
// Enemy_C가 시사하는 적 총알 스크립트
//
// 구현된 기능:
//   1. 발사 시점에 플레이어 위치를 1회만 조준 (홈밍이 아님)
//      - 이후에는 방향이 고정되어 직선 이동
//   2. 플레이어가 없으면 아래 방향으로 기본 이동
//   3. 화면 밖으로 나가면 자동 삭제
// ──────────────────────────────────────────────────────────────
public class EnemyBullet : MonoBehaviour
{
    public float speed = 12f;            // 총알 이동 속도 (Inspector에서 조절 가능)
    public float viewportMargin = 0.05f; // 화면 밖 삭제 여유값 (화면 경계보다 5% 더 나가야 삭제)

    private Camera mainCamera;           // 메인 카메라 참조
    private Vector3 moveDirection = Vector3.down; // 발사 방향 (기본값: 아래)
    private ObjectManager objectManager;

    // ──────────────────────────────────────────────────────────────
    // 초기화
    // - 카메라 참조 캐싱
    // - 플레이어 탐지 후 방향 고정
    //   (이 시점이후 moveDirection 값은 변하지 않음)
    // ──────────────────────────────────────────────────────────────

    void Start()
    {
        mainCamera = Camera.main;
        objectManager = ObjectManager.Instance;

        RetargetPlayer();
    }

    void OnEnable()
    {
        RetargetPlayer();
    }

    private void RetargetPlayer()
    {
        moveDirection = Vector3.down;

        // 플레이어를 찾아 발사 시점의 방향을 1회만 계산해 고정
        Transform target = FindPlayer();
        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 매 프레임: 고정된 moveDirection으로 직선 이동 + 화면 밖 삭제
    // ──────────────────────────────────────────────────────────────

    void Update()
    {
        // 고정된 방향으로 이동 (세계 좌표 기준)
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // 카메라가 없으면 Y=-8을 다닐 때 삭제로 대체
        if (mainCamera == null)
        {
            if (transform.position.y <= -8f)
            {
                ReturnSelf();
            }

            return;
        }

        // 월드 좌표를 뷰포트 좌표(0~1)로 변환
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // 화면 아래쪽 바깥으로 나가면 삭제
        if (viewportPosition.y < -viewportMargin)
        {
            ReturnSelf();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 플레이어 오브젝트 탐지
    // - 먼저 "Player" 태그로 찾고
    // - 태그가 없으면 이름으로 찾음 (Fallback)
    // ──────────────────────────────────────────────────────────────

    private Transform FindPlayer()
    {
        // 태그로 먼저 찾기 시도
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            // 태그가 없는 에디터 환경을 위한 Fallback: 이름으로 찾기
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            return player.transform;
        }

        // 태그/이름이 바뀐 경우를 대비한 최종 fallback
        PlayerControll playerController = FindAnyObjectByType<PlayerControll>();
        if (playerController != null)
        {
            return playerController.transform;
        }

        return null;
    }

    private void ReturnSelf()
    {
        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        if (objectManager != null)
        {
            objectManager.ReturnObj(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}