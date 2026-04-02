using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image lifeIcon;
    [SerializeField] private Text livesText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text aliveText;
    [SerializeField] private Text buffText;
    [SerializeField] private Image buffIcon;

    private static readonly BuffType[] BuffDisplayOrder =
    {
        BuffType.FullHeal,
        BuffType.MoveSpeed,
        BuffType.Damage,
        BuffType.FireRate,
        BuffType.Shield,
        BuffType.Pierce,
        BuffType.Explosive,
        BuffType.Magnet
    };

    private readonly Dictionary<BuffType, TrackedBuff> activeBuffs = new Dictionary<BuffType, TrackedBuff>();
    private float instantMessageTimer;
    private string instantMessage;
    private int currentWaveIndex;
    private int currentWaveTotal = 3;
    private string currentWaveLabel = "Ready";
    private int currentAliveEnemies;

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged += RefreshLanguage;

        RefreshLanguage();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged -= RefreshLanguage;
    }

    private void Update()
    {
        bool changed = false;
        List<BuffType> expired = null;
        foreach (KeyValuePair<BuffType, TrackedBuff> pair in activeBuffs)
        {
            pair.Value.Remaining -= Time.deltaTime;
            if (pair.Value.Remaining <= 0f)
            {
                expired ??= new List<BuffType>();
                expired.Add(pair.Key);
            }
        }

        if (expired != null)
        {
            foreach (BuffType type in expired)
                activeBuffs.Remove(type);
            changed = true;
        }

        if (instantMessageTimer > 0f)
        {
            instantMessageTimer -= Time.deltaTime;
            if (instantMessageTimer <= 0f)
                changed = true;
        }

        if (activeBuffs.Count > 0 || changed)
            RefreshBuffText();
    }

    public void SetHealth(int current, int max)
    {
        if (livesText != null)
            livesText.text = $"x {current}";

        if (lifeIcon != null)
            lifeIcon.color = current > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    public void SetWave(int index, int total, string label)
    {
        currentWaveIndex = index;
        currentWaveTotal = total;
        currentWaveLabel = label;

        if (waveText != null)
            waveText.text = FormatWaveText(index, total, label);
    }

    public void SetAliveEnemies(int count)
    {
        currentAliveEnemies = count;

        if (aliveText != null)
            aliveText.text = FormatAliveText(count);
    }

    public void ShowBuff(BuffDefinition buff)
    {
        if (buff == null)
            return;

        if (buff.duration > 0f)
            activeBuffs[buff.type] = new TrackedBuff(buff.type, buff.displayName, buff.duration);
        else
        {
            instantMessage = GetBuffName(buff.type, buff.displayName);
            instantMessageTimer = 0.8f;
        }

        if (buffIcon != null)
        {
            buffIcon.sprite = buff.icon;
            buffIcon.color = buff.icon != null ? Color.white : buff.pickupColor;
            buffIcon.enabled = true;
        }

        RefreshBuffText();
    }

    public void HideBuff(BuffType type)
    {
        if (activeBuffs.Remove(type))
            RefreshBuffText();
    }

    public void ClearBuff()
    {
        activeBuffs.Clear();
        instantMessageTimer = 0f;
        instantMessage = string.Empty;
        RefreshBuffText();

        if (buffIcon != null)
            buffIcon.enabled = false;
    }

    private void RefreshBuffText()
    {
        if (buffText == null)
            return;

        List<string> lines = new List<string>();
        if (instantMessageTimer > 0f && !string.IsNullOrEmpty(instantMessage))
            lines.Add(instantMessage);

        foreach (BuffType type in BuffDisplayOrder)
        {
            if (activeBuffs.TryGetValue(type, out TrackedBuff tracked))
                lines.Add($"{GetBuffName(tracked.Type, tracked.DisplayName)} {tracked.Remaining:0.0}{GetSecondsSuffix()}");
        }

        buffText.text = lines.Count > 0 ? string.Join("\n", lines) : GetNoBuffText();
    }

    public void RefreshLanguage()
    {
        if (waveText != null)
            waveText.text = FormatWaveText(currentWaveIndex, currentWaveTotal, currentWaveLabel);
        if (aliveText != null)
            aliveText.text = FormatAliveText(currentAliveEnemies);

        RefreshBuffText();
    }

    private static string FormatWaveText(int index, int total, string label)
    {
        LanguageManager manager = LanguageManager.Instance;
        if (manager != null)
            return manager.FormatWave(index, total, label);

        return $"Wave {index}/{total}  {label}";
    }

    private static string FormatAliveText(int count)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.FormatEnemies(count) : $"Enemies {count}";
    }

    private static string GetBuffName(BuffType type, string fallback)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetBuffName(type, fallback) : fallback;
    }

    private static string GetNoBuffText()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetNoBuffText() : "No Buff";
    }

    private static string GetSecondsSuffix()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetSecondsSuffix() : "s";
    }

    private sealed class TrackedBuff
    {
        public readonly BuffType Type;
        public readonly string DisplayName;
        public float Remaining;

        public TrackedBuff(BuffType type, string displayName, float remaining)
        {
            Type = type;
            DisplayName = displayName;
            Remaining = remaining;
        }
    }
}
