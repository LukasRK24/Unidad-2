using System.Collections;
using UnityEngine;

// Enemigo que camina de lado a lado. Da la vuelta si hay pared o se acaba el piso,
// y desaparece cuando recibe danio
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemigoPatrulla : MonoBehaviour, IRecibeDanio
{
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private Transform detectorPared;
    [SerializeField] private Transform detectorBorde;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private SpriteRenderer renderizador;
    [SerializeField] private AnimadorSprites animadorSprites;
    [SerializeField] private Sprite[] spritesGolpe;

    private Rigidbody2D rb;
    private float direccion = -1f;

    public bool EstaDerrotado { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (EstaDerrotado) return;

        // Un rayo hacia adelante para la pared y otro hacia abajo para el borde
        bool hayPared = Physics2D.Raycast(detectorPared.position, Vector2.right * direccion, 0.3f, capaSuelo);
        bool haySuelo = Physics2D.Raycast(detectorBorde.position, Vector2.down, 1.2f, capaSuelo);
        if (hayPared || !haySuelo) Voltear();

        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    private void Voltear()
    {
        direccion = -direccion;
        renderizador.flipX = direccion > 0f;

        // Los detectores se pasan al lado nuevo
        detectorPared.localPosition = new Vector3(Mathf.Abs(detectorPared.localPosition.x) * direccion, detectorPared.localPosition.y, 0f);
        detectorBorde.localPosition = new Vector3(Mathf.Abs(detectorBorde.localPosition.x) * direccion, detectorBorde.localPosition.y, 0f);
    }

    public void RecibirDanio(int cantidad, Vector2 puntoGolpe)
    {
        if (EstaDerrotado) return;

        EstaDerrotado = true;
        EventosJuego.DerrotarEnemigo(transform.position);
        EventosJuego.SacudirCamara(0.08f, 0.1f);

        // Se apaga el collider y la fisica para que no moleste mientras desaparece
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        animadorSprites.Reproducir(spritesGolpe, 14f, false);
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        yield return new WaitForSeconds(0.45f);
        Destroy(gameObject);
    }
}
