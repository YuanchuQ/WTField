using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private Damageable playerDamageable;
    [SerializeField] private BuffController playerBuffs;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private HUDController hud;

    [Header("UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;

    private bool gameStarted;
    private bool gameEnded;
    private bool paused;
    private LocalizedTextKey resultTitleKey = LocalizedTextKey.Victory;

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

    private void Update()
    {
        if (!gameStarted && IsSubmitPressed())
            StartGame();

        if (gameEnded && IsRestartPressed())
            Restart();

        if (gameStarted && !gameEnded && IsPausePressed())
            TogglePause();
    }

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

    public void Win()
    {
        ShowResult(LocalizedTextKey.Victory, GameSfx.Victory);
    }

    public void Lose()
    {
        ShowResult(LocalizedTextKey.Defeat, GameSfx.Defeat);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RefreshLanguage()
    {
        if (gameEnded)
            RefreshResultTitle();
    }

    private void HandleWaveChanged(int index, int total, string label)
    {
        hud?.SetWave(index, total, label);
    }

    private void HandleAliveChanged(int count)
    {
        hud?.SetAliveEnemies(count);
    }

    private void ShowResult(LocalizedTextKey titleKey, GameSfx sfx)
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

    private static string GetLocalizedText(LocalizedTextKey key)
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetText(key) : key.ToString();
    }

    private void RefreshResultTitle()
    {
        if (resultText != null)
            resultText.text = GetLocalizedText(resultTitleKey);
    }

    private static string GetReadyLabel()
    {
        LanguageManager manager = LanguageManager.Instance;
        return manager != null ? manager.GetText(LocalizedTextKey.Ready) : "Ready";
    }

    private bool IsSubmitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private bool IsPausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

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
