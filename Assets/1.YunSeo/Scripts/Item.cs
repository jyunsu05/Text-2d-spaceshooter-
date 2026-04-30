using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string type;
    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.linearVelocity = Vector2.down * 0.5f;
    }

    private void Update()
    {
        transform.Translate(Vector2.down * 0.5f * Time.deltaTime);
        if (transform.position.y <= -7f)
        {
            Destroy(gameObject);
        }
    }
}
