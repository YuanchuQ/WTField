// 管理玩家身上的临时 Buff 并同步属性
using System;
using System.Collections.Generic;
using UnityEngine;

// 负责应用、刷新和结束玩家 Buff
public class BuffController : MonoBehaviour
{
    // 玩家移动控制器引用
    [SerializeField] private PlayerController playerController;
    // 玩家射击控制器引用
    [SerializeField] private PlayerShooter playerShooter;
    // 玩家生命组件引用
    [SerializeField] private Damageable damageable;
    // 拾取物磁吸组件引用
    [SerializeField] private PickupMagnet pickupMagnet;

    // 当前仍在生效的 Buff 表
    private readonly Dictionary<BuffType, ActiveBuff> activeBuffs = new Dictionary<BuffType, ActiveBuff>();

    // Buff 开始时通知 HUD
    public event Action<BuffDefinition> BuffStarted;
    // Buff 结束时通知 HUD
    public event Action<BuffType> BuffEnded;

    // 初始化时缓存依赖组件
    private void Awake()
    {
        CacheReferences();
    }

    // 每帧倒计时并移除过期 Buff
    private void Update()
    {
        if (activeBuffs.Count == 0)
            return;

        List<BuffType> expired = null;
        foreach (KeyValuePair<BuffType, ActiveBuff> pair in activeBuffs)
        {
            pair.Value.Remaining -= Time.deltaTime;
            if (pair.Value.Remaining <= 0f)
            {
                expired ??= new List<BuffType>();
                expired.Add(pair.Key);
            }
        }

        if (expired == null)
            return;

        foreach (BuffType type in expired)
        {
            activeBuffs.Remove(type);
            BuffEnded?.Invoke(type);
        }

        RefreshStats();
    }

    // 由构建器注入玩家相关引用
    public void Configure(PlayerController controller, PlayerShooter shooter, Damageable body)
    {
        playerController = controller;
        playerShooter = shooter;
        damageable = body;
        CacheReferences();
    }

    // 应用一个 Buff 定义到玩家身上
    public void Apply(BuffDefinition buff)
    {
        if (buff == null)
            return;

        SfxPlayer.Play(DemoSfx.Pickup);

        if (buff.type == BuffType.HealPercent)
        {
            damageable?.HealPercent(buff.multiplier);
            BuffStarted?.Invoke(buff);
            return;
        }

        if (buff.type == BuffType.FullHeal)
        {
            damageable?.HealToFull();
            damageable?.GrantInvulnerability(buff.duration);
            BuffStarted?.Invoke(buff);
            return;
        }

        if (activeBuffs.TryGetValue(buff.type, out ActiveBuff active))
        {
            active.Definition = buff;
            active.Remaining = buff.duration;
        }
        else
        {
            activeBuffs.Add(buff.type, new ActiveBuff(buff));
        }

        BuffStarted?.Invoke(buff);
        RefreshStats();
    }

    // 补齐未手动配置的组件引用
    private void CacheReferences()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerShooter == null)
            playerShooter = GetComponent<PlayerShooter>();
        if (damageable == null)
            damageable = GetComponent<Damageable>();
        if (pickupMagnet == null)
            pickupMagnet = GetComponent<PickupMagnet>();
    }

    // 根据当前 Buff 统一刷新玩家属性
    private void RefreshStats()
    {
        playerController?.SetSpeedMultiplier(GetMultiplier(BuffType.MoveSpeed, 1f));
        playerShooter?.SetShootingMovePenalty(HasBuff(BuffType.MoveSpeed) ? GetSecondary(BuffType.MoveSpeed, 0.875f) : 0.75f);
        playerShooter?.SetDamageMultiplier(GetMultiplier(BuffType.Damage, 1f));
        playerShooter?.SetFireRateMultiplier(GetMultiplier(BuffType.FireRate, 1f));
        playerShooter?.SetPierce(GetFlat(BuffType.Pierce, 0), GetSecondary(BuffType.Pierce, 0.8f));
        playerShooter?.SetExplosiveProjectiles(
            HasBuff(BuffType.Explosive),
            GetFlat(BuffType.Explosive, 1),
            GetSecondary(BuffType.Explosive, 0.8f));
        damageable?.SetDefenseBuff(HasBuff(BuffType.Shield) ? GetMultiplier(BuffType.Shield, 0.6f) : 0f, HasBuff(BuffType.Shield));
        pickupMagnet?.SetActive(HasBuff(BuffType.Magnet));
    }

    // 判断某类 Buff 是否正在生效
    private bool HasBuff(BuffType type)
    {
        return activeBuffs.ContainsKey(type);
    }

    // 获取 Buff 的主倍率
    private float GetMultiplier(BuffType type, float fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.multiplier : fallback;
    }

    // 获取 Buff 的副倍率
    private float GetSecondary(BuffType type, float fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.secondaryMultiplier : fallback;
    }

    // 获取 Buff 的整数参数
    private int GetFlat(BuffType type, int fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.flatValue : fallback;
    }

    // 记录一个正在生效的 Buff
    private sealed class ActiveBuff
    {
        // Buff 的配置数据
        public BuffDefinition Definition;
        // Buff 剩余持续时间
        public float Remaining;

        // 创建 Buff 状态并初始化时长
        public ActiveBuff(BuffDefinition definition)
        {
            Definition = definition;
            Remaining = definition.duration;
        }
    }
}
