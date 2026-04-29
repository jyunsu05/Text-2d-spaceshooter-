using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float destroyY = -6f;
    [SerializeField] private Vector2 moveDirection = Vector2.down;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;

    private int currentHp;

    void Awake()
    {
        currentHp = maxHp;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateSprite();
    }

    void Update()
    {
        Vector3 movement = (Vector3)moveDirection.normalized * fallSpeed * Time.deltaTime;
        transform.Translate(movement);

        if (transform.position.y <= destroyY)
        {
            Destroy(gameObject);
        }
    }

    public void SetMoveDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            moveDirection = Vector2.down;
            return;
        }

        moveDirection = direction.normalized;
    }

    public void Hit(int damage = 1)
    {
        currentHp -= damage;
        UpdateSprite();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet"))
        {
            return;
        }

        Hit();
        Destroy(other.gameObject);
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null || sprites == null || sprites.Length == 0)
        {
            return;
        }

        int damageLevel = Mathf.Clamp(maxHp - currentHp, 0, sprites.Length - 1);
        spriteRenderer.sprite = sprites[damageLevel];
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
