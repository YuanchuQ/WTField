using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockback = 4f;
    [SerializeField] private GameObject impactPrefab;

    private Rigidbody2D rb;
    private ProjectilePool ownerPool;
    private Transform owner;
    private Vector2 direction = Vector2.right;
    private float lifeTimer;
    private int pierceLeft;
    private float currentDamageMultiplier = 1f;
    private float pierceDamageMultiplier = 1f;
    private bool explosive;
    private float explosionRadius = 1.15f;
    private float explosionDamageMultiplier = 0.8f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.SetVelocity2D(Vector2.zero);
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Despawn(false, false);
    }

    public void SetPool(ProjectilePool pool)
    {
        ownerPool = pool;
    }

    public void Launch(Vector2 dir, int finalDamage, int pierceCount, float pierceDecay, bool isExplosive, float radius, float explosionDamageScale, Transform ownerTransform)
    {
        owner = ownerTransform;
        direction = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
        damage = Mathf.Max(1, finalDamage);
        pierceLeft = Mathf.Max(0, pierceCount);
        currentDamageMultiplier = 1f;
        pierceDamageMultiplier = Mathf.Clamp(pierceDecay, 0.1f, 1f);
        explosive = isExplosive;
        explosionRadius = Mathf.Max(0.1f, radius);
        explosionDamageMultiplier = Mathf.Max(0.1f, explosionDamageScale);
        lifeTimer = lifeTime;

        rb.SetVelocity2D(direction * speed);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger || IsOwner(other.transform))
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Solid"))
        {
            Despawn(true, explosive);
            return;
        }

        if (other.TryGetComponent(out IDamageable damageable))
        {
            int hitDamage = Mathf.Max(1, Mathf.CeilToInt(damage * currentDamageMultiplier));
            damageable.TakeDamage(hitDamage, direction, knockback);
            SfxPlayer.Play(GameSfx.Hit);
            if (explosive)
                SpawnExplosionDamage();

            if (pierceLeft <= 0)
                Despawn(true, false);
            else
            {
                pierceLeft--;
                currentDamageMultiplier *= pierceDamageMultiplier;
            }
        }
    }

    private bool IsOwner(Transform candidate)
    {
        return owner != null && candidate.root == owner.root;
    }

    private void Despawn(bool spawnImpact, bool applyExplosion)
    {
        if (applyExplosion)
            SpawnExplosionDamage();

        if (spawnImpact && impactPrefab != null)
            Instantiate(impactPrefab, transform.position, Quaternion.identity);

        if (ownerPool != null)
            ownerPool.Return(this);
        else
            Destroy(gameObject);
    }

    private void SpawnExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        int areaDamage = Mathf.Max(1, Mathf.CeilToInt(damage * explosionDamageMultiplier));
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.isTrigger || IsOwner(hit.transform))
                continue;

            if (hit.TryGetComponent(out IDamageable damageable))
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                damageable.TakeDamage(areaDamage, dir, knockback * 0.5f);
            }
        }
    }
}
