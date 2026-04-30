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
}
