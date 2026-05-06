using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance { get; private set; }

    public GameObject enemy_APrefab;
    public GameObject enemy_BPrefab;
    public GameObject enemy_CPrefab;
    public GameObject itemCoinPrefab;
    public GameObject itemPowerPrefab;
    public GameObject itemBoomPrefab;
    public GameObject skillBoomPrefab;
    public GameObject playerLargeBulletPrefab;
    public GameObject playerSmallBulletPrefab;
    public GameObject followerPrefab;
    public GameObject followerBulletPrefab;
    public GameObject enemyBullet_0Prefab;
    public GameObject enemyBullet_1Prefab;
    public GameObject bossPrefab;            // 보스 프리팹
    
    GameObject[] enemy_A;
    GameObject[] enemy_B;
    GameObject[] enemy_C;
    
    GameObject[] itemCoin;
    GameObject[] itemPower;
    GameObject[] itemBoom;
    
    GameObject[] playerLargeBullet;
    GameObject[] playerSmallBullet;
    GameObject[] followers;
    GameObject[] followerBullet;
    GameObject[] enemyBullet_0;
    GameObject[] enemyBullet_1;
    GameObject[] boss;                   // 보스 풀 (1개)
    GameObject[] skillBoom;
    
    private readonly HashSet<GameObject> pooledObjects = new HashSet<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        enemy_A = new GameObject[10];
        enemy_B = new GameObject[10];
        enemy_C = new GameObject[20];
        
        itemCoin = new GameObject[20];
        itemPower = new GameObject[20];
        itemBoom = new GameObject[10];
        
        playerLargeBullet = new GameObject[100];
        playerSmallBullet = new GameObject[100];
        followers = new GameObject[3];
        followerBullet = new GameObject[100];
        enemyBullet_0 = new GameObject[100];
        enemyBullet_1 =  new GameObject[100];
        boss = new GameObject[1];            // 보스는 동시에 1개만 존재
        skillBoom = new GameObject[5];
        
        Generate();
    }

    void Generate()
    {
        //1.Enemy
        FillPool(enemy_A, enemy_APrefab);
        FillPool(enemy_B, enemy_BPrefab);
        FillPool(enemy_C, enemy_CPrefab);

        //2.Item
        FillPool(itemCoin, itemCoinPrefab);
        FillPool(itemPower, itemPowerPrefab);
        FillPool(itemBoom, itemBoomPrefab);

        //3.Bullet
        FillPool(playerLargeBullet, playerLargeBulletPrefab);
        FillPool(playerSmallBullet, playerSmallBulletPrefab);
        FillPool(followerBullet, followerBulletPrefab);
        FillPool(enemyBullet_0, enemyBullet_0Prefab);
        FillPool(enemyBullet_1, enemyBullet_1Prefab);
        FillPool(boss, bossPrefab);

        //4.Follower
        FillPool(followers, followerPrefab);

        //5.Skill
        FillPool(skillBoom, skillBoomPrefab);
    }

    private void FillPool(GameObject[] pool, GameObject prefab)
    {
        if (pool == null || prefab == null)
        {
            return;
        }

        for (int index = 0; index < pool.Length; index++)
        {
            pool[index] = Instantiate(prefab);
            pool[index].SetActive(false);
            pooledObjects.Add(pool[index]);
        }
    }

    // 타입 문자열로 해당 풀 배열을 반환하는 내부 헬퍼
    private GameObject[] GetPoolArray(string type)
    {
        switch (type)
        {
            case "Enemy_A":      return enemy_A;
            case "Enemy_B":      return enemy_B;
            case "Enemy_C":      return enemy_C;
            case "ItemCoin":     return itemCoin;
            case "ItemPower":    return itemPower;
            case "ItemBoom":     return itemBoom;
            case "largeBullet":  return playerLargeBullet;
            case "smallBullet":  return playerSmallBullet;
            case "Follower":     return followers;
            case "Follow Bullet": return followerBullet;
            case "FollowerBullet": return followerBullet;
            case "EnemyBullet_0": return enemyBullet_0;
            case "EnemyBullet_1": return enemyBullet_1;
            case "Boss":         return boss;
            case "SkillBoom":    return skillBoom;
            default:             return null;
        }
    }

    public GameObject MakeObj(string type)
    {
        GameObject[] pool = GetPoolArray(type);
        if (pool == null) return null;

        for (int index = 0; index < pool.Length; index++)
        {
            if (!pool[index].activeSelf)
            {
                pool[index].SetActive(true);
                return pool[index];
            }
        }

        return null;
    }

    public GameObject MakeObj(string type, Vector3 position, Quaternion rotation)
    {
        GameObject[] pool = GetPoolArray(type);
        if (pool == null) return null;

        for (int index = 0; index < pool.Length; index++)
        {
            if (!pool[index].activeSelf)
            {
                pool[index].transform.SetPositionAndRotation(position, rotation);
                pool[index].SetActive(true);
                return pool[index];
            }
        }

        return null;
    }

    public GameObject MakeObjByPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string type = ResolveTypeByPrefab(prefab);
        if (!string.IsNullOrEmpty(type))
        {
            return MakeObj(type, position, rotation);
        }

        return prefab != null ? Instantiate(prefab, position, rotation) : null;
    }

    public void ReturnObj(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (pooledObjects.Contains(target))
        {
            target.SetActive(false);
            return;
        }

        Destroy(target);
    }
    
    private string ResolveTypeByPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        if (prefab == enemy_APrefab) return "Enemy_A";
        if (prefab == enemy_BPrefab) return "Enemy_B";
        if (prefab == enemy_CPrefab) return "Enemy_C";
        if (prefab == itemCoinPrefab) return "ItemCoin";
        if (prefab == itemPowerPrefab) return "ItemPower";
        if (prefab == itemBoomPrefab) return "ItemBoom";
        if (prefab == playerLargeBulletPrefab) return "largeBullet";
        if (prefab == playerSmallBulletPrefab) return "smallBullet";
        if (prefab == followerPrefab) return "Follower";
        if (prefab == followerBulletPrefab) return "Follow Bullet";
        if (prefab == enemyBullet_0Prefab) return "EnemyBullet_0";
        if (prefab == enemyBullet_1Prefab) return "EnemyBullet_1";
        if (prefab == bossPrefab) return "Boss";
        if (prefab == skillBoomPrefab) return "SkillBoom";

        // 인스펙터에서 다른 인스턴스가 연결돼도 이름으로 최대한 매칭
        string prefabName = prefab.name;
        if (prefabName.Contains("Enemy_A")) return "Enemy_A";
        if (prefabName.Contains("Enemy_B")) return "Enemy_B";
        if (prefabName.Contains("Enemy_C")) return "Enemy_C";
        if (prefabName.Contains("ItemCoin")) return "ItemCoin";
        if (prefabName.Contains("ItemPower")) return "ItemPower";
        if (prefabName.Contains("ItemBoom")) return "ItemBoom";
        if (prefabName.Contains("largeBullet")) return "largeBullet";
        if (prefabName.Contains("smallBullet")) return "smallBullet";
        if (prefabName.Contains("Follower Bullet") || prefabName.Contains("Follow Bullet")) return "Follow Bullet";
        if (prefabName.Contains("Follower")) return "Follower";
        if (prefabName.Contains("EnemyBullet_0")) return "EnemyBullet_0";
        if (prefabName.Contains("EnemyBullet_1")) return "EnemyBullet_1";
        if (prefabName.Contains("Boss")) return "Boss";
        if (prefabName.Contains("SkillBoom")) return "SkillBoom";

        return null;
    }
    // 외부에서 풀 배열 전체를 조회할 때 사용 (디버그, UI 표시 등)
    public GameObject[] GetPool(string type)
    {
        return GetPoolArray(type);
    }
}
