using System.Collections;
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
    private Coroutine rutinaParpadeo;

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

    private void ManejarHerido(Vector3 posicion)
    {
        animador.SetTrigger(HeridoHash);
        if (rutinaParpadeo != null) StopCoroutine(rutinaParpadeo);
        rutinaParpadeo = StartCoroutine(RutinaParpadeoInvulnerabilidad());
    }

    private IEnumerator RutinaParpadeoInvulnerabilidad()
    {
        // El jugador es invulnerable por 1.2 segundos (definido en VidaJugador)
        float tiempoFin = Time.time + 1.2f;
        while (Time.time < tiempoFin)
        {
            renderizador.enabled = !renderizador.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        renderizador.enabled = true;
    }

    private void ManejarMuerte()
    {
        if (rutinaParpadeo != null) StopCoroutine(rutinaParpadeo);
        renderizador.enabled = true;
        animador.SetBool(MuertoHash, true);
    }
}
