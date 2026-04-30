using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float destroyY = -6f;
    [SerializeField] private Vector2 moveDirection = Vector2.down;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float firstShotDelay = 1.2f;
    [SerializeField] private float shotInterval = 2f;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject itemCoinPrefab;
    [SerializeField] private GameObject itemPowerPrefab;
    [SerializeField] private GameObject itemBoomPrefab;

    private int currentHp;
    private Transform enemyPoint1;
    private Transform enemyPoint2;
    private bool canShoot;

    void Awake()
    {
        currentHp = maxHp;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateSprite();

        canShoot = gameObject.name.StartsWith("Enemy_C");

        if (canShoot)
        {
            enemyPoint1 = transform.Find("EnemyPoint_1");
            enemyPoint2 = transform.Find("EnemyPoint_2");
        }
    }

    private void Start()
    {
        if (!canShoot || enemyBulletPrefab == null || enemyPoint1 == null || enemyPoint2 == null)
        {
            return;
        }

        StartCoroutine(ShootLoop());
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
        if (!other.CompareTag("Bullet") || other.CompareTag("EnemyBullet"))
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
        DropItem();
        Destroy(gameObject);
    }

    private void DropItem()
    {
        float rand = Random.value; // 0.0 ~ 1.0
        GameObject prefab = null;

        if (rand < 0.30f)           // 0% ~ 30% : None
        {
            return;
        }
        else if (rand < 0.60f)      // 30% ~ 60% : Coin
        {
            prefab = itemCoinPrefab;
        }
        else if (rand < 0.80f)      // 60% ~ 80% : Power
        {
            prefab = itemPowerPrefab;
        }
        else                        // 80% ~ 100% : Boom
        {
            prefab = itemBoomPrefab;
        }

        if (prefab != null)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator ShootLoop()
    {
        if (firstShotDelay > 0f)
        {
            yield return new WaitForSeconds(firstShotDelay);
        }

        // 화면에 진입할 때까지 대기
        while (!IsInsideMainCameraView())
        {
            yield return new WaitForSeconds(0.05f);
        }

        // 화면 안에 있는 동안 계속 발사
        while (IsInsideMainCameraView())
        {
            ShootFromPoint(enemyPoint1);
            ShootFromPoint(enemyPoint2);
            yield return new WaitForSeconds(shotInterval);
        }
    }

    private void ShootFromPoint(Transform firePoint)
    {
        if (firePoint == null || enemyBulletPrefab == null)
        {
            return;
        }

        Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
    }

    private bool IsInsideMainCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return true;
        }

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.z > 0f
               && viewportPos.x >= 0f && viewportPos.x <= 1f
               && viewportPos.y >= 0f && viewportPos.y <= 1f;
    }
}
