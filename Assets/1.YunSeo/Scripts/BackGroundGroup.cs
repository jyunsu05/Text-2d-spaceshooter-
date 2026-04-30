using System.Collections.Generic;
using UnityEngine;

// ──────────────────────────────────────────────────────────────
// BackGroundGroup
// 여러 배경 오브젝트를 무한 스크롤 만들 오브젝트풀링(재활용) 스크립트
//
// 구현된 기능:
//   1. 자식으로 등록된 배경 오브젝트들을 자동으로 Y축 정렬 (다리기식)
//   2. 가장 아래로 내려간 배경이 카메라 하단 경계 만큼을 벗어나면
//      가장 위에 있는 배경 바로 위로 재배치 (무한 루프 효과)
//   3. 배경 실제 이동은 각 BackGround 컴포넌트가 담당
//      (레이어별 시차효과는 BackGround.cs에서 설정)
// ──────────────────────────────────────────────────────────────
public class BackGroundGroup : MonoBehaviour
{
    private readonly List<Transform> backgrounds = new List<Transform>(); // 자식 배경 오브젝트 리스트
    private float backgroundHeight;  // 배경 한 장의 월드 높이 (Y 축 재배치 기준으로 사용)
    private Camera mainCamera;       // 화면 하단 경계 계산용 카메라 참조

    // ──────────────────────────────────────────────────────────────
    // 초기화
    // - 자식 오브젝트 수집
    // - 첫번째 배경의 높이를 기준으로 backgroundHeight 설정
    // - Y축 오름차순으로 정렬 후 간격 없이 배치
    // ──────────────────────────────────────────────────────────────

    private void Start()
    {
        mainCamera = Camera.main;

        // 자식 트랜스폼들을 리스트에 등록
        foreach (Transform child in transform)
        {
            backgrounds.Add(child);
        }

        // 자식이 없으면 발행 안 함
        if (backgrounds.Count == 0)
        {
            return;
        }

        // 첫번째 배경에서 높이값 캐싱 (부모가 없으면 발행 안 함)
        SpriteRenderer firstRenderer = backgrounds[0].GetComponent<SpriteRenderer>();
        if (firstRenderer == null)
        {
            return;
        }

        backgroundHeight = firstRenderer.bounds.size.y;

        // Y축 오름차순으로 정렬
        backgrounds.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        // 가장 아래 배경의 Y를 기준으로 등간격으로 배치
        float baseY = backgrounds[0].position.y;
        for (int i = 0; i < backgrounds.Count; i++)
        {
            Vector3 p = backgrounds[i].position;
            backgrounds[i].position = new Vector3(p.x, baseY + (backgroundHeight * i), p.z);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 매 프레임: 가장 아래로 내려간 배경을 재사용
    // - 리스트를 Y축 오름차순으로 다시 정렬
    // - 가장 아래 배경이 (카메라 하단 - 배경높이) 이하면
    //   가장 위 배경 바로 위로 재배치
    // ──────────────────────────────────────────────────────────────

    private void Update()
    {
        // 자식이 없거나 높이정보가 없으면 실행 안 함
        if (backgrounds.Count == 0 || backgroundHeight <= 0f)
        {
            return;
        }

        // 매 프레임 Y축으로 재정렬 (배경이 매 프레임 이동하므로 순서 변동)
        backgrounds.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        Transform lowest = backgrounds[0];                          // 가장 아래 배경
        Transform highest = backgrounds[backgrounds.Count - 1];    // 가장 위 배경

        // 카메라가 없으면 Y=0을 기준으로 사용
        float cameraBottomY = mainCamera != null
            ? mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y
            : 0f;

        // 가장 맨 아래 있는 배경이 기준선을 넘으면 맨 위로 재배치
        // (카메라 하단 Y - 배경 높이) 이하일 때 재활용
        if (lowest.position.y <= cameraBottomY - backgroundHeight)
        {
            Vector3 p = lowest.position;
            lowest.position = new Vector3(p.x, highest.position.y + backgroundHeight, p.z);
        }
    }
}
