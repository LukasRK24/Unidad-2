using UnityEngine;

// SRP: resuelve el contacto con enemigos (pisotón, dash o recibir daño).
[RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth), typeof(Collider2D))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float stompBounce = 12f;

    private PlayerMovement movement;
    private PlayerHealth health;
    private Collider2D body;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        body = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) => Resolve(collision);
    private void OnCollisionStay2D(Collision2D collision) => Resolve(collision);

    private void Resolve(Collision2D collision)
    {
        if (health.IsDead) return;
        if (!collision.collider.TryGetComponent(out EnemyPatrol enemy)) return;
        if (enemy.IsDefeated) return;

        Vector2 contact = collision.GetContact(0).point;
        bool stomp = body.bounds.min.y > collision.collider.bounds.center.y;

        if (stomp)
        {
            enemy.TakeDamage(1, contact);
            movement.Bounce(stompBounce);
        }
        else if (movement.IsDashing)
        {
            enemy.TakeDamage(1, contact);
        }
        else
        {
            health.TakeDamage(1, contact);
        }
    }
}
