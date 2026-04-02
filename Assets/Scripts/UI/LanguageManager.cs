using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [SerializeField] private GameLanguage currentLanguage = GameLanguage.Chinese;
    [SerializeField] private Font chineseFont;
    [SerializeField] private Font englishFont;

    public event Action LanguageChanged;

    public GameLanguage CurrentLanguage => currentLanguage;
    public Font CurrentFont => currentLanguage == GameLanguage.Chinese ? chineseFont : englishFont;

    public Font GetFont(LocalizedTextKey key)
    {
        if (key == LocalizedTextKey.LanguageToggle && currentLanguage == GameLanguage.English)
            return chineseFont != null ? chineseFont : CurrentFont;

        return CurrentFont;
    }

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

    private static readonly Dictionary<string, LocalizedPair> WaveNames = new Dictionary<string, LocalizedPair>
    {
        { "Opening Patrol", new LocalizedPair("开场巡逻", "Opening Patrol") },
        { "Rusher Mix", new LocalizedPair("突袭混编", "Rusher Mix") },
        { "Elite Push", new LocalizedPair("精英压制", "Elite Push") }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLanguage = GameLanguage.Chinese;
        ApplyFontToExistingText();
    }

    private void Start()
    {
        NotifyLanguageChanged();
    }

    public void ToggleLanguage()
    {
        SetLanguage(currentLanguage == GameLanguage.Chinese ? GameLanguage.English : GameLanguage.Chinese);
    }

    public void SetLanguage(GameLanguage language)
    {
        if (currentLanguage == language)
        {
            NotifyLanguageChanged();
            return;
        }

        currentLanguage = language;
        NotifyLanguageChanged();
    }

    public string GetText(LocalizedTextKey key)
    {
        return Texts.TryGetValue(key, out LocalizedPair pair) ? pair.Get(currentLanguage) : key.ToString();
    }

    public string GetBuffName(BuffType type, string fallback)
    {
        return BuffNames.TryGetValue(type, out LocalizedPair pair) ? pair.Get(currentLanguage) : fallback;
    }

    public string GetWaveName(string fallback)
    {
        if (fallback == "Ready" || fallback == "准备")
            return GetText(LocalizedTextKey.Ready);

        return WaveNames.TryGetValue(fallback, out LocalizedPair pair) ? pair.Get(currentLanguage) : fallback;
    }

    public string FormatWave(int index, int total, string label)
    {
        string waveName = GetWaveName(label);
        return currentLanguage == GameLanguage.Chinese
            ? $"波次 {index}/{total}  {waveName}"
            : $"Wave {index}/{total}  {waveName}";
    }

    public string FormatEnemies(int count)
    {
        return currentLanguage == GameLanguage.Chinese ? $"敌人 {count}" : $"Enemies {count}";
    }

    public string GetNoBuffText()
    {
        return currentLanguage == GameLanguage.Chinese ? "无增益" : "No Buff";
    }

    public string GetSecondsSuffix()
    {
        return currentLanguage == GameLanguage.Chinese ? "秒" : "s";
    }

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

    private void ApplyFontToExistingText()
    {
        Font font = CurrentFont;
        if (font == null)
            return;

        Text[] texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text text in texts)
            text.font = font;
    }

    private void RefreshAllLocalizedText()
    {
        LocalizedText[] texts = FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (LocalizedText text in texts)
            text.Refresh();
    }

    private void RefreshAllHud()
    {
        HUDController[] huds = FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (HUDController hud in huds)
            hud.RefreshLanguage();
    }

    private void RefreshAllGameManagers()
    {
        GameManager[] managers = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameManager manager in managers)
            manager.RefreshLanguage();
    }

    private readonly struct LocalizedPair
    {
        private readonly string chinese;
        private readonly string english;

        public LocalizedPair(string zh, string en)
        {
            chinese = zh;
            english = en;
        }

        public string Get(GameLanguage language)
        {
            return language == GameLanguage.Chinese ? chinese : english;
        }
    }
}
