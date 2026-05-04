using UnityEngine;

// ──────────────────────────────────────────────────────────────
// SkillBoom
// 플레이어 마우스 우클릭으로 발동되는 스킬 오브젝트 스크립트
//
// 구현된 기능:
//   1. 생성 즉시 화면 내 "Enemy" 태그를 가진 모든 적 제거
//   2. Animator가 연결된 경우 애니메이션 재생
//   3. 애니메이션 길이만큼 대기 후 자기 자신 삭제
//      (Animator 없으면 즉시 삭제)
// ──────────────────────────────────────────────────────────────
public class SkillBoom : MonoBehaviour
{
    [Tooltip("스킬붐 오브젝트가 유지되는 시간 (초). 0이면 애니메이션 길이 자동 사용")]
    public float duration = 2f;

    private CircleCollider2D circleCollider;
    private ObjectManager objectManager;
    private Coroutine returnRoutine;

    // ──────────────────────────────────────────────────────────────
    // Awake: 컴포넌트 참조 1회 초기화 (풀 재사용 시에도 유지됨)
    // OnEnable: 활성화될 때마다 지속시간 코루틴 시작 (풀 재사용 대응)
    // OnDisable: 비활성화 시 코루틴 정리
    // ──────────────────────────────────────────────────────────────

    void Awake()
    {
        objectManager = ObjectManager.Instance;

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            Debug.LogWarning("[스킬붐] CircleCollider2D가 없습니다. 오브젝트에 추가해주세요.");
        }
        else
        {
            circleCollider.isTrigger = true;
        }
    }

    void OnEnable()
    {
        if (objectManager == null)
            objectManager = ObjectManager.Instance;

        // duration이 지정되어 있으면 그 시간 사용
        // 아니면 애니메이터 클립 길이 자동 계산
        float destroyDelay = duration;

        if (destroyDelay <= 0f)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    destroyDelay = clips[0].length;
                }
            }
        }

        returnRoutine = StartCoroutine(ReturnAfterDelay(Mathf.Max(0f, destroyDelay)));
    }

    void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
    }

    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (objectManager == null)
        {
            objectManager = ObjectManager.Instance;
        }

        if (objectManager != null)
        {
            objectManager.ReturnObj(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTargetCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleTargetCollision(other);
    }

    private void HandleTargetCollision(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
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
    }
}
