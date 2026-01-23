// 控制敌人的循环逐帧动画
using UnityEngine;

// 为敌人播放简单的 Sprite 动画
public class EnemySpriteAnimator : MonoBehaviour
{
    // 需要替换精灵的渲染器
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 敌人动画帧数组
    [SerializeField] private Sprite[] frames;
    // 每秒播放帧数
    [SerializeField] private float framesPerSecond = 8f;

    // 当前帧计时器
    private float timer;
    // 当前帧索引
    private int frameIndex;

    // 初始化渲染器并显示当前帧
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyFrame();
    }

    // 启用时随机一个起始帧
    private void OnEnable()
    {
        if (frames == null || frames.Length == 0)
            return;

        frameIndex = Random.Range(0, frames.Length);
        timer = 0f;
        ApplyFrame();
    }

    // 每帧推进敌人动画
    private void Update()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        while (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex = (frameIndex + 1) % frames.Length;
            ApplyFrame();
        }
    }

    // 由构建器配置动画帧
    public void Configure(SpriteRenderer renderer, Sprite[] animationFrames, float fps)
    {
        spriteRenderer = renderer;
        frames = animationFrames;
        framesPerSecond = fps;
        frameIndex = 0;
        timer = 0f;
        ApplyFrame();
    }

    // 将当前帧应用到渲染器
    private void ApplyFrame()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
            return;

        Sprite frame = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        if (frame != null)
            spriteRenderer.sprite = frame;
    }
}
