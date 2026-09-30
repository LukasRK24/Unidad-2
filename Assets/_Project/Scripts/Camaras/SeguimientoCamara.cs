using System.Collections;
using UnityEngine;

// Camara que sigue al jugador con zona muerta (dead zone), suavizado (damping),
// anticipo hacia donde va, limites del nivel y sacudida cuando pasa algo
[RequireComponent(typeof(Camera))]
public class SeguimientoCamara : MonoBehaviour
{
    [Header("Seguimiento del objetivo")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private Vector2 desfase = new Vector2(0f, 1.5f);
    [SerializeField] private Vector2 zonaMuerta = new Vector2(1.5f, 1f);
    [SerializeField] private float tiempoSuavizado = 0.25f;

    [Header("Anticipo")]
    [SerializeField] private float distanciaAnticipo = 2f;
    [SerializeField] private float suavizadoAnticipo = 4f;

    [Header("Limites del nivel")]
    [SerializeField] private Vector2 limiteMin = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 limiteMax = new Vector2(100f, 100f);

    private Camera camara;
    private Rigidbody2D cuerpoObjetivo;
    private Vector3 velocidadReferencia;
    private Vector3 desplazamientoSacudida;
    private float anticipo;
    private Coroutine rutinaSacudida;

    private void Awake()
    {
        camara = GetComponent<Camera>();
        if (objetivo != null) cuerpoObjetivo = objetivo.GetComponent<Rigidbody2D>();
    }

    private void OnEnable() => EventosJuego.AlSacudirCamara += Sacudir;
    private void OnDisable() => EventosJuego.AlSacudirCamara -= Sacudir;

    private void Start()
    {
        if (objetivo == null) return;

        // Posicionamos la camara de inmediato cerca del objetivo
        Vector3 inicio = LimitarAlNivel(PosicionDeseada(transform.position));
        transform.position = new Vector3(inicio.x, inicio.y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;

        // Anticipo: la camara se adelanta un poco hacia donde corre el jugador
        float anticipoObjetivo = 0f;
        if (cuerpoObjetivo != null && Mathf.Abs(cuerpoObjetivo.linearVelocity.x) > 0.5f)
        {
            anticipoObjetivo = Mathf.Sign(cuerpoObjetivo.linearVelocity.x) * distanciaAnticipo;
        }
        anticipo = Mathf.Lerp(anticipo, anticipoObjetivo, Time.deltaTime * suavizadoAnticipo);

        Vector3 actual = transform.position - desplazamientoSacudida;
        Vector3 deseada = LimitarAlNivel(PosicionDeseada(actual));
        Vector3 suave = Vector3.SmoothDamp(actual, deseada, ref velocidadReferencia, tiempoSuavizado);
        suave.z = transform.position.z;
        transform.position = suave + desplazamientoSacudida;
    }

    // La camara solo se mueve cuando el jugador sale de la zona muerta
    private Vector3 PosicionDeseada(Vector3 actual)
    {
        Vector2 foco = (Vector2)objetivo.position + desfase + new Vector2(anticipo, 0f);
        float x = actual.x;
        float y = actual.y;

        float dx = foco.x - x;
        if (Mathf.Abs(dx) > zonaMuerta.x) x += dx - Mathf.Sign(dx) * zonaMuerta.x;

        float dy = foco.y - y;
        if (Mathf.Abs(dy) > zonaMuerta.y) y += dy - Mathf.Sign(dy) * zonaMuerta.y;

        return new Vector3(x, y, actual.z);
    }

    // Que la camara no muestre nada fuera del nivel
    private Vector3 LimitarAlNivel(Vector3 posicion)
    {
        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;
        posicion.x = LimitarEje(posicion.x, limiteMin.x + mitadAncho, limiteMax.x - mitadAncho);
        posicion.y = LimitarEje(posicion.y, limiteMin.y + mitadAlto, limiteMax.y - mitadAlto);
        return posicion;
    }

    private static float LimitarEje(float valor, float minimo, float maximo)
    {
        if (minimo > maximo) return (minimo + maximo) * 0.5f;
        return Mathf.Clamp(valor, minimo, maximo);
    }

    private void Sacudir(float duracion, float fuerza)
    {
        if (rutinaSacudida != null) StopCoroutine(rutinaSacudida);
        rutinaSacudida = StartCoroutine(RutinaSacudida(duracion, fuerza));
    }

    private IEnumerator RutinaSacudida(float duracion, float fuerza)
    {
        float transcurrido = 0f;
        while (transcurrido < duracion)
        {
            Vector2 azar = Random.insideUnitCircle * fuerza;
            desplazamientoSacudida = new Vector3(azar.x, azar.y, 0f);
            transcurrido += Time.unscaledDeltaTime;
            yield return null;
        }
        desplazamientoSacudida = Vector3.zero;
    }
}
