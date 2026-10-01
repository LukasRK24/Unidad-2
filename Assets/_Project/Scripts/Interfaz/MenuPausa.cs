using UnityEngine;
using UnityEngine.UI;

// Menu de pausa. Se muestra o se oculta cuando llega el evento de pausa.
// Este script va en un objeto aparte del panel, porque el panel se apaga y el script tiene que seguir escuchando
public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonMenu;

    private void Awake()
    {
        panel.SetActive(false);
        botonContinuar.onClick.AddListener(EventosJuego.PedirPausa);
        botonReiniciar.onClick.AddListener(EventosJuego.PedirReinicio);
        botonMenu.onClick.AddListener(() => CargadorEscenas.Cargar(CargadorEscenas.MenuPrincipal));
    }

    private void OnEnable() => EventosJuego.AlCambiarPausa += MostrarOcultar;
    private void OnDisable() => EventosJuego.AlCambiarPausa -= MostrarOcultar;

    private void MostrarOcultar(bool enPausa)
    {
        panel.SetActive(enPausa);

        // El primer boton queda seleccionado para poder navegar sin mouse
        if (enPausa) botonContinuar.Select();
    }
}
