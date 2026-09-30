using UnityEngine;

// Conecta lo que hace el jugador con el Animator Controller (la maquina de estados).
// Aqui solo se mandan los parametros, las transiciones estan en el controlador
[RequireComponent(typeof(MovimientoJugador))]
public class AnimadorJugador : MonoBehaviour
{
    [SerializeField] private Animator animador;
    [SerializeField] private SpriteRenderer renderizador;

    private static readonly int VelocidadHash = Animator.StringToHash("Velocidad");
    private static readonly int VelocidadYHash = Animator.StringToHash("VelocidadY");
    private static readonly int EnSueloHash = Animator.StringToHash("EnSuelo");
    private static readonly int EnDashHash = Animator.StringToHash("EnDash");
    private static readonly int HeridoHash = Animator.StringToHash("Herido");
    private static readonly int MuertoHash = Animator.StringToHash("Muerto");

    private MovimientoJugador movimiento;

    private void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
    }

    private void OnEnable()
    {
        EventosJuego.AlHerirJugador += ManejarHerido;
        EventosJuego.AlMorirJugador += ManejarMuerte;
    }

    private void OnDisable()
    {
        EventosJuego.AlHerirJugador -= ManejarHerido;
        EventosJuego.AlMorirJugador -= ManejarMuerte;
    }

    private void Update()
    {
        animador.SetFloat(VelocidadHash, Mathf.Abs(movimiento.Velocidad.x));
        animador.SetFloat(VelocidadYHash, movimiento.Velocidad.y);
        animador.SetBool(EnSueloHash, movimiento.EnSuelo);
        animador.SetBool(EnDashHash, movimiento.EnDash);

        // Voltear el sprite segun la direccion donde camina
        renderizador.flipX = movimiento.DireccionMirada < 0f;
    }

    private void ManejarHerido(Vector3 posicion) => animador.SetTrigger(HeridoHash);
    private void ManejarMuerte() => animador.SetBool(MuertoHash, true);
}
