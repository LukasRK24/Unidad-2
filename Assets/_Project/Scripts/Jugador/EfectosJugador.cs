using System.Collections;
using UnityEngine;

// Efectos visuales del jugador: estirar y aplastar al saltar o aterrizar,
// y parpadeo cuando recibe danio
public class EfectosJugador : MonoBehaviour
{
    [SerializeField] private Transform modeloVisual;
    [SerializeField] private SpriteRenderer renderizador;
    [SerializeField] private float velocidadRecuperar = 14f;
    [SerializeField] private float segundosParpadeo = 1.2f;

    private Vector3 escalaInicial;
    private Coroutine rutinaAplastar;
    private Coroutine rutinaParpadeo;

    private void Awake()
    {
        escalaInicial = modeloVisual.localScale;
    }

    private void OnEnable()
    {
        EventosJuego.AlSaltarJugador += ManejarSalto;
        EventosJuego.AlAterrizarJugador += ManejarAterrizaje;
        EventosJuego.AlHerirJugador += ManejarHerido;
    }

    private void OnDisable()
    {
        EventosJuego.AlSaltarJugador -= ManejarSalto;
        EventosJuego.AlAterrizarJugador -= ManejarAterrizaje;
        EventosJuego.AlHerirJugador -= ManejarHerido;
    }

    private void ManejarSalto(Vector3 posicion) => Aplastar(new Vector3(0.75f, 1.3f, 1f));
    private void ManejarAterrizaje(Vector3 posicion) => Aplastar(new Vector3(1.3f, 0.7f, 1f));

    private void ManejarHerido(Vector3 posicion)
    {
        if (rutinaParpadeo != null) StopCoroutine(rutinaParpadeo);
        rutinaParpadeo = StartCoroutine(Parpadear());
    }

    private void Aplastar(Vector3 deformacion)
    {
        if (rutinaAplastar != null) StopCoroutine(rutinaAplastar);
        rutinaAplastar = StartCoroutine(EfectoAplastar(deformacion));
    }

    // Efecto visual de estirar y aplastar para que el personaje se sienta vivo
    private IEnumerator EfectoAplastar(Vector3 deformacion)
    {
        modeloVisual.localScale = Vector3.Scale(escalaInicial, deformacion);
        while (Vector3.Distance(modeloVisual.localScale, escalaInicial) > 0.01f)
        {
            modeloVisual.localScale = Vector3.Lerp(modeloVisual.localScale, escalaInicial, Time.deltaTime * velocidadRecuperar);
            yield return null;
        }
        modeloVisual.localScale = escalaInicial;
    }

    private IEnumerator Parpadear()
    {
        float transcurrido = 0f;
        while (transcurrido < segundosParpadeo)
        {
            renderizador.enabled = !renderizador.enabled;
            yield return new WaitForSeconds(0.08f);
            transcurrido += 0.08f;
        }
        renderizador.enabled = true;
    }
}
