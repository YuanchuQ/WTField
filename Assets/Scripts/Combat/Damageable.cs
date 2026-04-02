using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invulnerabilityDuration = 0.15f;
    [SerializeField] private float flashTime = 0.08f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    private int currentHealth;
    private float invulnerabilityTimer;
    private float invulnerabilityTotal;
    private bool dead;
    private Color originalColor = Color.white;
    private Coroutine flashRoutine;
    private float damageReduction;
    private bool preventOneShot;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => dead;

    public event Action<int, int> HealthChanged;
    public event Action<Vector2> Damaged;
    public event Action Died;

    private void Awake()
    {
        CacheReferences();
        ResetHealth();
    }

    private void OnEnable()
    {
        if (currentHealth <= 0 || dead)
            ResetHealth();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        dead = false;
        invulnerabilityTimer = 0f;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 hitDirection, float knockback)
    {
        if (dead || amount <= 0 || invulnerabilityTimer > 0f)
            return;

        int finalAmount = ApplyDefense(amount);
        currentHealth = Mathf.Max(0, currentHealth - finalAmount);
        invulnerabilityTimer = invulnerabilityDuration;
        invulnerabilityTotal = invulnerabilityDuration;
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke(hitDirection);

        if (rb != null && knockback > 0f)
            rb.AddForce(hitDirection.normalized * knockback, ForceMode2D.Impulse);

        if (spriteRenderer != null)
            Flash();

        if (currentHealth <= 0)
        {
            dead = true;
            Died?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (dead || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void HealPercent(float percent)
    {
        if (dead || percent <= 0f)
            return;

        Heal(Mathf.CeilToInt(maxHealth * percent));
    }

    public void HealToFull()
    {
        if (dead)
            return;

        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void GrantInvulnerability(float seconds)
    {
        if (dead || seconds <= 0f)
            return;

        invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, seconds);
        invulnerabilityTotal = Mathf.Max(invulnerabilityTotal, seconds);
        if (spriteRenderer != null)
            Flash();
    }

    public void SetDefenseBuff(float reduction, bool oneShotProtection)
    {
        damageReduction = Mathf.Clamp01(reduction);
        preventOneShot = oneShotProtection;
    }

    private int ApplyDefense(int amount)
    {
        int finalAmount = Mathf.Max(1, Mathf.CeilToInt(amount * (1f - damageReduction)));
        if (preventOneShot && currentHealth > 1 && finalAmount >= currentHealth)
            finalAmount = currentHealth - 1;

        return finalAmount;
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < invulnerabilityTotal)
        {
            if (spriteRenderer == null)
                yield break;

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime;

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
