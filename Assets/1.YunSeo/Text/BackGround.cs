using UnityEngine;

public class BackGround : MonoBehaviour
{
    [SerializeField] private float speed = 1f; // 레이어 2의 기본 속도
    [SerializeField, Range(1, 3)] private int parallaxLayer = 2; // 1: 느림, 2: 기본, 3: 빠름

    private float GetParallaxMultiplier()
    {
        if (parallaxLayer == 1) return 0.6f;
        if (parallaxLayer == 3) return 1.6f;
        return 1f;
    }

    void Update()
    {
        float parallaxSpeed = speed * GetParallaxMultiplier();
        transform.Translate(Vector3.down * parallaxSpeed * Time.deltaTime);
    }
}
