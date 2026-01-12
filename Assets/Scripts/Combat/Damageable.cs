// 提供生命、受伤、治疗和死亡事件
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
// 统一处理玩家和敌人的生命系统
public class Damageable : MonoBehaviour, IDamageable
{
    // 最大生命值
    [SerializeField] private int maxHealth = 5;
    // 受伤后的无敌时间
    [SerializeField] private float invulnerabilityDuration = 0.15f;
    // 受伤闪烁间隔
    [SerializeField] private float flashTime = 0.08f;
    // 用于闪烁反馈的渲染器
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 用于击退的刚体
    [SerializeField] private Rigidbody2D rb;

    // 当前生命值
    private int currentHealth;
    // 当前无敌剩余时间
    private float invulnerabilityTimer;
    // 本轮无敌总时长
    private float invulnerabilityTotal;
    // 是否已经死亡
    private bool dead;
    // 原始显示颜色
    private Color originalColor = Color.white;
    // 当前闪烁协程
    private Coroutine flashRoutine;
    // 当前伤害减免比例
    private float damageReduction;
    // 是否防止满血秒杀
    private bool preventOneShot;

    // 当前生命值只读访问
    public int CurrentHealth => currentHealth;
    // 最大生命值只读访问
    public int MaxHealth => maxHealth;
    // 死亡状态只读访问
    public bool IsDead => dead;

    // 生命变化时通知 HUD
    public event Action<int, int> HealthChanged;
    // 受伤时通知其他系统
    public event Action<Vector2> Damaged;
    // 死亡时通知其他系统
    public event Action Died;

    // 初始化引用并重置生命
    private void Awake()
    {
        CacheReferences();
        ResetHealth();
    }

    // 重新启用时恢复生命状态
    private void OnEnable()
    {
        if (currentHealth <= 0 || dead)
            ResetHealth();
    }

    // 每帧递减无敌时间
    private void Update()
    {
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;
    }

    // 由构建器配置生命和表现引用
    public void Configure(int health, SpriteRenderer renderer, Rigidbody2D body, float invulnerability = 0.15f)
    {
        maxHealth = Mathf.Max(1, health);
        spriteRenderer = renderer;
        rb = body;
        invulnerabilityDuration = Mathf.Max(0f, invulnerability);
        CacheReferences();
        ResetHealth();
    }

    // 重置为满血未死亡状态
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        dead = false;
        invulnerabilityTimer = 0f;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 承受一次伤害并处理击退和死亡
    public void TakeDamage(int amount, Vector2 hitDirection, float knockback)
    {
        if (dead || amount <= 0 || invulnerabilityTimer > 0f)
            return;

        int finalAmount = ApplyDefense(amount);
        currentHealth = Mathf.Max(0, currentHealth - finalAmount);
        invulnerabilityTimer = invulnerabilityDuration;
        invulnerabilityTotal = invulnerabilityDuration;
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke(hitDirection);

        if (rb != null && knockback > 0f)
            rb.AddForce(hitDirection.normalized * knockback, ForceMode2D.Impulse);

        if (spriteRenderer != null)
            Flash();

        if (currentHealth <= 0)
        {
            dead = true;
            Died?.Invoke();
        }
    }

    // 按固定数值治疗
    public void Heal(int amount)
    {
        if (dead || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 按最大生命百分比治疗
    public void HealPercent(float percent)
    {
        if (dead || percent <= 0f)
            return;

        Heal(Mathf.CeilToInt(maxHealth * percent));
    }

    // 直接恢复到满血
    public void HealToFull()
    {
        if (dead)
            return;

        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 给予指定秒数的无敌状态
    public void GrantInvulnerability(float seconds)
    {
        if (dead || seconds <= 0f)
            return;

        invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, seconds);
        invulnerabilityTotal = Mathf.Max(invulnerabilityTotal, seconds);
        if (spriteRenderer != null)
            Flash();
    }

    // 设置防御 Buff 的减伤和保命效果
    public void SetDefenseBuff(float reduction, bool oneShotProtection)
    {
        damageReduction = Mathf.Clamp01(reduction);
        preventOneShot = oneShotProtection;
    }

    // 计算减伤后的最终伤害
    private int ApplyDefense(int amount)
    {
        int finalAmount = Mathf.Max(1, Mathf.CeilToInt(amount * (1f - damageReduction)));
        if (preventOneShot && currentHealth > 1 && finalAmount >= currentHealth)
            finalAmount = currentHealth - 1;

        return finalAmount;
    }

    // 自动查找缺失的组件引用
    private void CacheReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // 启动受伤闪烁反馈
    private void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    // 在无敌期间循环闪红
    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < invulnerabilityTotal)
        {
            if (spriteRenderer == null)
                yield break;

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime;

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
