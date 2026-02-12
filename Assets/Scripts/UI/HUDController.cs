// 控制生命、波次、敌人数和 Buff 的 HUD 显示
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 管理战斗界面的动态文本和图标
public class HUDController : MonoBehaviour
{
    // 生命图标
    [SerializeField] private Image lifeIcon;
    // 生命数量文本
    [SerializeField] private Text livesText;
    // 波次状态文本
    [SerializeField] private Text waveText;
    // 存活敌人数量文本
    [SerializeField] private Text aliveText;
    // Buff 状态文本
    [SerializeField] private Text buffText;
    // 当前 Buff 图标
    [SerializeField] private Image buffIcon;

    // Buff 在 HUD 中的显示顺序
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

    // 当前 HUD 正在追踪的 Buff
    private readonly Dictionary<BuffType, TrackedBuff> activeBuffs = new Dictionary<BuffType, TrackedBuff>();
    // 瞬时提示剩余时间
    private float instantMessageTimer;
    // 瞬时提示文本
    private string instantMessage;
    // 当前波次序号
    private int currentWaveIndex;
    // 当前总波次数
    private int currentWaveTotal = 3;
    // 当前波次名称
    private string currentWaveLabel = "Ready";
    // 当前存活敌人数
    private int currentAliveEnemies;

    // 注册语言变化事件
    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged += RefreshLanguage;

        RefreshLanguage();
    }

    // 注销语言变化事件
    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged -= RefreshLanguage;
    }

    // 每帧更新 Buff 倒计时显示
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

    // 由构建器注入 HUD 控件引用
    public void Configure(Image iconImage, Text livesLabel, Text waveLabel, Text aliveLabel, Text buffLabel, Image icon)
    {
        lifeIcon = iconImage;
        livesText = livesLabel;
        waveText = waveLabel;
        aliveText = aliveLabel;
        buffText = buffLabel;
        buffIcon = icon;
    }

    // 更新玩家生命显示
    public void SetHealth(int current, int max)
    {
        if (livesText != null)
            livesText.text = $"x {current}";

        if (lifeIcon != null)
            lifeIcon.color = current > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    // 更新波次显示
    public void SetWave(int index, int total, string label)
    {
        currentWaveIndex = index;
        currentWaveTotal = total;
        currentWaveLabel = label;

        if (waveText != null)
            waveText.text = FormatWaveText(index, total, label);
    }

    // 更新存活敌人数量显示
    public void SetAliveEnemies(int count)
    {
        currentAliveEnemies = count;

        if (aliveText != null)
            aliveText.text = FormatAliveText(count);
    }

    // 显示新获得的 Buff
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

    // 隐藏指定类型的 Buff
    public void HideBuff(BuffType type)
    {
        if (activeBuffs.Remove(type))
            RefreshBuffText();
    }

    // 清空所有 Buff 显示
    public void ClearBuff()
    {
        activeBuffs.Clear();
        instantMessageTimer = 0f;
        instantMessage = string.Empty;
        RefreshBuffText();

        if (buffIcon != null)
            buffIcon.enabled = false;
    }

    // 重新生成 Buff 文本内容
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

    // 根据语言刷新 HUD 文本
    public void RefreshLanguage()
    {
        if (waveText != null)
            waveText.text = FormatWaveText(currentWaveIndex, currentWaveTotal, currentWaveLabel);
        if (aliveText != null)
            aliveText.text = FormatAliveText(currentAliveEnemies);

        RefreshBuffText();
    }

    // 格式化波次文本
    private static string FormatWaveText(int index, int total, string label)
    {
        LanguageManager manager = LanguageManager.Instance;
        if (manager != null)
            return manager.FormatWave(index, total, label);

        return $"Wave {index}/{total}  {label}";
    }

    // 格式化敌人数量文本
    private static string FormatAliveText(int count)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.FormatEnemies(count) : $"Enemies {count}";
    }

    // 获取 Buff 名称
    private static string GetBuffName(BuffType type, string fallback)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetBuffName(type, fallback) : fallback;
    }

    // 获取无 Buff 文本
    private static string GetNoBuffText()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetNoBuffText() : "No Buff";
    }

    // 获取秒数后缀
    private static string GetSecondsSuffix()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetSecondsSuffix() : "s";
    }

    // 记录 HUD 上展示的 Buff 状态
    private sealed class TrackedBuff
    {
        // Buff 类型
        public readonly BuffType Type;
        // Buff 显示名称
        public readonly string DisplayName;
        // Buff 剩余时间
        public float Remaining;

        // 创建一个 HUD Buff 状态
        public TrackedBuff(BuffType type, string displayName, float remaining)
        {
            Type = type;
            DisplayName = displayName;
            Remaining = remaining;
        }
    }
}
