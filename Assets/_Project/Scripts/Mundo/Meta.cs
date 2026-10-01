using UnityEngine;

// Meta del nivel. Avisa cuando el jugador llega y ControladorJuego decide si ya se puede terminar
[RequireComponent(typeof(Collider2D))]
public class Meta : MonoBehaviour
{
    [SerializeField] private float segundosRepeticion = 2f;

    private float proximoAviso;

    // Cada cierto tiempo vuelve a avisar para que el mensaje de "faltan frutas" se repita si se queda ahi
    private void OnTriggerStay2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player") || Time.time < proximoAviso) return;

        proximoAviso = Time.time + segundosRepeticion;
        EventosJuego.LlegarMeta();
    }
}
