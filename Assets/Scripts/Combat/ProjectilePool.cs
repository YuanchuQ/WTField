using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int initialSize = 32;

    private readonly Queue<Projectile> pool = new Queue<Projectile>();

    private void Awake()
    {
        Warm(initialSize);
    }

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

    public void Return(Projectile projectile)
    {
        if (projectile == null || pool.Contains(projectile))
            return;

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform);
        pool.Enqueue(projectile);
    }

    private void Warm(int count)
    {
        if (projectilePrefab == null)
            return;

        for (int i = 0; i < count; i++)
            CreateInstance();
    }

    private Projectile CreateInstance()
    {
        Projectile projectile = Instantiate(projectilePrefab, transform);
        projectile.SetPool(this);
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
        return projectile;
    }
}
