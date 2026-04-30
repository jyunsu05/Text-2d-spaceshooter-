using System;
using UnityEngine;

// ──────────────────────────────────────────────────────────────
// Item
// 적 사망 시 드롭되는 아이템 오브젝트 스크립트
//
// 구현된 기능:
//   1. 생성 즉시 아래 방향으로 천천히 이동 (Rigidbody2D 속도 설정)
//   2. 매 프레임 추가로 아래로 이동 (transform.Translate 병행)
//   3. Y = -7f 이하로 내려가면 자동 삭제 (화면 밖 처리)
//   4. type 필드로 아이템 종류 구분
//      - "Coin"  : 점수 +1000
//      - "Power" : 점수 +500, 총알 파워업
//      - "Boom"  : 점수 +500, 스킬 카운트 +1
//      (실제 처리는 PlayerControll.cs의 HandleItemCollision에서 수행)
// ──────────────────────────────────────────────────────────────
public class Item : MonoBehaviour
{
    public string type;          // 아이템 종류 ("Coin" / "Power" / "Boom")
    private Rigidbody2D rigid;   // 물리 이동에 사용하는 Rigidbody2D 컴포넌트

    // ──────────────────────────────────────────────────────────────
    // 초기화
    // - Rigidbody2D 컴포넌트 캐싱
    // - 생성 직후 아래 방향으로 초기 속도 설정 (0.5f)
    // ──────────────────────────────────────────────────────────────

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.linearVelocity = Vector2.down * 0.5f; // 생성 즉시 아래 방향 속도 적용
    }

    // ──────────────────────────────────────────────────────────────
    // 매 프레임: 아래로 이동 + 화면 밖 삭제
    // - Rigidbody2D 속도와 별개로 transform.Translate도 함께 적용
    // - Y <= -7f 이면 오브젝트 삭제 (화면 하단 밖)
    // ──────────────────────────────────────────────────────────────

    private void Update()
    {
        // 아래 방향으로 이동 (프레임 독립적)
        transform.Translate(Vector2.down * 0.5f * Time.deltaTime);

        // 화면 아래로 완전히 벗어나면 삭제
        if (transform.position.y <= -7f)
        {
            Destroy(gameObject);
        }
    }
}
