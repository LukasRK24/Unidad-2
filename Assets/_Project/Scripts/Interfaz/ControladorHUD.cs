using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// El HUD solo escucha eventos: no conoce al jugador ni revisa nada en Update().
// Cuando cambia la vida o las frutas, el evento le trae el numero nuevo
public class ControladorHUD : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private Image barraVida;
    [SerializeField] private TMP_Text textoVida;
    [SerializeField] private Image superposicionDanio;

    [Header("Frutas y mensajes")]
    [SerializeField] private TMP_Text textoFrutas;
    [SerializeField] private TMP_Text textoAviso;

    private Coroutine rutinaParpadeo;
    private Coroutine rutinaDestello;
    private Coroutine rutinaAviso;
    private Color colorVida;

    private void Awake()
    {
        colorVida = barraVida.color;
        superposicionDanio.color = new Color(1f, 0f, 0f, 0f);
        textoAviso.text = string.Empty;
    }

    private void OnEnable()
    {
        EventosJuego.AlCambiarVida += ActualizarVida;
        EventosJuego.AlCambiarFrutas += ActualizarFrutas;
        EventosJuego.AlHerirJugador += ManejarHerido;
        EventosJuego.AlMostrarAviso += MostrarAviso;
    }

    private void OnDisable()
    {
        EventosJuego.AlCambiarVida -= ActualizarVida;
        EventosJuego.AlCambiarFrutas -= ActualizarFrutas;
        EventosJuego.AlHerirJugador -= ManejarHerido;
        EventosJuego.AlMostrarAviso -= MostrarAviso;
    }

    private void ActualizarVida(int actual, int maxima)
    {
        barraVida.fillAmount = maxima > 0 ? (float)actual / maxima : 0f;
        textoVida.text = actual + "/" + maxima;
    }

    private void ActualizarFrutas(int recogidas, int total)
    {
        textoFrutas.text = recogidas + " / " + total;
    }

    private void ManejarHerido(Vector3 posicion)
    {
        // Al recibir danio la barra parpadea y la pantalla se tiñe de rojo un momento
        if (rutinaParpadeo != null) StopCoroutine(rutinaParpadeo);
        if (rutinaDestello != null) StopCoroutine(rutinaDestello);
        rutinaParpadeo = StartCoroutine(ParpadearBarra());
        rutinaDestello = StartCoroutine(DestelloRojo());
    }

    private void MostrarAviso(string mensaje)
    {
        if (rutinaAviso != null) StopCoroutine(rutinaAviso);
        rutinaAviso = StartCoroutine(RutinaAviso(mensaje));
    }

    // Se usa tiempo real (unscaled) para que siga funcionando aunque el juego este en pausa
    private IEnumerator ParpadearBarra()
    {
        for (int i = 0; i < 6; i++)
        {
            barraVida.color = i % 2 == 0 ? Color.white : colorVida;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        barraVida.color = colorVida;
    }

    private IEnumerator DestelloRojo()
    {
        float duracion = 0.3f;
        float transcurrido = 0f;
        while (transcurrido < duracion)
        {
            transcurrido += Time.unscaledDeltaTime;
            superposicionDanio.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.45f, 0f, transcurrido / duracion));
            yield return null;
        }
        superposicionDanio.color = new Color(1f, 0f, 0f, 0f);
    }

    private IEnumerator RutinaAviso(string mensaje)
    {
        textoAviso.text = mensaje;
        yield return new WaitForSecondsRealtime(2.2f);
        textoAviso.text = string.Empty;
    }
}
