using UnityEngine;

public class bullet : MonoBehaviour
{
    // 총알 이동 속도 (Inspector에서 조절 가능)
    public float speed = 12f;

    // 카메라 밖으로 나갔는지 판단할 때 사용할 여유값
    public float viewportMargin = 0.05f;

    // 메인 카메라 참조
    private Camera mainCamera;

    void Start()
    {
        // 메인 카메라를 캐싱해서 매 프레임 탐색 비용을 줄임
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 화면 기준 위쪽 방향으로 계속 이동
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

        // 카메라가 없으면 마지막 안전장치로 Y=8을 넘어갈 때 삭제
        if (mainCamera == null)
        {
            if (transform.position.y >= 8f)
            {
                Destroy(gameObject);
            }

            return;
        }

        // 월드 좌표를 뷰포트 좌표(0~1)로 변환
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // 화면 위쪽 바깥으로 나가면 삭제
        if (viewportPosition.y > 1f + viewportMargin)
        {
            Destroy(gameObject);
        }
    }
}