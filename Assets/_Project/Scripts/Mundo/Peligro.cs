using UnityEngine;

// Los picos o el vacio: si el jugador los toca recibe danio.
// Con muerteInstantanea mata de una (se usa para la zona de caida)
[RequireComponent(typeof(Collider2D))]
public class Peligro : MonoBehaviour
{
    [SerializeField] private int danio = 1;
    [SerializeField] private bool muerteInstantanea;

    // Stay y no Enter para que lo siga lastimando si se queda encima cuando termina la invulnerabilidad
    private void OnTriggerStay2D(Collider2D otro)
    {
        if (!otro.TryGetComponent(out VidaJugador vida)) return;

        if (muerteInstantanea) vida.Matar();
        else vida.RecibirDanio(danio, transform.position);
    }
}
