// 管理中英文 UI 文本和界面字体
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 提供全局语言切换和本地化查询
public class LanguageManager : MonoBehaviour
{
    // 当前场景中的语言管理器
    public static LanguageManager Instance { get; private set; }

    // 默认界面语言
    [SerializeField] private DemoLanguage currentLanguage = DemoLanguage.Chinese;
    // 中文 UI 使用的字体
    [SerializeField] private Font chineseFont;
    // 英文 UI 使用的字体
    [SerializeField] private Font englishFont;

    // 界面语言变化事件
    public event Action LanguageChanged;

    // 当前界面语言
    public DemoLanguage CurrentLanguage => currentLanguage;
    // 当前语言应使用的字体
    public Font CurrentFont => currentLanguage == DemoLanguage.Chinese ? chineseFont : englishFont;

    // 获取指定文本键应使用的字体
    public Font GetFont(LocalizedTextKey key)
    {
        if (key == LocalizedTextKey.LanguageToggle && currentLanguage == DemoLanguage.English)
            return chineseFont != null ? chineseFont : CurrentFont;

        return CurrentFont;
    }

    // UI 固定文本字典
    private static readonly Dictionary<LocalizedTextKey, LocalizedPair> Texts = new Dictionary<LocalizedTextKey, LocalizedPair>
    {
        { LocalizedTextKey.Title, new LocalizedPair("明日方舟:肿么滴", "ARKNIGHTS: WHAT'S UP") },
        { LocalizedTextKey.Start, new LocalizedPair("开始作战", "START") },
        { LocalizedTextKey.Paused, new LocalizedPair("暂停", "PAUSED") },
        { LocalizedTextKey.PauseHint, new LocalizedPair("按 Esc 继续", "Press Esc to resume") },
        { LocalizedTextKey.Resume, new LocalizedPair("继续", "RESUME") },
        { LocalizedTextKey.Victory, new LocalizedPair("作战胜利", "VICTORY") },
        { LocalizedTextKey.Defeat, new LocalizedPair("作战失败", "DEFEAT") },
        { LocalizedTextKey.RestartHint, new LocalizedPair("按 R 重新开始", "Press R to restart") },
        { LocalizedTextKey.Restart, new LocalizedPair("重新开始", "RESTART") },
        { LocalizedTextKey.Quit, new LocalizedPair("退出", "QUIT") },
        { LocalizedTextKey.LanguageToggle, new LocalizedPair("English", "中文") },
        { LocalizedTextKey.Ready, new LocalizedPair("准备", "Ready") }
    };

    // Buff 名称字典
    private static readonly Dictionary<BuffType, LocalizedPair> BuffNames = new Dictionary<BuffType, LocalizedPair>
    {
        { BuffType.HealPercent, new LocalizedPair("小型医疗包", "Small Medkit") },
        { BuffType.FullHeal, new LocalizedPair("应急医疗包", "Emergency Kit") },
        { BuffType.MoveSpeed, new LocalizedPair("疾风糖果", "Gale Candy") },
        { BuffType.Damage, new LocalizedPair("锋利晶石", "Sharp Originium") },
        { BuffType.FireRate, new LocalizedPair("急速弹夹", "Rapid Magazine") },
        { BuffType.Shield, new LocalizedPair("能量护甲", "Energy Armor") },
        { BuffType.Pierce, new LocalizedPair("穿甲弹", "Piercing Rounds") },
        { BuffType.Explosive, new LocalizedPair("火焰榴弹", "Flame Grenade") },
        { BuffType.Magnet, new LocalizedPair("磁力光环", "Magnet Aura") }
    };

    // 波次名称字典
    private static readonly Dictionary<string, LocalizedPair> WaveNames = new Dictionary<string, LocalizedPair>
    {
        { "Opening Patrol", new LocalizedPair("开场巡逻", "Opening Patrol") },
        { "Rusher Mix", new LocalizedPair("突袭混编", "Rusher Mix") },
        { "Elite Push", new LocalizedPair("精英压制", "Elite Push") }
    };

    // 初始化全局实例和默认中文
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLanguage = DemoLanguage.Chinese;
        ApplyFontToExistingText();
    }

    // 场景启用后刷新所有本地化文本
    private void Start()
    {
        NotifyLanguageChanged();
    }

    // 由构建器注入中英文字体
    public void Configure(Font zhFont, Font enFont, DemoLanguage defaultLanguage = DemoLanguage.Chinese)
    {
        if (Instance == null)
            Instance = this;

        chineseFont = zhFont;
        englishFont = enFont != null ? enFont : zhFont;
        currentLanguage = defaultLanguage;
    }

    // 切换中英文界面
    public void ToggleLanguage()
    {
        SetLanguage(currentLanguage == DemoLanguage.Chinese ? DemoLanguage.English : DemoLanguage.Chinese);
    }

    // 设置当前界面语言
    public void SetLanguage(DemoLanguage language)
    {
        if (currentLanguage == language)
        {
            NotifyLanguageChanged();
            return;
        }

        currentLanguage = language;
        NotifyLanguageChanged();
    }

    // 获取固定文本
    public string GetText(LocalizedTextKey key)
    {
        return Texts.TryGetValue(key, out LocalizedPair pair) ? pair.Get(currentLanguage) : key.ToString();
    }

    // 获取 Buff 显示名称
    public string GetBuffName(BuffType type, string fallback)
    {
        return BuffNames.TryGetValue(type, out LocalizedPair pair) ? pair.Get(currentLanguage) : fallback;
    }

    // 获取波次显示名称
    public string GetWaveName(string fallback)
    {
        if (fallback == "Ready" || fallback == "准备")
            return GetText(LocalizedTextKey.Ready);

        return WaveNames.TryGetValue(fallback, out LocalizedPair pair) ? pair.Get(currentLanguage) : fallback;
    }

    // 格式化波次文本
    public string FormatWave(int index, int total, string label)
    {
        string waveName = GetWaveName(label);
        return currentLanguage == DemoLanguage.Chinese
            ? $"波次 {index}/{total}  {waveName}"
            : $"Wave {index}/{total}  {waveName}";
    }

    // 格式化敌人数量文本
    public string FormatEnemies(int count)
    {
        return currentLanguage == DemoLanguage.Chinese ? $"敌人 {count}" : $"Enemies {count}";
    }

    // 获取空 Buff 文本
    public string GetNoBuffText()
    {
        return currentLanguage == DemoLanguage.Chinese ? "无增益" : "No Buff";
    }

    // 获取秒数后缀
    public string GetSecondsSuffix()
    {
        return currentLanguage == DemoLanguage.Chinese ? "秒" : "s";
    }

    // 通知所有 UI 语言已变化
    private void NotifyLanguageChanged()
    {
        if (Instance == null)
            Instance = this;

        ApplyFontToExistingText();
        RefreshAllLocalizedText();
        LanguageChanged?.Invoke();
        RefreshAllHud();
        RefreshAllGameManagers();
    }

    // 给场景中现有 Text 应用当前字体
    private void ApplyFontToExistingText()
    {
        Font font = CurrentFont;
        if (font == null)
            return;

        Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text text in texts)
            text.font = font;
    }

    // 主动刷新所有本地化文本
    private void RefreshAllLocalizedText()
    {
        LocalizedText[] texts = FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (LocalizedText text in texts)
            text.Refresh();
    }

    // 主动刷新所有 HUD 动态文本
    private void RefreshAllHud()
    {
        HUDController[] huds = FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (HUDController hud in huds)
            hud.RefreshLanguage();
    }

    // 主动刷新所有流程 UI 文本
    private void RefreshAllGameManagers()
    {
        GameManager[] managers = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameManager manager in managers)
            manager.RefreshLanguage();
    }

    // 保存一条中英文文本
    private readonly struct LocalizedPair
    {
        // 中文文本
        private readonly string chinese;
        // 英文文本
        private readonly string english;

        // 创建中英文文本对
        public LocalizedPair(string zh, string en)
        {
            chinese = zh;
            english = en;
        }

        // 按语言取出文本
        public string Get(DemoLanguage language)
        {
            return language == DemoLanguage.Chinese ? chinese : english;
        }
    }
}
