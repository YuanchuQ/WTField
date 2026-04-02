using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private Damageable damageable;
    [SerializeField] private PickupMagnet pickupMagnet;

    private readonly Dictionary<BuffType, ActiveBuff> activeBuffs = new Dictionary<BuffType, ActiveBuff>();

    public event Action<BuffDefinition> BuffStarted;
    public event Action<BuffType> BuffEnded;

    private void Awake()
    {
        CacheReferences();
    }

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

    public void Apply(BuffDefinition buff)
    {
        if (buff == null)
            return;

        SfxPlayer.Play(GameSfx.Pickup);

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

    private bool HasBuff(BuffType type)
    {
        return activeBuffs.ContainsKey(type);
    }

    private float GetMultiplier(BuffType type, float fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.multiplier : fallback;
    }

    private float GetSecondary(BuffType type, float fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.secondaryMultiplier : fallback;
    }

    private int GetFlat(BuffType type, int fallback)
    {
        return activeBuffs.TryGetValue(type, out ActiveBuff active) ? active.Definition.flatValue : fallback;
    }

    private sealed class ActiveBuff
    {
        public BuffDefinition Definition;
        public float Remaining;

        public ActiveBuff(BuffDefinition definition)
        {
            Definition = definition;
            Remaining = definition.duration;
        }
    }
}
