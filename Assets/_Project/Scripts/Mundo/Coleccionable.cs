using UnityEngine;

// Fruta que se recoge. Solo avisa que la recogieron, el conteo lo lleva ControladorJuego
[RequireComponent(typeof(Collider2D))]
public class Coleccionable : MonoBehaviour
{
    [Header("Efecto de flotacion")]
    [SerializeField] private float velocidadFlotacion = 3f;
    [SerializeField] private float alturaFlotacion = 0.15f;

    private Vector3 posicionInicial;
    private bool recolectada;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        // Flotacion senoidal sencilla
        transform.position = posicionInicial + Vector3.up * (Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (recolectada || !otro.CompareTag("Player")) return;

        recolectada = true;
        EventosJuego.RecogerFruta(transform.position);
        Destroy(gameObject);
    }
}
