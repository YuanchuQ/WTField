using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private DirectionalSpriteAnimator spriteAnimator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMove = Vector2.right;
    private float speedMultiplier = 1f;
    private float shootingMoveMultiplier = 1f;
    private bool inputLocked;

    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public FacingDirection Facing { get; private set; } = FacingDirection.Right;
    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteAnimator == null)
            spriteAnimator = GetComponentInChildren<DirectionalSpriteAnimator>();
    }

    private void Update()
    {
        ReadMoveInput();
        UpdateAimDirection();
        UpdateAnimator();
        UpdateSpriteAnimator();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = inputLocked ? Vector2.zero : moveInput * moveSpeed * speedMultiplier * shootingMoveMultiplier;
        rb.SetVelocity2D(velocity);
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
            moveInput = Vector2.zero;
    }

    public void SetSpeedMultiplier(float value)
    {
        speedMultiplier = Mathf.Max(0.1f, value);
    }

    public void SetShootingMoveMultiplier(float value)
    {
        shootingMoveMultiplier = Mathf.Clamp(value, 0.1f, 1f);
    }

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

    private void UpdateSpriteAnimator()
    {
        if (spriteAnimator != null)
            spriteAnimator.SetState(Facing, moveInput.sqrMagnitude > 0.01f);
    }

    private static Vector2 SnapToCardinal(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return Vector2.right;

        return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));
    }

    private static FacingDirection DirectionToFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            return direction.x < 0f ? FacingDirection.Left : FacingDirection.Right;

        return direction.y < 0f ? FacingDirection.Down : FacingDirection.Up;
    }
}
