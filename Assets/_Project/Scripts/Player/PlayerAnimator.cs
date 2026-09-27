using UnityEngine;

// Puente entre el estado del jugador y el Animator Controller (la FSM de animación).
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int DashingHash = Animator.StringToHash("Dashing");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHurt += HandleHurt;
        GameEvents.OnPlayerDied += HandleDied;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHurt -= HandleHurt;
        GameEvents.OnPlayerDied -= HandleDied;
    }

    private void Update()
    {
        animator.SetFloat(SpeedHash, Mathf.Abs(movement.Velocity.x));
        animator.SetFloat(VelocityYHash, movement.Velocity.y);
        animator.SetBool(GroundedHash, movement.IsGrounded);
        animator.SetBool(DashingHash, movement.IsDashing);
        spriteRenderer.flipX = movement.FacingDirection < 0f;
    }

    private void HandleHurt(Vector3 position) => animator.SetTrigger(HurtHash);
    private void HandleDied() => animator.SetBool(DeadHash, true);
}
