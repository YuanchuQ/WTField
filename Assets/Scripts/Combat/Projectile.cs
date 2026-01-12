// 控制玩家子弹的飞行、命中和爆炸效果
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
// 表示一颗可复用的玩家子弹
public class Projectile : MonoBehaviour
{
    // 子弹飞行速度
    [SerializeField] private float speed = 12f;
    // 子弹存活时间
    [SerializeField] private float lifeTime = 2f;
    // 子弹基础伤害
    [SerializeField] private int damage = 1;
    // 子弹击退力度
    [SerializeField] private float knockback = 4f;
    // 命中特效预制体
    [SerializeField] private GameObject impactPrefab;

    // 子弹刚体引用
    private Rigidbody2D rb;
    // 所属对象池引用
    private ProjectilePool ownerPool;
    // 发射者引用
    private Transform owner;
    // 当前飞行方向
    private Vector2 direction = Vector2.right;
    // 当前剩余寿命
    private float lifeTimer;
    // 剩余穿透次数
    private int pierceLeft;
    // 当前伤害倍率
    private float currentDamageMultiplier = 1f;
    // 每次穿透后的伤害衰减
    private float pierceDamageMultiplier = 1f;
    // 是否启用爆炸效果
    private bool explosive;
    // 爆炸半径
    private float explosionRadius = 1.15f;
    // 爆炸伤害倍率
    private float explosionDamageMultiplier = 0.8f;

    // 缓存刚体引用
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 失活时清空速度
    private void OnDisable()
    {
        if (rb != null)
            rb.SetVelocity2D(Vector2.zero);
    }

    // 每帧检查寿命是否结束
    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Despawn(false, false);
    }

    // 由构建器配置子弹基础参数
    public void Configure(float projectileSpeed, float projectileLifeTime, float projectileKnockback, GameObject impact)
    {
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        knockback = projectileKnockback;
        impactPrefab = impact;
    }

    // 绑定所属对象池
    public void SetPool(ProjectilePool pool)
    {
        ownerPool = pool;
    }

    // 发射子弹并设置本次射击参数
    public void Launch(Vector2 dir, int finalDamage, int pierceCount, float pierceDecay, bool isExplosive, float radius, float explosionDamageScale, Transform ownerTransform)
    {
        owner = ownerTransform;
        direction = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
        damage = Mathf.Max(1, finalDamage);
        pierceLeft = Mathf.Max(0, pierceCount);
        currentDamageMultiplier = 1f;
        pierceDamageMultiplier = Mathf.Clamp(pierceDecay, 0.1f, 1f);
        explosive = isExplosive;
        explosionRadius = Mathf.Max(0.1f, radius);
        explosionDamageMultiplier = Mathf.Max(0.1f, explosionDamageScale);
        lifeTimer = lifeTime;

        rb.SetVelocity2D(direction * speed);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 碰撞时处理地形、敌人和穿透逻辑
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger || IsOwner(other.transform))
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Solid"))
        {
            Despawn(true, explosive);
            return;
        }

        if (other.TryGetComponent(out IDamageable damageable))
        {
            int hitDamage = Mathf.Max(1, Mathf.CeilToInt(damage * currentDamageMultiplier));
            damageable.TakeDamage(hitDamage, direction, knockback);
            SfxPlayer.Play(DemoSfx.Hit);
            if (explosive)
                SpawnExplosionDamage();

            if (pierceLeft <= 0)
                Despawn(true, false);
            else
            {
                pierceLeft--;
                currentDamageMultiplier *= pierceDamageMultiplier;
            }
        }
    }

    // 判断碰撞对象是否属于发射者
    private bool IsOwner(Transform candidate)
    {
        return owner != null && candidate.root == owner.root;
    }

    // 回收到对象池或销毁子弹
    private void Despawn(bool spawnImpact, bool applyExplosion)
    {
        if (applyExplosion)
            SpawnExplosionDamage();

        if (spawnImpact && impactPrefab != null)
            Instantiate(impactPrefab, transform.position, Quaternion.identity);

        if (ownerPool != null)
            ownerPool.Return(this);
        else
            Destroy(gameObject);
    }

    // 对爆炸范围内的可伤害目标造成范围伤害
    private void SpawnExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        int areaDamage = Mathf.Max(1, Mathf.CeilToInt(damage * explosionDamageMultiplier));
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.isTrigger || IsOwner(hit.transform))
                continue;

            if (hit.TryGetComponent(out IDamageable damageable))
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                damageable.TakeDamage(areaDamage, dir, knockback * 0.5f);
            }
        }
    }
}
