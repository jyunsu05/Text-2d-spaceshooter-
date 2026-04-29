using UnityEngine;

// ──────────────────────────────────────────────
// Bullet
// 플레이어 총알 이동 + 화면 밖 삭제 + 충돌 감지
//
// 담당 범위:
// 1. 총알이 위로 이동
// 2. 화면 밖으로 나가면 삭제
// 3. 적과 충돌했는지 감지
// 4. 데미지/점수 처리는 팀 통합 시 연결할 수 있게 구멍만 남김
// ──────────────────────────────────────────────
public class Bullet : MonoBehaviour
{
    // 총알 이동 속도
    public float speed = 12f;

    // 카메라 밖으로 나갔는지 판단할 때 사용할 여유값
    public float viewportMargin = 0.05f;

    // 팀장이 나중에 데미지 시스템과 연결할 수 있는 값
    public int damage = 1;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 총알을 위쪽으로 이동
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

        CheckOutOfScreen();
    }

    // ──────────────────────────────────────────────
    // 화면 밖으로 나가면 총알 삭제
    // ──────────────────────────────────────────────
    void CheckOutOfScreen()
    {
        if (mainCamera == null)
        {
            if (transform.position.y >= 8f)
            {
                Destroy(gameObject);
            }

            return;
        }

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPosition.y > 1f + viewportMargin)
        {
            Destroy(gameObject);
        }
    }

    // ──────────────────────────────────────────────
    // 충돌 감지
    //
    // Enemy 태그를 가진 오브젝트와 닿으면 실행된다.
    // 실제 데미지 처리/점수 처리는 팀장이 통합할 때 연결하면 된다.
    // ──────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // TODO:
            // 팀 통합 시 여기에서 적 체력 감소, 점수 증가, 이펙트 등을 연결한다.
            //
            // 예시:
            // Enemy enemy = other.GetComponent<Enemy>();
            // if (enemy != null)
            // {
            //     enemy.TakeDamage(damage);
            // }

            Destroy(gameObject);
        }
    }
}