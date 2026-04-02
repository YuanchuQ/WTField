using UnityEngine;

public enum GameSfx
{
    Shoot,
    Hit,
    EnemyDie,
    Pickup,
    PlayerHurt,
    Victory,
    Defeat
}

public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip enemyDieClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip playerHurtClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip defeatClip;

    private void Awake()
    {
        Instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public static void Play(GameSfx type)
    {
        if (Instance != null)
            Instance.PlayInternal(type);
    }

    private void PlayInternal(GameSfx type)
    {
        AudioClip clip = GetClip(type);
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private AudioClip GetClip(GameSfx type)
    {
        return type switch
        {
            GameSfx.Shoot => shootClip,
            GameSfx.Hit => hitClip,
            GameSfx.EnemyDie => enemyDieClip,
            GameSfx.Pickup => pickupClip,
            GameSfx.PlayerHurt => playerHurtClip,
            GameSfx.Victory => victoryClip,
            GameSfx.Defeat => defeatClip,
            _ => null
        };
    }
}
