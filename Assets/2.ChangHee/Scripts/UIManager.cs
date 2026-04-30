using TMPro;
using UnityEngine;


using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스 (Inspector 연결 실수 방지)
    public static UIManager Instance { get; private set; }

    [Header("플레이어 참조 (반드시 연결)")]
    public PlayerControll player;

    [Header("점수 텍스트 (반드시 연결)")]
    public TextMeshProUGUI scoreText;

    [Header("HP 이미지 배열 (좌→우 순서, 반드시 연결)")]
    [Tooltip("플레이어 HP 개수만큼 Image 오브젝트를 Inspector에서 순서대로 연결하세요. (예: 하트 3개면 3개 모두 연결)")]
    public Image[] hpImages;

    void Awake()
    {
        // 싱글톤 패턴: 중복 방지
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (player == null || scoreText == null || hpImages == null)
        {
            return;
        }

        // 점수 표시
        scoreText.text = "SCORE : " + player.CurrentScore.ToString("N0");

        // HP 이미지 표시
        UpdateHpImages();
    }

    /// <summary>
    /// 플레이어 현재 체력에 따라 HP 이미지를 On/Off
    /// </summary>
    private void UpdateHpImages()
    {
        int hp = player.CurrentHp;
        int maxHp = player.MaxHp;
        for (int i = 0; i < hpImages.Length; i++)
        {
            // 현재 체력 이하면 활성화, 초과면 비활성화
            hpImages[i].enabled = (i < hp);
        }
    }

    /// <summary>
    /// 점수 증가 함수 (적이 죽을 때 호출)
    /// </summary>
    /// <param name="amount">올릴 점수</param>
    public void AddScore(int amount)
    {
        if (player != null)
        {
            // PlayerControll의 score 변수에 직접 접근하는 구조라면 public/protected로 선언되어 있어야 함
            // 초보자 실수 방지: 반드시 PlayerControll에 score 증가 로직이 있어야 함
            // (score가 private이면 PlayerControll에 public 메서드 추가 필요)
            // 여기서는 CurrentScore가 get-only이므로, PlayerControll에 score 증가 메서드가 필요할 수 있음
            player.AddScore(amount);
        }
    }
}