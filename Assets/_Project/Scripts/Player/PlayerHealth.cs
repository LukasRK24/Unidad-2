using UnityEngine;

// SRP: solo vida y daño. Avisa con eventos; no conoce la UI ni el audio.
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilitySeconds = 1.2f;
    [SerializeField] private Vector2 knockback = new Vector2(7f, 9f);

    private PlayerMovement movement;
    private int currentHealth;
    private float invulnerableUntil;

    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameEvents.HealthChanged(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 hitPoint)
    {
        if (IsDead || Time.time < invulnerableUntil) return;

        invulnerableUntil = Time.time + invulnerabilitySeconds;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        GameEvents.HealthChanged(currentHealth, maxHealth);
        GameEvents.PlayerHurt(transform.position);
        GameEvents.CameraShake(0.2f, 0.25f);

        if (IsDead)
        {
            GameEvents.PlayerDied();
            return;
        }

        float away = transform.position.x >= hitPoint.x ? 1f : -1f;
        movement.Knockback(new Vector2(away * knockback.x, knockback.y));
    }

    public void Kill()
    {
        if (IsDead) return;
        currentHealth = 0;
        GameEvents.HealthChanged(currentHealth, maxHealth);
        GameEvents.PlayerHurt(transform.position);
        GameEvents.PlayerDied();
    }
}
