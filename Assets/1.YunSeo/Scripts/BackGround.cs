using UnityEngine;

// ──────────────────────────────────────────────────────────────
// BackGround
// 배경 스크롤의 스크롤 속도를 레이어별로 다르게 설정하는 스크립트
//
// 구현된 기능:
//   1. parallaxLayer 값에 따라 이동 속도를 다르게 적용 (시차 효과)
//      - Layer 1 (느림): 배경 깊은 원거리 물체 느리게 이동
//      - Layer 2 (기본): 일반 배경 속도
//      - Layer 3 (빠름): 배경 가까운 물체 빠르게 이동
//   2. 실제 이동은 BackGroundGroup이 제어하지 않음
//      - 각 배경이 스스로 transform.Translate로 내려감
// ──────────────────────────────────────────────────────────────
public class BackGround : MonoBehaviour
{
    [SerializeField] private float speed = 1f;                    // 기본 이동 속도 (레이어 2 기준)
    [SerializeField, Range(1, 3)] private int parallaxLayer = 2; // 시차 레이어: 1=느림, 2=기본, 3=빠름

    // ──────────────────────────────────────────────────────────────
    // 레이어별 속도 배율 반환
    // - Layer 1: 표준 속도의 60% (느리게 이동 → 멀 거리 느낌)
    // - Layer 2: 표준 속도 100%
    // - Layer 3: 표준 속도의 160% (빠르게 이동 → 가까운 느낌)
    // ──────────────────────────────────────────────────────────────

    private float GetParallaxMultiplier()
    {
        if (parallaxLayer == 1) return 0.6f;  // 느린 배경층
        if (parallaxLayer == 3) return 1.6f;  // 빠른 배경층
        return 1f;                            // 기본 배경층
    }

    // ──────────────────────────────────────────────────────────────
    // 매 프레임: 레이어별 속도로 아래로 이동
    // ──────────────────────────────────────────────────────────────

    void Update()
    {
        // 기본 속도 × 레이어 배율로 실제 이동 속도 계산
        float parallaxSpeed = speed * GetParallaxMultiplier();
        transform.Translate(Vector3.down * parallaxSpeed * Time.deltaTime);
    }
}
