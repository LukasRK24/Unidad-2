using UnityEngine;

// SRP: solo física del jugador (correr, saltar, dash). Informa lo que hace mediante GameEvents.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Correr")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 60f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 2.4f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldown = 0.7f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpHeld;
    private float lastJumpRequest = -10f;
    private float lastGroundedTime = -10f;
    private float dashEndTime;
    private float nextDashTime;
    private float baseGravityScale;
    private float lowestVelocityY;
    private bool isDead;

    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public float FacingDirection { get; private set; } = 1f;
    public Vector2 Velocity => rb.linearVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        baseGravityScale = rb.gravityScale;
    }

    private void OnEnable() => GameEvents.OnPlayerDied += HandlePlayerDied;
    private void OnDisable() => GameEvents.OnPlayerDied -= HandlePlayerDied;

    public void Move(float direction) => moveInput = direction;
    public void SetJumpHeld(bool held) => jumpHeld = held;
    public void RequestJump() => lastJumpRequest = Time.time;

    public void RequestDash()
    {
        if (isDead || IsDashing || Time.time < nextDashTime) return;

        IsDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
        rb.gravityScale = 0f;
        GameEvents.PlayerDashed(transform.position, FacingDirection);
        GameEvents.CameraShake(0.1f, 0.12f);
    }

    public void Bounce(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    public void Knockback(Vector2 velocity)
    {
        EndDash();
        rb.linearVelocity = velocity;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        UpdateGroundState();

        if (IsDashing)
        {
            rb.linearVelocity = new Vector2(FacingDirection * dashSpeed, 0f);
            if (Time.time >= dashEndTime) EndDash();
            return;
        }

        ApplyHorizontal();
        TryJump();
        ApplyBetterGravity();
    }

    private void UpdateGroundState()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;

        if (IsGrounded) lastGroundedTime = Time.time;

        if (!IsGrounded)
        {
            lowestVelocityY = Mathf.Min(lowestVelocityY, rb.linearVelocity.y);
        }
        else if (!wasGrounded)
        {
            if (lowestVelocityY < -4f) GameEvents.PlayerLanded(groundCheck.position);
            lowestVelocityY = 0f;
        }
    }

    private void ApplyHorizontal()
    {
        if (Mathf.Abs(moveInput) > 0.05f) FacingDirection = Mathf.Sign(moveInput);

        float target = moveInput * moveSpeed;
        float rate = Mathf.Abs(moveInput) > 0.05f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, target, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void TryJump()
    {
        bool buffered = Time.time - lastJumpRequest <= jumpBufferTime;
        bool coyote = Time.time - lastGroundedTime <= coyoteTime;
        if (!buffered || !coyote) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        lastJumpRequest = -10f;
        lastGroundedTime = -10f;
        IsGrounded = false;
        GameEvents.PlayerJumped(groundCheck.position);
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = baseGravityScale * fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            rb.gravityScale = baseGravityScale * lowJumpGravityMultiplier;
        else
            rb.gravityScale = baseGravityScale;
    }

    private void EndDash()
    {
        if (!IsDashing) return;
        IsDashing = false;
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, 0f);
    }

    private void HandlePlayerDied()
    {
        isDead = true;
        IsDashing = false;
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = Vector2.zero;
    }
}
