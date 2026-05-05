using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float followDistance = 0.5f;

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= followDistance) return;

        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}