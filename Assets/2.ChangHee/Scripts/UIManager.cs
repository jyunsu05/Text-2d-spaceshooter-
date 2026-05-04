using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스 (Inspector 연결 실수 방지)
    public static UIManager Instance { get; private set; }

    [Header("게임오버 UI (선택 연결)")]
    [Tooltip("게임오버 시 표시할 패널(텍스트/버튼 포함 가능)")]
    public GameObject gameOverPanel;
    [Tooltip("재시작 버튼(선택). 패널 안에 있으면 연결 권장")]
    public Button retryButton;

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

    // 게임오버 로직이 중복 실행되지 않도록 상태를 저장
    private bool isGameOver;

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

        // 씬 시작 시 GameOver/Retry는 반드시 비활성 상태로 고정
        SetGameOverUiVisible(false);
    }

    void Start()
    {
        // 시작 시 버튼 이벤트 연결
        BindUiEvents();

        // 플레이어의 MaxHp를 기준으로 HP 이미지 초기 상태 설정
        InitHpImages();
    }

    void Update()
    {
        // HP가 0이 되면 즉시 게임오버 처리 (다른 UI 참조 누락과 무관하게 동작)
        DecreaseLife();

        if (player == null)
        {
            return;
        }

        // 점수 표시 (연결된 경우만)
        if (scoreText != null)
        {
            scoreText.text = player.CurrentScore.ToString("N0");
        }

        // HP 이미지 표시 (연결된 경우만)
        if (hpImages != null)
        {
            UpdateHpImages();
        }

        // 붐 이미지 표시
        UpdateBoomImages();
    }

    private void BindUiEvents()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryCurrentScene);
            retryButton.onClick.AddListener(RetryCurrentScene);
        }
    }

    private void SetGameOverUiVisible(bool visible)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(visible);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 다른 스크립트에서 게임오버 UI 참조를 등록할 때 사용한다.
    /// </summary>
    public void RegisterGameOverUi(GameObject panel, Button retry)
    {
        gameOverPanel = panel;
        retryButton = retry;

        BindUiEvents();
        SetGameOverUiVisible(isGameOver);
    }

    /// <summary>
    /// 다른 스크립트에서 플레이어 참조를 등록할 때 사용한다.
    /// </summary>
    public void RegisterPlayer(PlayerControll targetPlayer)
    {
        player = targetPlayer;
        InitHpImages();
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
    
    // Item Boom 획득 시: 오른쪽부터 채워짐
    // 스킬붐 사용 시: 오른쪽부터 비어짐
    // 두 경우 모두 player.CurrentSkillBoomCount를 기준으로 매 프레임 표시 갱신
    private void UpdateBoomImages()
    {
        if (player == null || boomImages == null) return;

        int boom = Mathf.Clamp(player.CurrentSkillBoomCount, 0, boomImages.Length);
        for (int i = 0; i < boomImages.Length; i++)
        {
            // boom=1이면 맨 오른쪽 1칸만 켜짐
            boomImages[i].enabled = (i >= boomImages.Length - boom);
        }
    }

    public bool DecreaseLife()
    {
        // 이미 게임오버 처리했다면 true 반환
        if (isGameOver)
        {
            return true;
        }

        // 플레이어가 없거나 HP가 남아 있으면 GameOver/Retry를 계속 비활성 상태로 유지
        if (player == null || player.CurrentHp > 0)
        {
            SetGameOverUiVisible(false);
            return false;
        }

        // 여기부터는 최초 1회 게임오버 처리
        isGameOver = true;
        SetGameOverUiVisible(true);

        // 플레이어 조작 중지
        player.enabled = false;

        // 적 스폰 중지
        SpawnManager[] spawnManagers = FindObjectsByType<SpawnManager>();
        for (int i = 0; i < spawnManagers.Length; i++)
        {
            spawnManagers[i].enabled = false;
        }

        // 이미 생성된 적의 이동/행동 중지
        Enemy[] enemies = FindObjectsByType<Enemy>();
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].enabled = false;
        }

        // 전체 시간 정지 (물리/코루틴/애니메이션 진행 멈춤)
        Time.timeScale = 0f;
        return true;
    }

    /// <summary>
    /// 재시작 버튼에서 호출: 현재 씬을 다시 로드한다.
    /// </summary>
    public void RetryCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        // 씬 이동 시 시간 정지가 남아있지 않도록 안전 복구
        Time.timeScale = 1f;
    }
}