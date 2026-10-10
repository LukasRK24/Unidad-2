using System.Collections;
using UnityEngine;

// --- PATRÓN STATE (MÁQUINA DE ESTADOS) ---
public interface IEstadoEnemigo
{
    void Entrar(EnemigoPatrulla enemigo);
    void Actualizar(EnemigoPatrulla enemigo);
}

// Estado: Caminar de lado a lado
public class EstadoPatrullar : IEstadoEnemigo
{
    public void Entrar(EnemigoPatrulla enemigo) { }

    public void Actualizar(EnemigoPatrulla enemigo)
    {
        bool hayPared = Physics2D.Raycast(enemigo.DetectorPared.position, Vector2.right * enemigo.Direccion, 0.3f, enemigo.CapaSuelo);
        bool haySuelo = Physics2D.Raycast(enemigo.DetectorBorde.position, Vector2.down, 1.2f, enemigo.CapaSuelo);
        
        if (hayPared || !haySuelo) enemigo.Voltear();

        enemigo.CuerpoRigido.linearVelocity = new Vector2(enemigo.Direccion * enemigo.Velocidad, enemigo.CuerpoRigido.linearVelocity.y);
    }
}

// Estado: Muerto (Desactiva físicas para la animación de muerte)
public class EstadoMuerto : IEstadoEnemigo
{
    public void Entrar(EnemigoPatrulla enemigo)
    {
        enemigo.GetComponent<Collider2D>().enabled = false;
        enemigo.CuerpoRigido.linearVelocity = Vector2.zero;
        enemigo.CuerpoRigido.simulated = false;
    }

    public void Actualizar(EnemigoPatrulla enemigo) { }
}

// Contexto de la Máquina de Estados
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemigoPatrulla : MonoBehaviour, IRecibeDanio
{
    [Header("Configuracion")]
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private Transform detectorPared;
    [SerializeField] private Transform detectorBorde;
    [SerializeField] private LayerMask capaSuelo;
    
    [Header("Visuales")]
    [SerializeField] private SpriteRenderer renderizador;
    [SerializeField] private AnimadorSprites animadorSprites;
    [SerializeField] private Sprite[] spritesGolpe;

    // Propiedades expuestas para los Estados
    public float Velocidad => velocidad;
    public Transform DetectorPared => detectorPared;
    public Transform DetectorBorde => detectorBorde;
    public LayerMask CapaSuelo => capaSuelo;
    public Rigidbody2D CuerpoRigido { get; private set; }
    public float Direccion { get; private set; } = -1f;

    private IEstadoEnemigo estadoActual;

    public bool EstaDerrotado { get; private set; }
    public bool EstaMuerto => EstaDerrotado;

    public void Matar() => RecibirDanio(999, transform.position);

    private void Awake()
    {
        CuerpoRigido = GetComponent<Rigidbody2D>();
        CuerpoRigido.freezeRotation = true;
        CambiarEstado(new EstadoPatrullar());
    }

    public void CambiarEstado(IEstadoEnemigo nuevoEstado)
    {
        estadoActual = nuevoEstado;
        estadoActual.Entrar(this);
    }

    private void FixedUpdate()
    {
        if (EstaDerrotado) return;
        estadoActual?.Actualizar(this);
    }

    public void Voltear()
    {
        Direccion = -Direccion;
        renderizador.flipX = Direccion > 0f;

        detectorPared.localPosition = new Vector3(Mathf.Abs(detectorPared.localPosition.x) * Direccion, detectorPared.localPosition.y, 0f);
        detectorBorde.localPosition = new Vector3(Mathf.Abs(detectorBorde.localPosition.x) * Direccion, detectorBorde.localPosition.y, 0f);
    }

    public void RecibirDanio(int cantidad, Vector2 puntoGolpe)
    {
        if (EstaDerrotado) return;

        EstaDerrotado = true;
        CambiarEstado(new EstadoMuerto());
        
        EventosJuego.DerrotarEnemigo(transform.position);
        EventosJuego.SacudirCamara(0.08f, 0.1f);

        animadorSprites.Reproducir(spritesGolpe, 14f, false);
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        yield return new WaitForSeconds(0.45f);
        Destroy(gameObject);
    }
}
