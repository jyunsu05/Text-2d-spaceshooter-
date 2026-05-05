using System;
using UnityEngine;

// ──────────────────────────────────────────────
// BossBullet
// 보스가 발사하는 총알
// - 회전 여부 설정 가능
// - 플레이어와 충돌 시 데미지 처리
// - 화면 밖으로 나가면 ObjectManager 풀로 반납
// ──────────────────────────────────────────────
public class BossBullet : MonoBehaviour
{
    public int damage = 1;
    public float speed = 3f; // 보스 총알 기본 속도
    public bool isRotate;
    public float viewportMargin = 0.1f; // 화면 밖 판정 여유값

    private Rigidbody2D rigid;
    private Camera mainCamera;
    private ObjectManager objectManager;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        objectManager = ObjectManager.Instance;
    }

    private void OnEnable()
    {
        // Rigidbody가 있으면 현재 방향은 유지하고 속도만 speed로 맞춤
        if (rigid != null)
        {
            Vector2 dir = rigid.linearVelocity.sqrMagnitude > 0.0001f
                ? rigid.linearVelocity.normalized
                : Vector2.down;

            rigid.linearVelocity = dir * speed;
        }
    }

    private void Update()
    {
        // Rigidbody가 없으면 아래 방향으로 직접 이동
        if (rigid == null)
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
        }

        if (isRotate)
        {
            transform.Rotate(Vector3.forward * 10 * Time.deltaTime);
        }

        CheckOutOfScreen();
    }

    // 화면 밖으로 나가면 풀로 반납
    private void CheckOutOfScreen()
    {
        if (mainCamera == null)
        {
            if (transform.position.y <= -8f || transform.position.y >= 8f)
            {
                ReturnSelf();
            }
            return;
        }

        Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);
        bool outOfBounds = vp.x < -viewportMargin || vp.x > 1f + viewportMargin
                        || vp.y < -viewportMargin || vp.y > 1f + viewportMargin;
        if (outOfBounds)
        {
            ReturnSelf();
        }
    }

    // ──────────────────────────────────────────────
    // 플레이어와 충돌 감지
    // ──────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControll player = other.GetComponent<PlayerControll>();
            if (player != null)
            {
                player.TakeDamage(damage, other);
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
