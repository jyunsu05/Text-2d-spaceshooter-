using UnityEngine;

public class bullet : MonoBehaviour
{
    // 총알 이동 속도 (Inspector에서 조절 가능)
    public float speed = 12f;

    // 총알 생존 시간 (너무 멀리 가면 자동 삭제)
    public float lifeTime = 3f;

    void Start()
    {
        // 생성 후 일정 시간이 지나면 자동으로 삭제
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 화면 기준 위쪽 방향으로 계속 이동
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }
}