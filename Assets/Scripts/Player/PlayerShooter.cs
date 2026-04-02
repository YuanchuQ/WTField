using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private float fireInterval = 0.18f;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float shootingMovePenalty = 0.75f;

    private float fireTimer;
    private float fireRateMultiplier = 1f;
    private float damageMultiplier = 1f;
    private int pierceCount;
    private float pierceDamageMultiplier = 1f;
    private bool explosiveProjectiles;
    private float explosionRadius = 1.15f;
    private float explosionDamageMultiplier = 0.8f;
    private bool canShoot = true;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        bool firing = canShoot && IsFirePressed();
        if (controller != null)
            controller.SetShootingMoveMultiplier(firing ? shootingMovePenalty : 1f);

        if (firing && fireTimer <= 0f)
            Shoot();
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.SetShootingMoveMultiplier(1f);
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    public void SetFireRateMultiplier(float value)
    {
        fireRateMultiplier = Mathf.Max(0.1f, value);
    }

    public void SetDamageMultiplier(float value)
    {
        damageMultiplier = Mathf.Max(0.1f, value);
    }

    public void SetPierce(int value, float damageDecay)
    {
        pierceCount = Mathf.Max(0, value);
        pierceDamageMultiplier = Mathf.Clamp(damageDecay, 0.1f, 1f);
    }

    public void SetExplosiveProjectiles(bool enabled, float radius, float damageScale)
    {
        explosiveProjectiles = enabled;
        explosionRadius = Mathf.Max(0.1f, radius);
        explosionDamageMultiplier = Mathf.Max(0.1f, damageScale);
    }

    public void SetShootingMovePenalty(float value)
    {
        shootingMovePenalty = Mathf.Clamp(value, 0.1f, 1f);
    }

    private void Shoot()
    {
        if (projectilePrefab == null && projectilePool == null)
            return;

        Vector2 dir = controller != null ? controller.AimDirection : Vector2.right;
        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;

        Projectile projectile = projectilePool != null
            ? projectilePool.Get(spawnPos, Quaternion.identity)
            : Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        int finalDamage = Mathf.Max(1, Mathf.CeilToInt(baseDamage * damageMultiplier));
        projectile.Launch(dir, finalDamage, pierceCount, pierceDamageMultiplier, explosiveProjectiles, explosionRadius, explosionDamageMultiplier, transform);
        SfxPlayer.Play(GameSfx.Shoot);

        fireTimer = fireInterval / fireRateMultiplier;
    }

    private bool IsFirePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.jKey.isPressed;
#else
        return Input.GetKey(KeyCode.J);
#endif
    }
}
