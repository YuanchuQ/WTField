using UnityEngine;

public enum FacingDirection
{
    Right,
    Left,
    Up,
    Down
}

public class DirectionalSpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] rightFrames;
    [SerializeField] private Sprite[] leftFrames;
    [SerializeField] private Sprite[] upFrames;
    [SerializeField] private Sprite[] downFrames;
    [SerializeField] private float framesPerSecond = 8f;

    private FacingDirection facing = FacingDirection.Right;
    private bool moving;
    private float timer;
    private int frameIndex;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ApplyFrame();
    }

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
