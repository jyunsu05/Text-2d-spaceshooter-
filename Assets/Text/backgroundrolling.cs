using UnityEngine;

// 배경 무한 스크롤 스크립트
// 사용법: 이 스크립트를 배경 오브젝트 2개에 모두 붙이세요.
//         배경 2개를 세로로 나란히 배치해두면 끊김 없이 무한 반복됩니다.
public class backgroundrolling : MonoBehaviour
{
    public float speed = 2f;        // 배경이 내려오는 속도 (Inspector에서 조절 가능)

    private float backgroundHeight; // 배경 이미지 1장의 높이

    void Start()
    {
        // SpriteRenderer에서 이 배경 이미지의 실제 높이를 가져옴
        backgroundHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        // 매 프레임마다 배경을 아래로 이동 (플레이어가 위로 날아가는 느낌)
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 배경이 화면 아래로 완전히 사라지면 위로 점프
        // (배경 2장 높이만큼 올려서 다른 배경 바로 위에 붙임)
        if (transform.position.y < -backgroundHeight)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + backgroundHeight * 2f,
                transform.position.z
            );
        }
    }
}
