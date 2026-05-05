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
    private ObjectManager objectManager;

    void Start()
    {
        mainCamera = Camera.main;
        objectManager = ObjectManager.Instance;
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
                ReturnSelf();
            }

            return;
        }

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPosition.y > 1f + viewportMargin)
        {
            ReturnSelf();
        }
    }

    // ──────────────────────────────────────────────
    // 충돌 감지
    // Enemy 또는 Boss 태그를 가진 오브젝트와 닿으면 데미지 처리
    // ──────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Hit(damage);
            }

            ReturnSelf();
        }
        else if (other.CompareTag("Boss"))
        {
            BossController boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            ReturnSelf();
        }
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