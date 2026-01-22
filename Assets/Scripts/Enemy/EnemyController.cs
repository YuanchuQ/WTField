// 控制敌人的移动、攻击、死亡和掉落
using System;
using UnityEngine;

// 表示敌人的行为类型
public enum EnemyBehavior
{
    // 普通追踪敌人
    Chaser,
    // 会短距离冲刺的敌人
    Rusher,
    // 血量更高的精英敌人
    Elite
}

[RequireComponent(typeof(Rigidbody2D))]
// 负责单个敌人的战斗行为
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    // 当前敌人行为类型
    [SerializeField] private EnemyBehavior behavior = EnemyBehavior.Chaser;
    // 普通移动速度
    [SerializeField] private float moveSpeed = 2.2f;
    // 冲刺移动速度
    [SerializeField] private float dashSpeed = 6.5f;
    // 触发冲刺的距离
    [SerializeField] private float dashRange = 4.5f;
    // 单次冲刺持续时间
    [SerializeField] private float dashDuration = 0.25f;
    // 冲刺冷却时间
    [SerializeField] private float dashCooldown = 1.4f;

    [Header("Combat")]
    // 接触玩家造成的伤害
    [SerializeField] private int contactDamage = 1;
    // 接触攻击间隔
    [SerializeField] private float attackInterval = 0.8f;
    // 接触攻击击退力度
    [SerializeField] private float attackKnockback = 5f;

    [Header("Drops")]
    // 可能掉落的 Buff 拾取物
    [SerializeField] private BuffPickup[] possibleDrops;
    // 掉落 Buff 的概率
    [SerializeField] private float dropChance = 0.22f;

    [Header("References")]
    // 生命组件引用
    [SerializeField] private Damageable damageable;
    // 精灵渲染器引用
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 网格寻路组件引用
    [SerializeField] private GridPathfinder pathfinder;

    // 敌人刚体引用
    private Rigidbody2D rb;
    // 玩家 Transform 引用
    private Transform player;
    // 接触攻击冷却计时
    private float attackTimer;
    // 当前冲刺剩余时间
    private float dashTimer;
    // 当前冲刺冷却剩余时间
    private float dashCooldownTimer;
    // 当前冲刺方向
    private Vector2 dashDirection;
    // 是否正在冲刺
    private bool dashing;
    // 是否已经死亡
    private bool dead;
    // 寻路刷新计时
    private float pathRefreshTimer;
    // 当前寻路方向
    private Vector2 currentPathDirection;

    // 敌人死亡时通知波次系统
    public event Action<EnemyController> Died;

    // 缓存组件并订阅死亡事件
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (damageable == null)
            damageable = GetComponent<Damageable>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (pathfinder == null)
            pathfinder = FindFirstObjectByType<GridPathfinder>();

        if (damageable != null)
            damageable.Died += Die;
    }

    // 销毁时取消死亡事件订阅
    private void OnDestroy()
    {
        if (damageable != null)
            damageable.Died -= Die;
    }

    // 开始时寻找玩家目标
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    // 更新攻击冷却、冲刺冷却和朝向表现
    private void Update()
    {
        attackTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
        if (spriteRenderer != null && rb.GetVelocity2D().sqrMagnitude > 0.01f)
            spriteRenderer.flipX = rb.GetVelocity2D().x < 0f;
    }

    // 在物理帧中追踪玩家
    private void FixedUpdate()
    {
        if (dead || player == null)
        {
            rb.SetVelocity2D(Vector2.zero);
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        Vector2 desired = GetMoveDirection(toPlayer);

        if (behavior == EnemyBehavior.Rusher)
            UpdateRusherMovement(toPlayer, desired);
        else
            rb.SetVelocity2D(desired * moveSpeed);
    }

    // 由构建器配置敌人参数
    public void Configure(
        EnemyBehavior enemyBehavior,
        float speed,
        int damage,
        float interval,
        float knockback,
        Damageable body,
        SpriteRenderer renderer,
        BuffPickup[] drops,
        float dropRate)
    {
        behavior = enemyBehavior;
        moveSpeed = speed;
        contactDamage = damage;
        attackInterval = interval;
        attackKnockback = knockback;
        damageable = body;
        spriteRenderer = renderer;
        possibleDrops = drops;
        dropChance = Mathf.Clamp01(dropRate);
    }

    // 更新冲刺敌人的移动逻辑
    private void UpdateRusherMovement(Vector2 toPlayer, Vector2 desired)
    {
        if (dashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.SetVelocity2D(dashDirection * dashSpeed);

            if (dashTimer <= 0f)
            {
                dashing = false;
                dashCooldownTimer = dashCooldown;
            }

            return;
        }

        if (dashCooldownTimer <= 0f && toPlayer.magnitude <= dashRange)
        {
            dashing = true;
            dashTimer = dashDuration;
            dashDirection = desired;
            return;
        }

        rb.SetVelocity2D(desired * moveSpeed);
    }

    // 获取追踪玩家的移动方向
    private Vector2 GetMoveDirection(Vector2 directToPlayer)
    {
        pathRefreshTimer -= Time.fixedDeltaTime;
        if (pathRefreshTimer <= 0f)
        {
            pathRefreshTimer = 0.18f;
            if (pathfinder != null && pathfinder.TryGetDirection(transform.position, player.position, out Vector2 nextDirection))
                currentPathDirection = nextDirection;
            else
                currentPathDirection = directToPlayer.sqrMagnitude > 0.01f ? directToPlayer.normalized : Vector2.zero;
        }

        return currentPathDirection;
    }

    // 与玩家接触时按间隔造成伤害
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (dead || attackTimer > 0f || !collision.collider.CompareTag("Player"))
            return;

        if (collision.collider.TryGetComponent(out IDamageable target))
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            target.TakeDamage(contactDamage, dir, attackKnockback);
            attackTimer = attackInterval;
            SfxPlayer.Play(DemoSfx.PlayerHurt);
        }
    }

    // 执行死亡、掉落和通知流程
    private void Die()
    {
        if (dead)
            return;

        dead = true;
        rb.SetVelocity2D(Vector2.zero);
        TryDropBuff();
        SfxPlayer.Play(DemoSfx.EnemyDie);
        Died?.Invoke(this);
        Destroy(gameObject);
    }

    // 按概率生成一个 Buff 掉落
    private void TryDropBuff()
    {
        if (possibleDrops == null || possibleDrops.Length == 0 || UnityEngine.Random.value > dropChance)
            return;

        BuffPickup drop = possibleDrops[UnityEngine.Random.Range(0, possibleDrops.Length)];
        if (drop != null)
            Instantiate(drop, transform.position, Quaternion.identity);
    }

}
