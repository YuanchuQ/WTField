using System;
using UnityEngine;

public enum EnemyBehavior
{
    Chaser,
    Rusher,
    Elite
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private EnemyBehavior behavior = EnemyBehavior.Chaser;
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float dashSpeed = 6.5f;
    [SerializeField] private float dashRange = 4.5f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1.4f;

    [Header("Combat")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float attackKnockback = 5f;

    [Header("Drops")]
    [SerializeField] private BuffPickup[] possibleDrops;
    [SerializeField] private float dropChance = 0.22f;

    [Header("References")]
    [SerializeField] private Damageable damageable;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GridPathfinder pathfinder;

    private Rigidbody2D rb;
    private Transform player;
    private float attackTimer;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private bool dashing;
    private bool dead;
    private float pathRefreshTimer;
    private Vector2 currentPathDirection;

    public event Action<EnemyController> Died;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (damageable == null)
            damageable = GetComponent<Damageable>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (pathfinder == null)
            pathfinder = FindFirstObjectByType<GridPathfinder>();

        if (damageable != null)
            damageable.Died += Die;
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.Died -= Die;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
        if (spriteRenderer != null && rb.GetVelocity2D().sqrMagnitude > 0.01f)
            spriteRenderer.flipX = rb.GetVelocity2D().x < 0f;
    }

    private void FixedUpdate()
    {
        if (dead || player == null)
        {
            rb.SetVelocity2D(Vector2.zero);
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        Vector2 desired = GetMoveDirection(toPlayer);

        if (behavior == EnemyBehavior.Rusher)
            UpdateRusherMovement(toPlayer, desired);
        else
            rb.SetVelocity2D(desired * moveSpeed);
    }

    private void UpdateRusherMovement(Vector2 toPlayer, Vector2 desired)
    {
        if (dashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.SetVelocity2D(dashDirection * dashSpeed);

            if (dashTimer <= 0f)
            {
                dashing = false;
                dashCooldownTimer = dashCooldown;
            }

            return;
        }

        if (dashCooldownTimer <= 0f && toPlayer.magnitude <= dashRange)
        {
            dashing = true;
            dashTimer = dashDuration;
            dashDirection = desired;
            return;
        }

        rb.SetVelocity2D(desired * moveSpeed);
    }

    private Vector2 GetMoveDirection(Vector2 directToPlayer)
    {
        pathRefreshTimer -= Time.fixedDeltaTime;
        if (pathRefreshTimer <= 0f)
        {
            pathRefreshTimer = 0.18f;
            if (pathfinder != null && pathfinder.TryGetDirection(transform.position, player.position, out Vector2 nextDirection))
                currentPathDirection = nextDirection;
            else
                currentPathDirection = directToPlayer.sqrMagnitude > 0.01f ? directToPlayer.normalized : Vector2.zero;
        }

        return currentPathDirection;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (dead || attackTimer > 0f || !collision.collider.CompareTag("Player"))
            return;

        if (collision.collider.TryGetComponent(out IDamageable target))
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            target.TakeDamage(contactDamage, dir, attackKnockback);
            attackTimer = attackInterval;
            SfxPlayer.Play(GameSfx.PlayerHurt);
        }
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;
        rb.SetVelocity2D(Vector2.zero);
        TryDropBuff();
        SfxPlayer.Play(GameSfx.EnemyDie);
        Died?.Invoke(this);
        Destroy(gameObject);
    }

    private void TryDropBuff()
    {
        if (possibleDrops == null || possibleDrops.Length == 0 || UnityEngine.Random.value > dropChance)
            return;

        BuffPickup drop = possibleDrops[UnityEngine.Random.Range(0, possibleDrops.Length)];
        if (drop != null)
            Instantiate(drop, transform.position, Quaternion.identity);
    }

}
