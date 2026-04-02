// 管理 demo 内的短音效播放
using UnityEngine;

// 表示可播放的音效类型
public enum DemoSfx
{
    // 射击音效
    Shoot,
    // 命中音效
    Hit,
    // 敌人死亡音效
    EnemyDie,
    // 拾取音效
    Pickup,
    // 玩家受伤音效
    PlayerHurt,
    // 胜利音效
    Victory,
    // 失败音效
    Defeat
}

// 提供全局可访问的音效播放入口
public class SfxPlayer : MonoBehaviour
{
    // 当前场景中的音效播放器实例
    public static SfxPlayer Instance { get; private set; }

    // 实际播放音效的 AudioSource
    [SerializeField] private AudioSource audioSource;
    // 射击音效片段
    [SerializeField] private AudioClip shootClip;
    // 命中音效片段
    [SerializeField] private AudioClip hitClip;
    // 敌人死亡音效片段
    [SerializeField] private AudioClip enemyDieClip;
    // 拾取音效片段
    [SerializeField] private AudioClip pickupClip;
    // 玩家受伤音效片段
    [SerializeField] private AudioClip playerHurtClip;
    // 胜利音效片段
    [SerializeField] private AudioClip victoryClip;
    // 失败音效片段
    [SerializeField] private AudioClip defeatClip;

    // 初始化单例和音源引用
    private void Awake()
    {
        Instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // 由构建器注入所有音效资源
    public void Configure(
        AudioSource source,
        AudioClip shoot,
        AudioClip hit,
        AudioClip enemyDie,
        AudioClip pickup,
        AudioClip playerHurt,
        AudioClip victory,
        AudioClip defeat)
    {
        audioSource = source;
        shootClip = shoot;
        hitClip = hit;
        enemyDieClip = enemyDie;
        pickupClip = pickup;
        playerHurtClip = playerHurt;
        victoryClip = victory;
        defeatClip = defeat;
    }

    // 按类型播放一次音效
    public static void Play(DemoSfx type)
    {
        if (Instance != null)
            Instance.PlayInternal(type);
    }

    // 在当前播放器上执行播放
    private void PlayInternal(DemoSfx type)
    {
        AudioClip clip = GetClip(type);
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // 根据类型找到对应音频片段
    private AudioClip GetClip(DemoSfx type)
    {
        return type switch
        {
            DemoSfx.Shoot => shootClip,
            DemoSfx.Hit => hitClip,
            DemoSfx.EnemyDie => enemyDieClip,
            DemoSfx.Pickup => pickupClip,
            DemoSfx.PlayerHurt => playerHurtClip,
            DemoSfx.Victory => victoryClip,
            DemoSfx.Defeat => defeatClip,
            _ => null
        };
    }
}
