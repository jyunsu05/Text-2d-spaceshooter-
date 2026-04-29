using UnityEngine;

public class playercontroll : MonoBehaviour
{
    // 플레이어 이동 속도 (Inspector에서 조절 가능)
    public float speed = 5f;

    // 화면 경계를 계산할 때 사용할 카메라
    private Camera mainCamera;

    // 플레이어 스프라이트의 절반 크기 (화면 경계 계산에 사용)
    private float halfWidth;
    private float halfHeight;

    // 애니메이션을 제어할 Animator 컴포넌트
    private Animator animator;

    void Start()
    {
        // 메인 카메라를 가져옴
        mainCamera = Camera.main;

        // SpriteRenderer에서 스프라이트 크기의 절반을 구함
        // 플레이어가 화면 밖으로 나가지 않도록 경계 계산에 사용
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        halfWidth  = sr.bounds.extents.x; // 가로 절반 크기
        halfHeight = sr.bounds.extents.y; // 세로 절반 크기

        // Animator 컴포넌트를 가져옴 (애니메이션 전환에 사용)
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ─── 입력 받기 ───────────────────────────────────────────
        // GetAxisRaw : -1, 0, 1 중 하나를 반환 (대각선도 자동 처리됨)
        // Horizontal : 좌(-1) / 우(+1)
        // Vertical   : 아래(-1) / 위(+1)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // ─── 애니메이션 상태 업데이트 ───────────────────────────
        // moveX 값에 따라 애니메이션 전환
        // State: 0=Idle, 1=Left, 2=Right
        if (moveX < 0)
        {
            // 왼쪽 이동
            animator.SetInteger("State", 1);
        }
        else if (moveX > 0)
        {
            // 오른쪽 이동
            animator.SetInteger("State", 2);
        }
        else
        {
            // 입력 없음 (정지 상태)
            animator.SetInteger("State", 0);
        }

        // ─── 이동 방향 벡터 만들기 ───────────────────────────────
        // normalized : 대각선 이동 시 속도가 빨라지지 않도록 크기를 1로 맞춤
        Vector2 direction = new Vector2(moveX, moveY).normalized;

        // ─── 실제 이동 적용 ──────────────────────────────────────
        // Time.deltaTime : 프레임 속도에 관계없이 일정한 속도로 이동하게 함
        transform.Translate(direction * speed * Time.deltaTime);

        // ─── 화면 경계 처리 ──────────────────────────────────────
        ClampToScreen();
    }

    // 플레이어가 화면 밖으로 나가지 못하도록 위치를 제한하는 함수
    void ClampToScreen()
    {
        // 현재 플레이어 위치
        Vector3 pos = transform.position;

        // 카메라 기준으로 화면의 왼쪽 아래(0,0)와 오른쪽 위(1,1)를
        // 월드 좌표로 변환하여 화면 경계값을 구함
        Vector3 minScreen = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 maxScreen = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // 플레이어 위치를 화면 경계 안으로 제한 (스프라이트 절반 크기 고려)
        pos.x = Mathf.Clamp(pos.x, minScreen.x + halfWidth,  maxScreen.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, minScreen.y + halfHeight, maxScreen.y - halfHeight);

        // 제한된 위치를 다시 적용
        transform.position = pos;
    }
}
