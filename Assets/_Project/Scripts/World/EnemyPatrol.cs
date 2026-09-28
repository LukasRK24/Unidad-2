using System.Collections;
using UnityEngine;

// Enemigo que patrulla, da la vuelta en paredes y bordes, y muere al recibir daño.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyPatrol : MonoBehaviour, IDamageable
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteFrameAnimator frameAnimator;
    [SerializeField] private Sprite[] hitFrames;

    private Rigidbody2D rb;
    private float direction = -1f;

    public bool IsDefeated { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (IsDefeated) return;

        bool wallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, 0.3f, groundMask);
        bool groundAhead = Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1.2f, groundMask);
        if (wallAhead || !groundAhead) Flip();

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        direction = -direction;
        spriteRenderer.flipX = direction > 0f;
        wallCheck.localPosition = new Vector3(Mathf.Abs(wallCheck.localPosition.x) * direction, wallCheck.localPosition.y, 0f);
        ledgeCheck.localPosition = new Vector3(Mathf.Abs(ledgeCheck.localPosition.x) * direction, ledgeCheck.localPosition.y, 0f);
    }

    public void TakeDamage(int amount, Vector2 hitPoint)
    {
        if (IsDefeated) return;

        IsDefeated = true;
        GameEvents.EnemyDefeated(transform.position);
        GameEvents.CameraShake(0.08f, 0.1f);
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        frameAnimator.Play(hitFrames, 14f, false);
        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(0.45f);
        Destroy(gameObject);
    }
}
