using UnityEngine;

// Solo la fisica del jugador: caminar, saltar y dash.
// Lo que hace lo avisa con EventosJuego, no toca la interfaz ni el audio
[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 8f;
    [SerializeField] private float aceleracion = 70f;
    [SerializeField] private float desaceleracion = 60f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 15f;
    [SerializeField] private float multiplicadorCaida = 1.8f;
    [SerializeField] private float multiplicadorSaltoCorto = 2.4f;
    [SerializeField] private float tiempoCoyote = 0.12f;
    [SerializeField] private float tiempoBufferSalto = 0.12f;

    [Header("Suelo")]
    [SerializeField] private Transform detectorSuelo;
    [SerializeField] private float radioSuelo = 0.25f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Dash")]
    [SerializeField] private float velocidadDash = 20f;
    [SerializeField] private float duracionDash = 0.16f;
    [SerializeField] private float enfriamientoDash = 0.7f;

    private Rigidbody2D rb;
    private float direccionX;
    private bool saltoSostenido;
    private float ultimoPedidoSalto = -10f;
    private float ultimoMomentoEnSuelo = -10f;
    private float finDash;
    private float proximoDash;
    private float gravedadBase;
    private float caidaMasRapida;
    private bool estaMuerto;

    public bool EnSuelo { get; private set; }
    public bool EnDash { get; private set; }
    public float DireccionMirada { get; private set; } = 1f;
    public Vector2 Velocidad => rb.linearVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Evitar que el personaje gire al chocar con esquinas
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        gravedadBase = rb.gravityScale;
    }

    private void OnEnable() => EventosJuego.AlMorirJugador += ManejarMuerte;
    private void OnDisable() => EventosJuego.AlMorirJugador -= ManejarMuerte;

    // Estos metodos los llama ControladorEntradaJugador
    public void Mover(float direccion) => direccionX = direccion;
    public void MantenerSalto(bool sostenido) => saltoSostenido = sostenido;
    public void PedirSalto() => ultimoPedidoSalto = Time.time;

    public void PedirDash()
    {
        if (estaMuerto || EnDash || Time.time < proximoDash) return;

        EnDash = true;
        finDash = Time.time + duracionDash;
        proximoDash = Time.time + enfriamientoDash;

        // En el dash no hay gravedad para que vaya recto
        rb.gravityScale = 0f;
        EventosJuego.HacerDashJugador(transform.position, DireccionMirada);
        EventosJuego.SacudirCamara(0.1f, 0.12f);
    }

    // Rebote al pisar un enemigo
    public void Rebotar(float fuerza)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerza);
    }

    // Empujon cuando recibe un golpe
    public void Empujar(Vector2 velocidad)
    {
        TerminarDash();
        rb.linearVelocity = velocidad;
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;

        ActualizarSuelo();

        // Si estamos haciendo dash, solo se procesa ese impulso
        if (EnDash)
        {
            rb.linearVelocity = new Vector2(DireccionMirada * velocidadDash, 0f);
            if (Time.time >= finDash) TerminarDash();
            return;
        }

        AplicarMovimientoHorizontal();
        IntentarSaltar();
        AjustarGravedad();
    }

    private void ActualizarSuelo()
    {
        bool estabaEnSuelo = EnSuelo;
        EnSuelo = Physics2D.OverlapCircle(detectorSuelo.position, radioSuelo, capaSuelo) != null;

        if (EnSuelo) ultimoMomentoEnSuelo = Time.time;

        if (!EnSuelo)
        {
            // Guardamos la caida mas fuerte para saber si el aterrizaje fue de verdad
            caidaMasRapida = Mathf.Min(caidaMasRapida, rb.linearVelocity.y);
        }
        else if (!estabaEnSuelo)
        {
            // Si recien toca el suelo tras caer
            if (caidaMasRapida < -4f) EventosJuego.AterrizarJugador(detectorSuelo.position);
            caidaMasRapida = 0f;
        }
    }

    private void AplicarMovimientoHorizontal()
    {
        if (Mathf.Abs(direccionX) > 0.05f) DireccionMirada = Mathf.Sign(direccionX);

        float velocidadObjetivo = direccionX * velocidadCaminar;
        float tasa = Mathf.Abs(direccionX) > 0.05f ? aceleracion : desaceleracion;
        float nuevoX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivo, tasa * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(nuevoX, rb.linearVelocity.y);
    }

    private void IntentarSaltar()
    {
        // Buffer: si apreto salto un poquito antes de tocar el suelo igual salta
        // Coyote: si recien se cayo de una plataforma todavia puede saltar
        bool conBuffer = Time.time - ultimoPedidoSalto <= tiempoBufferSalto;
        bool conCoyote = Time.time - ultimoMomentoEnSuelo <= tiempoCoyote;
        if (!conBuffer || !conCoyote) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        ultimoPedidoSalto = -10f;
        ultimoMomentoEnSuelo = -10f;
        EnSuelo = false;
        EventosJuego.SaltarJugador(detectorSuelo.position);
    }

    private void AjustarGravedad()
    {
        // Al caer baja con mas peso para que no parezca que flota
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = gravedadBase * multiplicadorCaida;
        }
        // Si solto el boton de saltar rapido, corta el salto
        else if (rb.linearVelocity.y > 0f && !saltoSostenido)
        {
            rb.gravityScale = gravedadBase * multiplicadorSaltoCorto;
        }
        else
        {
            rb.gravityScale = gravedadBase;
        }
    }

    private void TerminarDash()
    {
        if (!EnDash) return;

        EnDash = false;
        rb.gravityScale = gravedadBase;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, 0f);
    }

    private void ManejarMuerte()
    {
        estaMuerto = true;
        EnDash = false;
        rb.gravityScale = gravedadBase;
        rb.linearVelocity = Vector2.zero;
    }

    // Dibujamos el radio de deteccion en la escena para verificar en el editor
    private void OnDrawGizmosSelected()
    {
        if (detectorSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(detectorSuelo.position, radioSuelo);
        }
    }
}
