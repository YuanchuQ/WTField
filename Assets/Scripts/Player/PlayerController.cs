// 控制玩家移动、朝向和瞄准方向
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
// 处理玩家输入和刚体移动
public class PlayerController : MonoBehaviour
{
    // 玩家基础移动速度
    [SerializeField] private float moveSpeed = 5f;
    // 可选 Animator 引用
    [SerializeField] private Animator animator;
    // 用于旋转枪口的瞄准轴
    [SerializeField] private Transform aimPivot;
    // 玩家精灵渲染器引用
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 方向帧动画控制器引用
    [SerializeField] private DirectionalSpriteAnimator spriteAnimator;

    // 玩家刚体引用
    private Rigidbody2D rb;
    // 当前移动输入
    private Vector2 moveInput;
    // 最近一次有效移动方向
    private Vector2 lastMove = Vector2.right;
    // Buff 提供的速度倍率
    private float speedMultiplier = 1f;
    // 射击时的移动倍率
    private float shootingMoveMultiplier = 1f;
    // 是否锁定玩家输入
    private bool inputLocked;

    // 当前瞄准方向
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    // 当前面朝方向
    public FacingDirection Facing { get; private set; } = FacingDirection.Right;
    // 当前移动输入只读访问
    public Vector2 MoveInput => moveInput;

    // 初始化并缓存组件引用
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteAnimator == null)
            spriteAnimator = GetComponentInChildren<DirectionalSpriteAnimator>();
    }

    // 每帧读取输入并更新表现状态
    private void Update()
    {
        ReadMoveInput();
        UpdateAimDirection();
        UpdateAnimator();
        UpdateSpriteAnimator();
    }

    // 在物理帧中移动刚体
    private void FixedUpdate()
    {
        Vector2 velocity = inputLocked ? Vector2.zero : moveInput * moveSpeed * speedMultiplier * shootingMoveMultiplier;
        rb.SetVelocity2D(velocity);
    }

    // 由构建器配置玩家引用和速度
    public void Configure(float speed, Transform aim, SpriteRenderer renderer, DirectionalSpriteAnimator directionalAnimator = null, Animator animatorOverride = null)
    {
        moveSpeed = speed;
        aimPivot = aim;
        spriteRenderer = renderer;
        spriteAnimator = directionalAnimator;
        animator = animatorOverride;
    }

    // 设置是否锁定玩家输入
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
            moveInput = Vector2.zero;
    }

    // 设置移动速度倍率
    public void SetSpeedMultiplier(float value)
    {
        speedMultiplier = Mathf.Max(0.1f, value);
    }

    // 设置射击时移动倍率
    public void SetShootingMoveMultiplier(float value)
    {
        shootingMoveMultiplier = Mathf.Clamp(value, 0.1f, 1f);
    }

    // 读取键盘移动输入
    private void ReadMoveInput()
    {
        if (inputLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                input.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                input.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                input.y += 1f;
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
#endif

        moveInput = Vector2.ClampMagnitude(input, 1f);
        if (moveInput.sqrMagnitude > 0.01f)
            lastMove = moveInput.normalized;
    }

    // 根据移动方向更新瞄准和朝向
    private void UpdateAimDirection()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            AimDirection = SnapToCardinal(moveInput);
            Facing = DirectionToFacing(AimDirection);
        }

        if (aimPivot != null)
        {
            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // 更新可选 Animator 参数
    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
        animator.SetFloat("LastX", lastMove.x);
        animator.SetFloat("LastY", lastMove.y);
    }

    // 更新自定义方向精灵动画
    private void UpdateSpriteAnimator()
    {
        if (spriteAnimator != null)
            spriteAnimator.SetState(Facing, moveInput.sqrMagnitude > 0.01f);
    }

    // 将任意方向吸附到四方向
    private static Vector2 SnapToCardinal(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return Vector2.right;

        return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));
    }

    // 将向量转换为面朝枚举
    private static FacingDirection DirectionToFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            return direction.x < 0f ? FacingDirection.Left : FacingDirection.Right;

        return direction.y < 0f ? FacingDirection.Down : FacingDirection.Up;
    }
}
