using UnityEngine;
using UnityEngine.InputSystem;

// SRP: solo lee dispositivos y le pasa órdenes a PlayerMovement.
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        float horizontal = 0f;
        bool jumpDown = false, jumpHeld = false, dash = false, pause = false, restart = false;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
            jumpDown = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            jumpHeld = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
            dash = kb.leftShiftKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame;
            pause = kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
            restart = kb.rKey.wasPressedThisFrame;
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            float stick = pad.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.2f) horizontal += stick;
            jumpDown |= pad.buttonSouth.wasPressedThisFrame;
            jumpHeld |= pad.buttonSouth.isPressed;
            dash |= pad.buttonWest.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame;
            pause |= pad.startButton.wasPressedThisFrame;
        }

        if (pause) GameEvents.PauseRequested();
        if (restart) GameEvents.RestartRequested();

        if (Time.timeScale == 0f) return;

        movement.Move(Mathf.Clamp(horizontal, -1f, 1f));
        movement.SetJumpHeld(jumpHeld);
        if (jumpDown) movement.RequestJump();
        if (dash) movement.RequestDash();
    }
}
