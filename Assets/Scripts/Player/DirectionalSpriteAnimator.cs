// 根据玩家朝向播放四方向精灵动画
using UnityEngine;

// 表示玩家当前面朝方向
public enum FacingDirection
{
    // 面朝右方
    Right,
    // 面朝左方
    Left,
    // 面朝上方
    Up,
    // 面朝下方
    Down
}

// 控制玩家不同方向的逐帧动画
public class DirectionalSpriteAnimator : MonoBehaviour
{
    // 需要替换精灵的渲染器
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 向右移动的动画帧
    [SerializeField] private Sprite[] rightFrames;
    // 向左移动的动画帧
    [SerializeField] private Sprite[] leftFrames;
    // 向上移动的动画帧
    [SerializeField] private Sprite[] upFrames;
    // 向下移动的动画帧
    [SerializeField] private Sprite[] downFrames;
    // 每秒播放帧数
    [SerializeField] private float framesPerSecond = 8f;

    // 当前面朝方向
    private FacingDirection facing = FacingDirection.Right;
    // 当前是否正在移动
    private bool moving;
    // 当前帧计时器
    private float timer;
    // 当前帧索引
    private int frameIndex;

    // 初始化渲染器并显示第一帧
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ApplyFrame();
    }

    // 根据移动状态推进动画帧
    private void Update()
    {
        Sprite[] frames = GetFrames(facing);
        if (spriteRenderer == null || frames.Length == 0)
            return;

        if (!moving)
        {
            frameIndex = 0;
            timer = 0f;
            ApplyFrame();
            return;
        }

        timer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        while (timer >= frameTime)
        {
            timer -= frameTime;
            frameIndex = (frameIndex + 1) % frames.Length;
            ApplyFrame();
        }
    }

    // 由构建器配置四方向帧动画
    public void Configure(SpriteRenderer renderer, Sprite[] right, Sprite[] left, Sprite[] up, Sprite[] down, float fps)
    {
        spriteRenderer = renderer;
        rightFrames = right;
        leftFrames = left;
        upFrames = up;
        downFrames = down;
        framesPerSecond = fps;
        ApplyFrame();
    }

    // 设置当前朝向和移动状态
    public void SetState(FacingDirection nextFacing, bool isMoving)
    {
        if (facing != nextFacing)
        {
            facing = nextFacing;
            frameIndex = 0;
            timer = 0f;
        }

        moving = isMoving;
    }

    // 获取指定方向的帧数组
    private Sprite[] GetFrames(FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.Left => leftFrames,
            FacingDirection.Up => upFrames,
            FacingDirection.Down => downFrames,
            _ => rightFrames
        } ?? System.Array.Empty<Sprite>();
    }

    // 将当前帧应用到渲染器
    private void ApplyFrame()
    {
        if (spriteRenderer == null)
            return;

        Sprite[] frames = GetFrames(facing);
        if (frames.Length == 0)
            return;

        spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
    }
}
