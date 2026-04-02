// 复用玩家子弹以减少频繁实例化
using System.Collections.Generic;
using UnityEngine;

// 管理子弹对象池
public class ProjectilePool : MonoBehaviour
{
    // 用于创建子弹的预制体
    [SerializeField] private Projectile projectilePrefab;
    // 初始预热数量
    [SerializeField] private int initialSize = 32;

    // 可复用子弹队列
    private readonly Queue<Projectile> pool = new Queue<Projectile>();

    // 初始化时预创建子弹
    private void Awake()
    {
        Warm(initialSize);
    }

    // 由构建器配置对象池
    public void Configure(Projectile prefab, int size)
    {
        projectilePrefab = prefab;
        initialSize = Mathf.Max(1, size);
    }

    // 取出一个可用子弹
    public Projectile Get(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
            CreateInstance();

        Projectile projectile = pool.Dequeue();
        projectile.transform.SetParent(null);
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.SetPool(this);
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    // 将子弹回收到池中
    public void Return(Projectile projectile)
    {
        if (projectile == null || pool.Contains(projectile))
            return;

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform);
        pool.Enqueue(projectile);
    }

    // 按数量预热对象池
    private void Warm(int count)
    {
        if (projectilePrefab == null)
            return;

        for (int i = 0; i < count; i++)
            CreateInstance();
    }

    // 创建一个新的池内子弹
    private Projectile CreateInstance()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform);
        projectile.SetPool(this);
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
        return projectile;
    }
}
