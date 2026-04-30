using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    // 총알 이동 속도 (Inspector에서 조절 가능)
    public float speed = 12f;

    // 카메라 밖으로 나갔는지 판단할 때 사용할 여유값
    public float viewportMargin = 0.05f;

    // 메인 카메라 참조
    private Camera mainCamera;

    // 발사 방향 (발사 시점에 플레이어 방향으로 고정)
    private Vector3 moveDirection = Vector3.down;

    void Start()
    {
        mainCamera = Camera.main;

        Transform target = FindPlayer();
        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // 카메라가 없으면 마지막 안전장치로 Y=-8을 넘어갈 때 삭제
        if (mainCamera == null)
        {
            if (transform.position.y <= -8f)
            {
                Destroy(gameObject);
            }

            return;
        }

        // 월드 좌표를 뷰포트 좌표(0~1)로 변환
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // 화면 아래쪽 바깥으로 나가면 삭제
        if (viewportPosition.y < -viewportMargin)
        {
            Destroy(gameObject);
        }
    }

    private Transform FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            // 태그 미설정 프로젝트를 위한 폴백
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            return player.transform;
        }

        return null;
    }
}