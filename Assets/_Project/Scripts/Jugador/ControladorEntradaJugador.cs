using UnityEngine;
using UnityEngine.InputSystem;

// Solo lee teclado y mando y le pasa las ordenes a MovimientoJugador.
// No mueve nada por si mismo, asi el movimiento queda separado del input
[RequireComponent(typeof(MovimientoJugador))]
public class ControladorEntradaJugador : MonoBehaviour
{
    private MovimientoJugador movimiento;

    private void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
    }

    private void Update()
    {
        float horizontal = 0f;
        bool saltoPresionado = false;
        bool saltoSostenido = false;
        bool dash = false;
        bool pausa = false;
        bool reiniciar = false;

        // Teclado: flechas o A/D para moverse, arriba/W/Espacio para saltar
        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) horizontal -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) horizontal += 1f;

            saltoPresionado = teclado.spaceKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame;
            saltoSostenido = teclado.spaceKey.isPressed || teclado.wKey.isPressed || teclado.upArrowKey.isPressed;
            dash = teclado.leftShiftKey.wasPressedThisFrame || teclado.jKey.wasPressedThisFrame;
            pausa = teclado.escapeKey.wasPressedThisFrame || teclado.pKey.wasPressedThisFrame;
            reiniciar = teclado.rKey.wasPressedThisFrame;
        }

        // Mando (opcional)
        Gamepad mando = Gamepad.current;
        if (mando != null)
        {
            float palanca = mando.leftStick.x.ReadValue();
            if (Mathf.Abs(palanca) > 0.2f) horizontal += palanca;

            saltoPresionado |= mando.buttonSouth.wasPressedThisFrame;
            saltoSostenido |= mando.buttonSouth.isPressed;
            dash |= mando.buttonWest.wasPressedThisFrame || mando.rightShoulder.wasPressedThisFrame;
            pausa |= mando.startButton.wasPressedThisFrame;
        }

        // Pausa y reinicio se avisan por evento, los escucha ControladorJuego
        if (pausa) EventosJuego.PedirPausa();
        if (reiniciar) EventosJuego.PedirReinicio();

        // Con el juego en pausa el personaje no recibe ordenes
        if (Time.timeScale == 0f) return;

        movimiento.Mover(Mathf.Clamp(horizontal, -1f, 1f));
        movimiento.MantenerSalto(saltoSostenido);
        if (saltoPresionado) movimiento.PedirSalto();
        if (dash) movimiento.PedirDash();
    }
}
