using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 이동, 발사, 화면 범위 제한, 파워업을 담당하는 컨트롤러
/// </summary>
public class PlayerControll : MonoBehaviour
{
    private ObjectManager objectManager;

    /// <summary>
    /// 점수 증가 함수 (외부에서 안전하게 호출)
    /// </summary>
    /// <param name="amount">올릴 점수</param>
    public void AddScore(int amount)
    {
        score += amount;
        // 필요시 점수 증가 이펙트/사운드 등 추가 가능
    }
    // UI/외부 스크립트에서 읽기 쉽게 제공하는 상태값
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int CurrentScore => score;
    public int CurrentBulletPower => bulletPower;
    public int CurrentPowerCount => powerCount;
    public int CurrentSkillBoomCount => skillBoomCount;
    public int MaxItemCount => maxItemCount;

    // ──────────────────────────────────────────────
    // 인스펙터 설정값
    // ──────────────────────────────────────────────

    [Header("이동 설정")]
    [Tooltip("플레이어 이동 속도")]
    public float speed = 5f;

    [Header("총알 단계")]
    [Range(1, 3)]
    [Tooltip("현재 총알 파워 단계 (1~3)")]
    public int bulletPower = 1;

    [Header("총알 프리팹")]
    [Tooltip("기본(소형) 총알 프리팹")]
    public GameObject smallBulletPrefab;
    [Tooltip("대형 총알 프리팹 (파워3 중앙 발사용)")]
    public GameObject largeBulletPrefab;

    [Header("스킬붐")]
    [Tooltip("SkillBoom 프리팹 (마우스 우클릭으로 발동)")]
    public GameObject skillBoomPrefab;

    [Header("총알 발사 위치")]
    [Tooltip("총알이 생성될 기준 위치")]
    public Transform firePoint;

    [Header("파워2 좌우 간격")]
    [Tooltip("파워2일 때 좌우 총알 간격 (단위: 유닛)")]
    public float power2Offset = 0.3f;

    [Header("파워3 좌우 간격")]
    [Tooltip("파워3일 때 좌우 소형 총알 간격 (단위: 유닛)")]
    public float power3Offset = 0.4f;

    [Header("연사 설정")]
    [Tooltip("총알 발사 간격 (초). 낮을수록 빠른 연사")]
    public float shotInterval = 0.12f;

    [Header("쫄다구 발사")]
    [Tooltip("플레이어와 함께 총알을 발사할 쫄다구 Transform 목록")]
    // Inspector에서 Follower_0, Follower_1, Follower_2를 순서대로 넣는 배열입니다.
    // 이 배열은 쫄다구 오브젝트의 Transform 주소 목록 역할을 합니다.
    public Transform[] followers;

    [Tooltip("이 파워 단계 이상일 때 쫄다구도 함께 총알을 발사")]
    // 예: 2로 두면 bulletPower가 2 이상일 때부터 쫄다구도 발사합니다.
    public int followerShootRequiredPower = 2;

    [Tooltip("쫄다구 총알의 겉모습으로 사용할 오브젝트입니다. Follow Bullet을 여기에 연결하세요. 실제 이동/삭제 기능은 smallBulletPrefab을 사용합니다.")]
    public GameObject followerBulletPrefab;

    [Tooltip("쫄다구 총알 속도를 따로 조절할지 여부")]
    public bool useFollowerBulletSpeed = true;

    [Tooltip("쫄다구 총알 속도입니다. 총알 스크립트 안에 speed 필드/프로퍼티가 있으면 이 값으로 덮어씁니다.")]
    public float followerBulletSpeed = 8f;

    [Tooltip("쫄다구가 1개만 활성화되었을 때, 플레이어가 몇 발 쏠 때마다 쫄다구가 1발 쏠지 정합니다.")]
    public int followerShotIntervalWhenOneFollower = 5;

    [Tooltip("쫄다구가 2개 활성화되었을 때 발사 간격")]
    public int followerShotIntervalWhenTwoFollowers = 3;

    [Tooltip("쫄다구가 3개 활성화되었을 때 발사 간격")]
    public int followerShotIntervalWhenThreeFollowers = 2;

    [Tooltip("쫄다구 점사 횟수 (한 번에 몇 발을 연속으로 쏠지)")]
    public int followerBurstCount = 3;

    [Tooltip("점사 내부 총알 간 간격 (초)")]
    public float followerBurstInterval = 0.06f;

    [Header("HP 설정")]
    [Tooltip("플레이어 최대 HP")]
    public int maxHp = 3;
    [Tooltip("피격 후 무적 지속 시간(초)")]
    public float invincibleDuration = 1.0f;

    [Header("점수")]
    [Tooltip("현재 점수")]
    public int score = 0;
    [Tooltip("코인 아이템 점수")]
    public int coinScore = 1000;
    [Tooltip("파워 아이템 점수")]
    public int powerScore = 500;
    [Tooltip("붐 아이템 점수")]
    public int boomScore = 500;

    [Header("아이템 카운트")]
    [Tooltip("Power 누적 획득 수")]
    public int powerCount = 0;
    [Tooltip("SkillBoom 보유 수")]
    public int skillBoomCount = 0;
    [Tooltip("Power / SkillBoom 최대 보유 수")]
    public int maxItemCount = 3;

    // ──────────────────────────────────────────────
    // 내부 상태
    // ──────────────────────────────────────────────

    private float nextShotTime;   // 다음 발사 가능 시각
    private Camera mainCamera;
    private float halfWidth;      // 스프라이트 가로 절반 크기 (화면 경계 계산용)
    private float halfHeight;     // 스프라이트 세로 절반 크기 (화면 경계 계산용)
    private SpriteRenderer playerSpriteRenderer;
    private Animator animator;
    private int currentHp;
    private bool isInvincible;
    private Coroutine invincibleRoutine;

    // 현재 활성화된 쫄다구 개수입니다.
    // 시작 시에는 0입니다.
    // Power 아이템을 먹을 때마다 1씩 증가하며, followers 배열의 앞쪽부터 하나씩 켭니다.
    // 예: 0이면 Follower_0을 켜고, 1이면 Follower_1을 켜고, 2이면 Follower_2를 켭니다.
    private int activeFollowerCount = 0;

    // 플레이어가 총알을 몇 번 쐈는지 세는 카운터입니다.
    // 이 값을 이용해서 쫄다구는 매번 쏘지 않고, 몇 발에 한 번씩만 쏩니다.
    private int followerShotCounter = 0;

    // ──────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────

    void Start()
    {
        mainCamera = Camera.main;
        currentHp = maxHp;
        objectManager = ObjectManager.Instance;

        // 화면 경계 클램핑에 사용할 스프라이트 크기 캐싱
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            halfWidth  = playerSpriteRenderer.bounds.extents.x;
            halfHeight = playerSpriteRenderer.bounds.extents.y;
        }

        animator = GetComponent<Animator>();

        AutoAssignFollowersIfNeeded();
        AutoAssignFollowerTargets();
        DetachFollowersFromPlayer();

        // 게임 시작 시에는 쫄다구를 모두 꺼둡니다.
        // 쫄다구는 Power 아이템을 먹을 때 ActivateNextFollower()로 하나씩 켜집니다.
        if (followers != null)
        {
            for (int i = 0; i < followers.Length; i++)
            {
                if (followers[i] != null)
                {
                    followers[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Prefab 안에서는 Player 자식으로 보관하되, 플레이 중에는 월드 오브젝트로 분리합니다.
    /// 자식 상태로 남아 있으면 Player 이동을 그대로 물려받아 고정 위치처럼 보입니다.
    /// </summary>
    void DetachFollowersFromPlayer()
    {
        if (followers == null || followers.Length == 0)
        {
            return;
        }

        for (int i = 0; i < followers.Length; i++)
        {
            Transform follower = followers[i];
            if (follower == null) continue;

            if (follower.IsChildOf(transform))
            {
                follower.SetParent(null, true);
            }
        }
    }

    /// <summary>
    /// Prefab 원본에는 Scene 오브젝트를 직접 연결할 수 없어서 followers가 비어 있을 수 있습니다.
    /// 실행 시 Player 자식 또는 Scene 안의 Follower_0, Follower_1, Follower_2를 찾아 자동 연결합니다.
    /// </summary>
    void AutoAssignFollowersIfNeeded()
    {
        if (!ShouldAutoAssignFollowers())
        {
            return;
        }

        int targetCount = Mathf.Max(1, maxItemCount);
        List<Transform> foundFollowers = new List<Transform>();

        for (int i = 0; i < targetCount; i++)
        {
            string followerName = $"Follower_{i}";
            Transform follower = FindChildRecursive(transform, followerName);

            if (follower == null)
            {
                GameObject followerObject = GameObject.Find(followerName);
                if (followerObject != null)
                {
                    follower = followerObject.transform;
                }
            }

            if (follower != null)
            {
                foundFollowers.Add(follower);
            }
        }

        if (foundFollowers.Count > 0)
        {
            followers = foundFollowers.ToArray();
            Debug.Log($"[쫄다구] followers 자동 연결 완료: {followers.Length}개");
        }
        else
        {
            Debug.LogWarning("[쫄다구] followers가 비어 있고 Follower_0, Follower_1, Follower_2도 찾지 못했습니다.");
        }
    }

    bool ShouldAutoAssignFollowers()
    {
        if (followers == null || followers.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < followers.Length; i++)
        {
            if (followers[i] != null)
            {
                return false;
            }
        }

        return true;
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nestedChild = FindChildRecursive(child, childName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }

    /// <summary>
    /// Follower 컴포넌트의 target이 비어 있으면 플레이어부터 순서대로 따라가도록 자동 연결합니다.
    /// Follower_0은 Player, Follower_1은 Follower_0, Follower_2는 Follower_1을 따라갑니다.
    /// </summary>
    void AutoAssignFollowerTargets()
    {
        if (followers == null || followers.Length == 0)
        {
            return;
        }

        Transform previousTarget = transform;
        for (int i = 0; i < followers.Length; i++)
        {
            Transform followerTransform = followers[i];
            if (followerTransform == null) continue;

            Follower follower = followerTransform.GetComponent<Follower>();
            if (follower != null && follower.target == null)
            {
                follower.target = previousTarget;
            }

            previousTarget = followerTransform;
        }
    }

    // ──────────────────────────────────────────────
    // 매 프레임 업데이트
    // ──────────────────────────────────────────────

    void Update()
    {
        Move();
        ShootInput();
        ClampToScreen();
    }

    // ──────────────────────────────────────────────
    // 이동 처리
    // WASD / 방향키 입력을 받아 플레이어를 이동시키고
    // 애니메이터 State 파라미터를 갱신한다.
    //   0 = 정면, 1 = 좌, 2 = 우
    // ──────────────────────────────────────────────

    void Move()
    {
        Vector2 moveInput = ReadMoveInput();
        float moveX = moveInput.x;
        float moveY = moveInput.y;

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        if (animator == null) return;

        int state = moveX < 0 ? 1 : moveX > 0 ? 2 : 0;
        animator.SetInteger("State", state);
    }

    // ──────────────────────────────────────────────
    // 발사 입력 처리
    // 마우스 왼쪽 버튼을 누르고 있는 동안 shotInterval 간격으로 연사한다.
    // ──────────────────────────────────────────────

    void ShootInput()
    {
        // 체력이 0이면 더 이상 총알이나 스킬붐을 사용하지 않습니다.
        // GameOver 상태에서 플레이어가 계속 발사되는 문제를 막습니다.
        if (currentHp <= 0) return;

        bool isFireHeld = IsFireHeld();
        bool isSkillPressed = IsSkillPressedThisFrame();

        // 마우스 왼쪽: 일반 총알 연사
        if (!isInvincible && isFireHeld && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + shotInterval;
        }

        // 마우스 우클릭: 스킬붐 발동
        if (!isInvincible && isSkillPressed)
        {
            UseSkillBoom();
        }
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 move = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move.y += 1f;
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > move.sqrMagnitude)
            {
                move = stick;
            }
        }

        return Vector2.ClampMagnitude(move, 1f);
    }

    private bool IsFireHeld()
    {
        bool mouseFire = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool keyboardFire = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool gamepadFire = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
        return mouseFire || keyboardFire || gamepadFire;
    }

    private bool IsSkillPressedThisFrame()
    {
        bool mouseSkill = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool keyboardSkill = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
        bool gamepadSkill = Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame;
        return mouseSkill || keyboardSkill || gamepadSkill;
    }

    // ──────────────────────────────────────────────
    // 총알 생성 (파워 단계별)
    //   파워1 : 중앙에 소형 총알 1발
    //   파워2 : 중앙 기준 좌우에 소형 총알 2발 (간격: power2Offset)
    //   파워3 : 중앙에 대형 총알 1발 + 좌우에 소형 총알 2발 (간격: power3Offset)
    // ──────────────────────────────────────────────

    void Shoot()
    {
        switch (bulletPower)
        {
            case 1:
                // 중앙 소형 총알 1발
                SpawnBulletFromManager("smallBullet", firePoint.position, firePoint.rotation);
                break;

            case 2:
                // 좌우 소형 총알 2발
                SpawnBullet(smallBulletPrefab, -power2Offset);
                SpawnBullet(smallBulletPrefab,  power2Offset);
                break;

            case 3:
                // 중앙 대형 총알 + 좌우 소형 총알
                SpawnBulletFromManager("largeBullet", firePoint.position, firePoint.rotation);
                SpawnBullet(smallBulletPrefab, -power3Offset);
                SpawnBullet(smallBulletPrefab,  power3Offset);
                break;
        }

        // 플레이어가 총알을 쏜 횟수를 기준으로 쫄다구 발사 타이밍을 조절합니다.
        // 예: 쫄다구 1개일 때는 플레이어 5발마다 1발,
        // 쫄다구 2개 이상일 때는 플레이어 3발마다 1발 발사합니다.
        TryShootFollowersByInterval();
    }

    /// <summary>
    /// 쫄다구 발사 빈도를 조절한다.
    /// 총알 속도를 느리게 만드는 것이 아니라, 플레이어가 여러 번 쏠 때 쫄다구는 한 번만 쏘게 한다.
    /// 쫄다구 1/2/3개 각각 별도의 발사 간격을 사용한다.
    /// </summary>
    void TryShootFollowersByInterval()
    {
        // 아직 파워가 부족하면 쫄다구는 발사하지 않습니다.
        if (bulletPower < followerShootRequiredPower) return;

        // 활성화된 쫄다구가 없으면 발사하지 않습니다.
        if (activeFollowerCount <= 0) return;

        followerShotCounter++;
        int shotInterval;

        if (activeFollowerCount <= 1)
        {
            shotInterval = followerShotIntervalWhenOneFollower;
        }
        else if (activeFollowerCount == 2)
        {
            shotInterval = followerShotIntervalWhenTwoFollowers;
        }
        else
        {
            shotInterval = followerShotIntervalWhenThreeFollowers;
        }

        // 실수로 0 이하가 들어와도 1 이상으로 보정합니다.
        shotInterval = Mathf.Max(1, shotInterval);

        // 아직 쫄다구가 쏠 차례가 아니면 종료합니다.
        if (followerShotCounter < shotInterval) return;

        // 쏠 차례가 되었으므로 카운터를 초기화하고 쫄다구 총알을 발사합니다.
        followerShotCounter = 0;
        StartCoroutine(ShootFollowersBurst());
    }

    /// <summary>
    /// 실제로 쫄다구 총알을 생성한다.
    /// 이 함수는 매번 호출되는 것이 아니라 TryShootFollowersByInterval()에서 정해진 발사 간격이 되었을 때만 호출된다.
    /// 쫄다구 총알은 smallBulletPrefab으로 만들고, followerBulletPrefab의 Sprite만 복사해서 겉모습을 바꿉니다.
    /// </summary>
    void ShootFollowers()
    {
        // 아직 파워가 부족하면 쫄다구는 발사하지 않습니다.
        if (bulletPower < followerShootRequiredPower) return;

        // followers 배열이 비어 있으면 아무것도 하지 않습니다.
        if (followers == null || followers.Length == 0) return;

        for (int i = 0; i < followers.Length; i++)
        {
            Transform follower = followers[i];

            // 배열 칸이 비어 있으면 건너뜁니다.
            if (follower == null) continue;

            // 아직 비활성화된 쫄다구는 총알을 발사하지 않습니다.
            if (!follower.gameObject.activeInHierarchy) continue;

            // 실제 총알은 기존 smallBulletPrefab으로 생성합니다.
            // 이렇게 해야 smallBulletPrefab에 들어 있는 이동/삭제/충돌 로직을 그대로 사용할 수 있습니다.
            GameObject followerBullet = Instantiate(smallBulletPrefab, follower.position, firePoint.rotation);

            // 겉모습만 Follow Bullet 오브젝트의 Sprite로 바꿉니다.
            // 즉, 총알 기능은 smallBulletPrefab / 총알 그림은 followerBulletPrefab 구조입니다.
            ApplyFollowerBulletVisual(followerBullet);

            // 필요하면 쫄다구 총알 속도를 Inspector 값으로 덮어씁니다.
            ApplyFollowerBulletSpeed(followerBullet);
        }
    }


    /// <summary>
    /// Power 아이템을 먹을 때마다 다음 쫄다구를 하나씩 활성화한다.
    /// 이미 켜진 쫄다구는 유지하고, 최대 followers.Length까지만 켠다.
    /// </summary>
    void ActivateNextFollower()
    {
        if (followers == null || followers.Length == 0)
        {
            Debug.LogWarning("[쫄다구] followers 배열이 비어 있습니다. Player Inspector에서 Followers Size와 Element를 확인하세요.");
            return;
        }

        if (activeFollowerCount >= followers.Length)
        {
            Debug.Log("[쫄다구] 이미 최대 개수까지 활성화되었습니다.");
            return;
        }

        Transform follower = followers[activeFollowerCount];
        if (follower == null)
        {
            Debug.LogWarning($"[쫄다구] followers[{activeFollowerCount}]가 비어 있습니다. Inspector에서 Follower_{activeFollowerCount}를 연결하세요.");
            activeFollowerCount++;
            return;
        }

        // 켜지는 순간 플레이어 주변에 배치해서 화면 밖이나 엉뚱한 위치에서 시작하지 않게 합니다.
        follower.position = transform.position + new Vector3(-0.6f * (activeFollowerCount + 1), 0f, 0f);
        follower.gameObject.SetActive(true);

        Debug.Log($"[쫄다구] Follower {activeFollowerCount} 활성화: {follower.name}");
        activeFollowerCount++;
    }

    /// <summary>
    /// firePoint 기준으로 X 오프셋을 적용한 위치에 총알을 생성한다.
    /// </summary>
    /// <param name="prefab">생성할 총알 프리팹</param>
    /// <param name="xOffset">firePoint.x 기준 오프셋 (음수=왼쪽, 양수=오른쪽)</param>
    void SpawnBullet(GameObject prefab, float xOffset)
    {
        Vector3 spawnPos = firePoint.position + new Vector3(xOffset, 0f, 0f);

        string bulletType = prefab == largeBulletPrefab ? "largeBullet" : "smallBullet";
        SpawnBulletFromManager(bulletType, spawnPos, firePoint.rotation);
    }

    private GameObject SpawnBulletFromManager(string type, Vector3 position, Quaternion rotation)
    {
        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        if (objectManager != null)
        {
            GameObject pooled = objectManager.MakeObj(type, position, rotation);
            if (pooled != null)
            {
                return pooled;
            }
        }

        // 풀에서 못 가져오면 기존 방식으로 안전 폴백
        GameObject fallbackPrefab = type == "largeBullet" ? largeBulletPrefab : smallBulletPrefab;
        if (fallbackPrefab != null)
        {
            return Instantiate(fallbackPrefab, position, rotation);
        }

        return null;
    }

    /// <summary>
    /// 쫄다구 총알의 겉모습만 Follow Bullet 오브젝트처럼 바꾸는 함수입니다.
    /// followerBulletPrefab 자체를 발사하지 않습니다.
    /// 이유: Follow Bullet 오브젝트에는 총알 이동/삭제 스크립트가 없을 수 있기 때문입니다.
    /// 실제 총알 기능은 smallBulletPrefab을 사용하고, 여기서는 SpriteRenderer의 sprite만 복사합니다.
    /// </summary>
    void ApplyFollowerBulletVisual(GameObject bullet)
    {
        if (bullet == null) return;
        if (followerBulletPrefab == null) return;

        SpriteRenderer sourceRenderer = followerBulletPrefab.GetComponent<SpriteRenderer>();
        SpriteRenderer targetRenderer = bullet.GetComponent<SpriteRenderer>();

        if (sourceRenderer == null || targetRenderer == null) return;

        targetRenderer.sprite = sourceRenderer.sprite;
        targetRenderer.color = sourceRenderer.color;
        targetRenderer.flipX = sourceRenderer.flipX;
        targetRenderer.flipY = sourceRenderer.flipY;
    }

    /// <summary>
    /// 쫄다구 총알 속도를 Inspector에서 조절할 수 있게 하는 보조 함수입니다.
    /// 총알 프리팹의 이동 스크립트에 speed라는 float 필드나 프로퍼티가 있으면 그 값을 바꿉니다.
    /// 팀원이 총알 속도를 바꿔달라고 하면 코드 수정 없이 followerBulletSpeed만 조절하면 됩니다.
    /// </summary>
    void ApplyFollowerBulletSpeed(GameObject bullet)
    {
        if (!useFollowerBulletSpeed) return;
        if (bullet == null) return;

        MonoBehaviour[] scripts = bullet.GetComponents<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++)
        {
            MonoBehaviour script = scripts[i];
            if (script == null) continue;

            System.Type scriptType = script.GetType();

            // 총알 스크립트 안에 speed라는 이름의 float 변수가 있는지 찾습니다.
            // public이든 private이든 찾기 위해 Reflection을 사용합니다.
            FieldInfo speedField = scriptType.GetField("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (speedField != null && speedField.FieldType == typeof(float))
            {
                speedField.SetValue(script, followerBulletSpeed);
                return;
            }

            // speed가 변수(Field)가 아니라 프로퍼티(Property)로 만들어져 있을 경우도 처리합니다.
            PropertyInfo speedProperty = scriptType.GetProperty("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (speedProperty != null && speedProperty.PropertyType == typeof(float) && speedProperty.CanWrite)
            {
                speedProperty.SetValue(script, followerBulletSpeed);
                return;
            }
        }
    }

    // ──────────────────────────────────────────────
    // 파워업
    // 외부(아이템 등)에서 호출하면 bulletPower를 1 증가시킨다.
    // 최대 3단계까지만 증가한다.
    // ──────────────────────────────────────────────

    public void PowerUp()
    {
        bulletPower = Mathf.Min(bulletPower + 1, 3);
    }

    // ──────────────────────────────────────────────
    // 스킬붐 발동 (마우스 우클릭)
    // - skillBoomCount가 1 이상일 때만 사용 가능
    // - 카운트 -1 후 플레이어 위치에 SkillBoom 프리팹 생성
    // - 실제 적 제거는 SkillBoom.cs에서 처리
    // ──────────────────────────────────────────────

    void UseSkillBoom()
    {
        if (skillBoomCount <= 0)
        {
            Debug.Log("[스킬붐] 보유 수량이 없습니다.");
            return;
        }

        if (skillBoomPrefab == null)
        {
            Debug.LogWarning("[스킬붐] skillBoomPrefab이 연결되지 않았습니다.");
            return;
        }

        skillBoomCount--;

        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        if (objectManager != null && objectManager.MakeObj("SkillBoom", Vector3.zero, Quaternion.identity) != null)
        {
            Debug.Log($"[스킬붐] 발동! 남은 수량: {skillBoomCount}/{maxItemCount}");
            return;
        }

        Instantiate(skillBoomPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log($"[스킬붐] 발동! 남은 수량: {skillBoomCount}/{maxItemCount}");
    }

    // ──────────────────────────────────────────────
    // 화면 경계 클램핑
    // 플레이어가 카메라 뷰포트 밖으로 나가지 않도록 위치를 제한한다.
    // ──────────────────────────────────────────────

    void ClampToScreen()
    {
        if (mainCamera == null) return;

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x + halfWidth,  max.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, min.y + halfHeight, max.y - halfHeight);
        transform.position = pos;
    }

    // ──────────────────────────────────────────────
    // 충돌 감지
    // - Enemy(적 본체): 체력 -1
    // - EnemyBullet(적 총알): 체력 -1 + 총알 삭제
    // - Item(아이템): 종류별 효과 적용
    // ──────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1, other);
        }
        else if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1, other);
            if (objectManager == null)
            {
                objectManager = ObjectManager.Instance;
            }

            if (objectManager != null)
            {
                objectManager.ReturnObj(other.gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("Item"))
        {
            HandleItemCollision(other);
        }
    }

    // ──────────────────────────────────────────────
    // 아이템 충돌 처리
    // - Item 컴포넌트의 type 값으로 종류 구분
    //   Coin  : 점수 +1000
    //   Power : 점수 +500, 파워 단계 +1 (최대 3단계)
    //           powerCount 증가 (최대 3)
    //   Boom  : 점수 +500, skillBoomCount 증가 (최대 3)
    // - 충돌한 아이템 오브젝트는 항상 삭제
    // ──────────────────────────────────────────────

    void HandleItemCollision(Collider2D itemCollider)
    {
        Item item = itemCollider.GetComponent<Item>();
        if (item == null)
        {
            return;
        }

        switch (item.type)
        {
            case "Coin":
                score += coinScore;
                Debug.Log($"[아이템] Coin 획득 | +{coinScore}점 | 총점: {score}");
                break;

            case "Power":
                score += powerScore;

                if (powerCount < maxItemCount)
                {
                    powerCount++;
                }

                // Power 아이템을 먹을 때마다 쫄다구를 1개씩 활성화합니다.
                // bulletPower가 이미 최대여도 쫄다구는 최대 3개까지 순서대로 켜질 수 있습니다.
                ActivateNextFollower();

                if (bulletPower < 3)
                {
                    PowerUp();
                    Debug.Log($"[아이템] Power 획득 | +{powerScore}점 | 파워 단계: {bulletPower}/3 | Power 카운트: {powerCount}/{maxItemCount} | 총점: {score}");
                }
                else
                {
                    Debug.Log($"[아이템] Power 획득 | +{powerScore}점 | power가 최대입니다 ({bulletPower}/3) | Power 카운트: {powerCount}/{maxItemCount} | 총점: {score}");
                }
                break;

            case "Boom":
                score += boomScore;

                if (skillBoomCount < maxItemCount)
                {
                    skillBoomCount++;
                    Debug.Log($"[아이템] Boom 획득 | +{boomScore}점 | SkillBoom 카운트 증가: {skillBoomCount}/{maxItemCount} | 총점: {score}");
                }
                else
                {
                    Debug.Log($"[아이템] Boom 획득 | +{boomScore}점 | SkillBoom이 최대입니다 ({skillBoomCount}/{maxItemCount}) | 총점: {score}");
                }
                break;

            default:
                Debug.Log($"알 수 없는 아이템: {item.type}");
                break;
        }

        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        if (objectManager != null)
        {
            objectManager.ReturnObj(itemCollider.gameObject);
        }
        else
        {
            Destroy(itemCollider.gameObject);
        }
    }

    // ──────────────────────────────────────────────
    // 피격 처리
    // - HP가 이미 0이면 피격 무시 + 로그 출력
    // - 무적 상태면 피격 무시
    // - HP가 남아있으면 damage만큼 감소
    // - 피격 후 invincibleDuration 동안 무적
    // - HP가 0이 되면 "체력 부족" 로그 출력
    // - 로그 형식: (현재 체력 / 최대 체력) = 충돌 개체 : 개체 이름
    // ──────────────────────────────────────────────

    public void TakeDamage(int damage, Collider2D collision)
    {
        if (currentHp <= 0)
        {
            string collisionType = collision.CompareTag("Enemy") ? "적" : "적 총알";
            Debug.Log($"({currentHp} / {maxHp}) = {collisionType} : {collision.gameObject.name} (체력 없음)");
            return;
        }

        if (isInvincible)
        {
            return;
        }

        int previousHp = currentHp;
        currentHp = Mathf.Max(0, currentHp - damage);
        string type = collision.CompareTag("Enemy") ? "적" : "적 총알";
        Debug.Log($"({previousHp} / {maxHp}) = {type} : {collision.gameObject.name}");

        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
        }
        invincibleRoutine = StartCoroutine(InvincibleRoutine());

        if (currentHp <= 0)
        {
            Debug.Log($"({currentHp} / {maxHp}) = 플레이어 체력 부족");
        }
    }

    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        Debug.Log($"[무적] 시작 ({Mathf.Max(0f, invincibleDuration):0.00}초)");

        if (playerSpriteRenderer != null)
        {
            // 무적 시간 동안 플레이어 아이콘 숨김
            playerSpriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, invincibleDuration));

        if (playerSpriteRenderer != null)
        {
            // 무적 종료 시 아이콘 다시 표시
            playerSpriteRenderer.enabled = true;
        }

        isInvincible = false;
        invincibleRoutine = null;
        Debug.Log("[무적] 종료");
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화/파괴 시 아이콘이 꺼진 채 남지 않도록 복구
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// 쫄다구 점사(따다닥) 발사 코루틴
    /// 한 번 발사 타이밍이 오면 여러 발을 짧은 간격으로 연속 발사한다.
    /// </summary>
    IEnumerator ShootFollowersBurst()
    {
        int count = Mathf.Max(1, followerBurstCount);

        for (int i = 0; i < count; i++)
        {
            ShootFollowers();

            // 마지막 발 이후에는 대기하지 않음
            if (i < count - 1)
            {
                yield return new WaitForSeconds(followerBurstInterval);
            }
        }
    }
}
