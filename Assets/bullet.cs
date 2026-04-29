using UnityEngine;

public class bullet : MonoBehaviour
{
    // 총알 이동 속도 (Inspector에서 조절 가능)
    public float speed = 12f;

    // 총알이 이 Y 좌표를 넘어가면 자동 삭제 (화면 밖으로 너무 멀리 나가지 않게)
    public float destroyY = 8f;

    void Update()
    {
        // 화면 기준 위쪽 방향으로 계속 이동
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

        // Y 위치가 destroyY 이상이면 오브젝트 삭제
        if (transform.position.y >= destroyY)
        {
            Destroy(gameObject);
        }
    }
}