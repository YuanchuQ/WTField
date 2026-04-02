// 控制玩家射击和子弹 Buff 参数
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 负责根据输入发射玩家子弹
public class PlayerShooter : MonoBehaviour
{
    // 玩家控制器引用
    [SerializeField] private PlayerController controller;
    // 子弹生成位置
    [SerializeField] private Transform muzzle;
    // 子弹预制体引用
    [SerializeField] private Projectile projectilePrefab;
    // 子弹对象池引用
    [SerializeField] private ProjectilePool projectilePool;
    // 基础射击间隔
    [SerializeField] private float fireInterval = 0.18f;
    // 基础子弹伤害
    [SerializeField] private int baseDamage = 1;
    // 射击时移动速度惩罚
    [SerializeField] private float shootingMovePenalty = 0.75f;

    // 当前射击冷却
    private float fireTimer;
    // 射速 Buff 倍率
    private float fireRateMultiplier = 1f;
    // 伤害 Buff 倍率
    private float damageMultiplier = 1f;
    // 子弹穿透次数
    private int pierceCount;
    // 穿透后的伤害倍率
    private float pierceDamageMultiplier = 1f;
    // 是否启用爆炸子弹
    private bool explosiveProjectiles;
    // 爆炸半径
    private float explosionRadius = 1.15f;
    // 爆炸伤害倍率
    private float explosionDamageMultiplier = 0.8f;
    // 是否允许射击
    private bool canShoot = true;

    // 初始化时缓存玩家控制器
    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
    }

    // 每帧处理射击输入和冷却
    private void Update()
    {
        fireTimer -= Time.deltaTime;

        bool firing = canShoot && IsFirePressed();
        if (controller != null)
            controller.SetShootingMoveMultiplier(firing ? shootingMovePenalty : 1f);

        if (firing && fireTimer <= 0f)
            Shoot();
    }

    // 失活时恢复移动倍率
    private void OnDisable()
    {
        if (controller != null)
            controller.SetShootingMoveMultiplier(1f);
    }

    // 由构建器配置射击依赖
    public void Configure(PlayerController playerController, Transform muzzleTransform, Projectile prefab, ProjectilePool pool, float interval, int damage)
    {
        controller = playerController;
        muzzle = muzzleTransform;
        projectilePrefab = prefab;
        projectilePool = pool;
        fireInterval = interval;
        baseDamage = damage;
    }

    // 设置是否允许射击
    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    // 设置射速倍率
    public void SetFireRateMultiplier(float value)
    {
        fireRateMultiplier = Mathf.Max(0.1f, value);
    }

    // 设置伤害倍率
    public void SetDamageMultiplier(float value)
    {
        damageMultiplier = Mathf.Max(0.1f, value);
    }

    // 设置穿透次数和衰减
    public void SetPierce(int value, float damageDecay)
    {
        pierceCount = Mathf.Max(0, value);
        pierceDamageMultiplier = Mathf.Clamp(damageDecay, 0.1f, 1f);
    }

    // 设置爆炸子弹参数
    public void SetExplosiveProjectiles(bool enabled, float radius, float damageScale)
    {
        explosiveProjectiles = enabled;
        explosionRadius = Mathf.Max(0.1f, radius);
        explosionDamageMultiplier = Mathf.Max(0.1f, damageScale);
    }

    // 设置射击时移动惩罚
    public void SetShootingMovePenalty(float value)
    {
        shootingMovePenalty = Mathf.Clamp(value, 0.1f, 1f);
    }

    // 创建或取出子弹并发射
    private void Shoot()
    {
        if (projectilePrefab == null && projectilePool == null)
            return;

        Vector2 dir = controller != null ? controller.AimDirection : Vector2.right;
        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;

        Projectile projectile = projectilePool != null
            ? projectilePool.Get(spawnPos, Quaternion.identity)
            : Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        int finalDamage = Mathf.Max(1, Mathf.CeilToInt(baseDamage * damageMultiplier));
        projectile.Launch(dir, finalDamage, pierceCount, pierceDamageMultiplier, explosiveProjectiles, explosionRadius, explosionDamageMultiplier, transform);
        SfxPlayer.Play(DemoSfx.Shoot);

        fireTimer = fireInterval / fireRateMultiplier;
    }

    // 判断射击键是否按住
    private bool IsFirePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.jKey.isPressed;
#else
        return Input.GetKey(KeyCode.J);
#endif
    }
}
