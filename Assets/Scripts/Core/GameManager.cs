// 管理游戏开始、暂停、胜负和重开流程
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 作为整局游戏的流程控制中心
public class GameManager : MonoBehaviour
{
    [Header("Flow")]
    // 波次生成器引用
    [SerializeField] private WaveSpawner waveSpawner;
    // 玩家生命组件引用
    [SerializeField] private Damageable playerDamageable;
    // 玩家 Buff 控制器引用
    [SerializeField] private BuffController playerBuffs;
    // 玩家移动控制器引用
    [SerializeField] private PlayerController playerController;
    // 玩家射击控制器引用
    [SerializeField] private PlayerShooter playerShooter;
    // HUD 控制器引用
    [SerializeField] private HUDController hud;

    [Header("UI")]
    // 开始菜单面板
    [SerializeField] private GameObject menuPanel;
    // 暂停面板
    [SerializeField] private GameObject pausePanel;
    // 结算面板
    [SerializeField] private GameObject resultPanel;
    // 结算标题文本
    [SerializeField] private Text resultText;

    // 游戏是否已经开始
    private bool gameStarted;
    // 游戏是否已经结束
    private bool gameEnded;
    // 游戏是否处于暂停
    private bool paused;
    // 当前结算标题键
    private LocalizedTextKey resultTitleKey = LocalizedTextKey.Victory;

    // 初始化为菜单暂停状态
    private void Awake()
    {
        Time.timeScale = 0f;

        if (menuPanel != null)
            menuPanel.SetActive(true);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (playerController != null)
            playerController.SetInputLocked(true);
        if (playerShooter != null)
            playerShooter.SetCanShoot(false);
    }

    // 注册玩家和波次事件
    private void OnEnable()
    {
        if (playerDamageable != null)
        {
            if (hud != null)
                playerDamageable.HealthChanged += hud.SetHealth;
            playerDamageable.Died += Lose;
        }

        if (playerBuffs != null && hud != null)
        {
            playerBuffs.BuffStarted += hud.ShowBuff;
            playerBuffs.BuffEnded += hud.HideBuff;
        }

        if (waveSpawner != null)
        {
            waveSpawner.WaveChanged += HandleWaveChanged;
            waveSpawner.AliveCountChanged += HandleAliveChanged;
            waveSpawner.AllWavesCompleted += Win;
        }

        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged += RefreshLanguage;
    }

    // 初始化 HUD 显示
    private void Start()
    {
        if (playerDamageable != null && hud != null)
            hud.SetHealth(playerDamageable.CurrentHealth, playerDamageable.MaxHealth);

        if (hud != null)
        {
            hud.SetWave(0, 3, GetReadyLabel());
            hud.SetAliveEnemies(0);
            hud.ClearBuff();
        }
    }

    // 监听开始、暂停和重开输入
    private void Update()
    {
        if (!gameStarted && IsSubmitPressed())
            StartGame();

        if (gameEnded && IsRestartPressed())
            Restart();

        if (gameStarted && !gameEnded && IsPausePressed())
            TogglePause();
    }

    // 注销玩家和波次事件
    private void OnDisable()
    {
        if (playerDamageable != null)
        {
            if (hud != null)
                playerDamageable.HealthChanged -= hud.SetHealth;
            playerDamageable.Died -= Lose;
        }

        if (playerBuffs != null && hud != null)
        {
            playerBuffs.BuffStarted -= hud.ShowBuff;
            playerBuffs.BuffEnded -= hud.HideBuff;
        }

        if (waveSpawner != null)
        {
            waveSpawner.WaveChanged -= HandleWaveChanged;
            waveSpawner.AliveCountChanged -= HandleAliveChanged;
            waveSpawner.AllWavesCompleted -= Win;
        }

        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged -= RefreshLanguage;
    }

    // 由构建器注入场景流程引用
    public void Configure(
        WaveSpawner spawner,
        Damageable playerHealth,
        BuffController buffs,
        PlayerController controller,
        PlayerShooter shooter,
        HUDController hudController,
        GameObject menu,
        GameObject pause,
        GameObject results,
        Text resultsText)
    {
        waveSpawner = spawner;
        playerDamageable = playerHealth;
        playerBuffs = buffs;
        playerController = controller;
        playerShooter = shooter;
        hud = hudController;
        menuPanel = menu;
        pausePanel = pause;
        resultPanel = results;
        resultText = resultsText;
    }

    // 开始一局游戏
    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;
        gameEnded = false;
        paused = false;
        Time.timeScale = 1f;

        if (menuPanel != null)
            menuPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (playerController != null)
            playerController.SetInputLocked(false);
        if (playerShooter != null)
            playerShooter.SetCanShoot(true);

        waveSpawner?.Begin();
    }

    // 从暂停状态恢复游戏
    public void ResumeGame()
    {
        if (!gameStarted || gameEnded)
            return;

        paused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (playerController != null)
            playerController.SetInputLocked(false);
        if (playerShooter != null)
            playerShooter.SetCanShoot(true);
    }

    // 切换暂停状态
    public void TogglePause()
    {
        if (paused)
        {
            ResumeGame();
            return;
        }

        paused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        if (playerController != null)
            playerController.SetInputLocked(true);
        if (playerShooter != null)
            playerShooter.SetCanShoot(false);
    }

    // 触发胜利结算
    public void Win()
    {
        ShowResult(LocalizedTextKey.Victory, DemoSfx.Victory);
    }

    // 触发失败结算
    public void Lose()
    {
        ShowResult(LocalizedTextKey.Defeat, DemoSfx.Defeat);
    }

    // 重新加载当前场景
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 退出应用
    public void QuitGame()
    {
        Application.Quit();
    }

    // 根据语言刷新流程 UI 文本
    public void RefreshLanguage()
    {
        if (gameEnded)
            RefreshResultTitle();
    }

    // 接收波次变化并更新 HUD
    private void HandleWaveChanged(int index, int total, string label)
    {
        hud?.SetWave(index, total, label);
    }

    // 接收存活敌人数量并更新 HUD
    private void HandleAliveChanged(int count)
    {
        hud?.SetAliveEnemies(count);
    }

    // 显示最终结果并冻结游戏
    private void ShowResult(LocalizedTextKey titleKey, DemoSfx sfx)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        resultTitleKey = titleKey;
        Time.timeScale = 0f;

        if (playerController != null)
            playerController.SetInputLocked(true);
        if (playerShooter != null)
            playerShooter.SetCanShoot(false);

        RefreshResultTitle();
        if (resultPanel != null)
            resultPanel.SetActive(true);

        SfxPlayer.Play(sfx);
    }

    // 获取本地化文本
    private static string GetLocalizedText(LocalizedTextKey key)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetText(key) : key.ToString();
    }

    // 刷新当前结算标题
    private void RefreshResultTitle()
    {
        if (resultText != null)
            resultText.text = GetLocalizedText(resultTitleKey);
    }

    // 获取准备状态文本
    private static string GetReadyLabel()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetText(LocalizedTextKey.Ready) : "Ready";
    }

    // 判断开始键是否按下
    private bool IsSubmitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    // 判断暂停键是否按下
    private bool IsPausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    // 判断重开键是否按下
    private bool IsRestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}
