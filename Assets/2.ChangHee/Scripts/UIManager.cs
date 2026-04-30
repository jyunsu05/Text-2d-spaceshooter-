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

    [Header("붐 이미지 배열 (좌→우 순서, 반드시 연결)")]
    [Tooltip("최대 붐 개수만큼 Image 오브젝트를 Inspector에서 순서대로 연결하세요.")]
    public Image[] boomImages;

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

    void Start()
    {
        // 플레이어의 MaxHp를 기준으로 HP 이미지 초기 상태 설정
        InitHpImages();
    }

    void Update()
    {
        if (player == null || scoreText == null || hpImages == null)
        {
            return;
        }

        // 점수 표시
        scoreText.text = player.CurrentScore.ToString("N0");

        // HP 이미지 표시
        UpdateHpImages();

        // 붐 이미지 표시
        UpdateBoomImages();
    }

    /// <summary>
    /// 게임 시작 시 MaxHp 기준으로 HP 이미지 전체 활성화
    /// </summary>
    private void InitHpImages()
    {
        if (player == null || hpImages == null) return;

        int maxHp = player.MaxHp;
        for (int i = 0; i < hpImages.Length; i++)
        {
            // MaxHp 이하 인덱스만 활성화 (MaxHp보다 이미지가 많을 경우 나머지 숨김)
            hpImages[i].enabled = (i < maxHp);
        }
    }

    /// <summary>
    /// 플레이어 현재 체력에 따라 HP 이미지를 오른쪽부터 하나씩 비활성화
    /// 배열 순서: hpImages[0]=왼쪽 첫 번째, hpImages[끝]=오른쪽 마지막
    /// </summary>
    private void UpdateHpImages()
    {
        int hp = player.CurrentHp;
        for (int i = 0; i < hpImages.Length; i++)
        {
            // 인덱스 i가 현재 HP 미만이면 활성화 → HP 감소 시 오른쪽(높은 인덱스)부터 꺼짐
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
            player.AddScore(amount);
        }
    }
    
    //boomImages가 ItemBoom을 먹으면 오른쪽부터 하나씩 이미지가 활성화
    //플레이어가 우클릭으로 스킬붐을 쓸 시 왼쪽부터 boomImages 비활성화
    // → 두 경우 모두 CurrentSkillBoomCount 기준으로 매 프레임 갱신
    private void UpdateBoomImages()
    {
        if (boomImages == null) return;

        int boom = player.CurrentSkillBoomCount;
        for (int i = 0; i < boomImages.Length; i++)
        {
            // 오른쪽부터 채움: boom=1이면 맨 오른쪽(Length-1)만 활성
            // 왼쪽부터 비움: boom 감소 시 왼쪽(낮은 인덱스)의 활성 이미지부터 꺼짐
            boomImages[i].enabled = (i >= boomImages.Length - boom);
        }
    }
}