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

    // ──────────────────────────────────────────────────────────────
    // 생성 시 실행
    // - CircleCollider2D를 트리거로 사용해 애니메이션 재생 중 닿는 적/적 총알 제거
    // - duration이 0보다 크면 해당 시간만큼 대기 후 삭제
    // - duration이 0이면 애니메이션 클립 길이만큼 대기 후 삭제
    //   (Animator도 없으면 즉시 삭제)
    // ──────────────────────────────────────────────────────────────

    void Start()
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

        StartCoroutine(ReturnAfterDelay(Mathf.Max(0f, destroyDelay)));
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
