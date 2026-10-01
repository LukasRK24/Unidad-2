using UnityEngine;

// Resuelve el choque del jugador con los enemigos:
// si cae encima lo derrota y rebota, si va en dash tambien, y si lo toca de costado recibe danio
[RequireComponent(typeof(MovimientoJugador), typeof(VidaJugador), typeof(Collider2D))]
public class CombateJugador : MonoBehaviour
{
    [SerializeField] private float rebotePisoton = 12f;

    private MovimientoJugador movimiento;
    private VidaJugador vida;
    private Collider2D cuerpo;

    private void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
        vida = GetComponent<VidaJugador>();
        cuerpo = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D colision) => Resolver(colision);
    private void OnCollisionStay2D(Collision2D colision) => Resolver(colision);

    private void Resolver(Collision2D colision)
    {
        if (vida.EstaMuerto) return;
        if (!colision.collider.TryGetComponent(out EnemigoPatrulla enemigo)) return;
        if (enemigo.EstaDerrotado) return;

        Vector2 punto = colision.GetContact(0).point;

        // Es pisoton si los pies del jugador estan por encima del centro del enemigo
        bool pisoton = cuerpo.bounds.min.y > colision.collider.bounds.center.y;

        if (pisoton)
        {
            enemigo.RecibirDanio(1, punto);
            movimiento.Rebotar(rebotePisoton);
        }
        else if (movimiento.EnDash)
        {
            enemigo.RecibirDanio(1, punto);
        }
        else
        {
            vida.RecibirDanio(1, punto);
        }
    }
}
