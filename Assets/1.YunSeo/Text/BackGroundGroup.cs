using System.Collections.Generic;
using UnityEngine;

public class BackGroundGroup : MonoBehaviour
{
    // 백그라운드 안에있는 오브젝트가 1를 기준으로 위로 3개가 되게 만들기
    // 백그라운드가 카메라 하단경계 배경높이만큼 내려가면 재활용
    private readonly List<Transform> backgrounds = new List<Transform>();
    private float backgroundHeight;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        foreach (Transform child in transform)
        {
            backgrounds.Add(child);
        }

        if (backgrounds.Count == 0)
        {
            return;
        }

        SpriteRenderer firstRenderer = backgrounds[0].GetComponent<SpriteRenderer>();
        if (firstRenderer == null)
        {
            return;
        }

        backgroundHeight = firstRenderer.bounds.size.y;

        backgrounds.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        float baseY = backgrounds[0].position.y;
        for (int i = 0; i < backgrounds.Count; i++)
        {
            Vector3 p = backgrounds[i].position;
            backgrounds[i].position = new Vector3(p.x, baseY + (backgroundHeight * i), p.z);
        }
    }

    private void Update()
    {
        if (backgrounds.Count == 0 || backgroundHeight <= 0f)
        {
            return;
        }

        backgrounds.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        Transform lowest = backgrounds[0];
        Transform highest = backgrounds[backgrounds.Count - 1];

        float cameraBottomY = mainCamera != null
            ? mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y
            : 0f;

        // 가장 맨 아래 있는 배경이 기준선을 넘으면 맨 위로 재배치
        if (lowest.position.y <= cameraBottomY - backgroundHeight)
        {
            Vector3 p = lowest.position;
            lowest.position = new Vector3(p.x, highest.position.y + backgroundHeight, p.z);
        }
    }
}
