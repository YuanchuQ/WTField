using UnityEngine;

public class EnemySpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;

    private float timer;
    private int frameIndex;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyFrame();
    }

    private void OnEnable()
    {
        if (frames == null || frames.Length == 0)
            return;

        frameIndex = Random.Range(0, frames.Length);
        timer = 0f;
        ApplyFrame();
    }

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

    private void ApplyFrame()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
            return;

        Sprite frame = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        if (frame != null)
            spriteRenderer.sprite = frame;
    }
}
