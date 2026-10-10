using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de fin de partida. Es el mismo script para Victoria y para Game Over,
// cambia el evento que escucha segun el modo
public class PanelFinPartida : MonoBehaviour
{
    public enum Modo { Victoria, FinDelJuego }

    [SerializeField] private Modo modo;
    [SerializeField] private float retrasoMostrar = 0.8f;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titulo;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonMenu;

    private void Awake()
    {
        panel.SetActive(false);
        botonReiniciar.onClick.AddListener(EventosJuego.PedirReinicio);
        botonMenu.onClick.AddListener(() => CargadorEscenas.Cargar(CargadorEscenas.MenuPrincipal));
    }

    private void OnEnable()
    {
        if (modo == Modo.Victoria) EventosJuego.AlCompletarNivel += Mostrar;
        else EventosJuego.AlMorirJugador += Mostrar;
    }

    private void OnDisable()
    {
        if (modo == Modo.Victoria) EventosJuego.AlCompletarNivel -= Mostrar;
        else EventosJuego.AlMorirJugador -= Mostrar;
    }

    // Se usa WaitForSecondsRealtime porque si hay Hit-Stop activo, Time.timeScale es 0 y se buguea
    private void Mostrar() => StartCoroutine(RutinaMostrar());

    private System.Collections.IEnumerator RutinaMostrar()
    {
        yield return new WaitForSecondsRealtime(retrasoMostrar);
        panel.SetActive(true);
        botonReiniciar.Select();
    }
}
