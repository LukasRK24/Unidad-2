using UnityEngine;

// Solo la vida y el danio del jugador. Avisa por eventos, no conoce la interfaz
[RequireComponent(typeof(MovimientoJugador))]
public class VidaJugador : MonoBehaviour, IRecibeDanio
{
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private float segundosInvulnerable = 1.2f;
    [SerializeField] private Vector2 empuje = new Vector2(7f, 9f);

    private MovimientoJugador movimiento;
    private int vidaActual;
    private float invulnerableHasta;
    private Vector3 posicionInicial;

    public int VidaActual => vidaActual;
    public bool EstaMuerto => vidaActual <= 0;

    private void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
        vidaActual = vidaMaxima;
        posicionInicial = transform.position;
    }

    private void Start()
    {
        EventosJuego.CambiarVida(vidaActual, vidaMaxima);
    }

    public void RecibirDanio(int cantidad, Vector2 puntoGolpe)
    {
        // Mientras esta invulnerable no recibe danio, asi un solo contacto no lo mata de golpe
        if (EstaMuerto || Time.time < invulnerableHasta) return;

        invulnerableHasta = Time.time + segundosInvulnerable;
        vidaActual = Mathf.Max(0, vidaActual - cantidad);
        EventosJuego.CambiarVida(vidaActual, vidaMaxima);
        EventosJuego.HerirJugador(transform.position);
        EventosJuego.SacudirCamara(0.2f, 0.25f);

        if (EstaMuerto)
        {
            EventosJuego.MorirJugador();
            return;
        }

        // Lo empujamos hacia el lado contrario del golpe
        float sentido = transform.position.x >= puntoGolpe.x ? 1f : -1f;
        movimiento.Empujar(new Vector2(sentido * empuje.x, empuje.y));
    }

    // Para el vacio: quita 1 vida y reaparece, a menos que sea la ultima
    public void Matar()
    {
        if (EstaMuerto) return;

        vidaActual--;
        EventosJuego.CambiarVida(vidaActual, vidaMaxima);
        EventosJuego.HerirJugador(transform.position);

        if (vidaActual <= 0)
        {
            EventosJuego.MorirJugador();
        }
        else
        {
            // Respawn en la posicion inicial
            transform.position = posicionInicial;
            movimiento.ResetearFisicas();
            invulnerableHasta = Time.time + segundosInvulnerable;
            
            // Para resetear la camara al instante
            EventosJuego.MostrarAviso("¡Cuidado con el vacio!");
        }
    }
}
