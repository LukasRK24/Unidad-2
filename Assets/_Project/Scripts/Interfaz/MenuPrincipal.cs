using UnityEngine;
using UnityEngine.UI;

// Menu principal: jugar, salir y los volumenes de musica y efectos
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonSalir;
    [SerializeField] private Slider deslizadorMusica;
    [SerializeField] private Slider deslizadorEfectos;

    private void Start()
    {
        botonJugar.onClick.AddListener(() => CargadorEscenas.Cargar(CargadorEscenas.Nivel01));
        botonSalir.onClick.AddListener(CargadorEscenas.Salir);

        // Los deslizadores arrancan con el volumen guardado, sin disparar el cambio
        deslizadorMusica.SetValueWithoutNotify(ControladorAudio.Instancia.VolumenMusica);
        deslizadorEfectos.SetValueWithoutNotify(ControladorAudio.Instancia.VolumenEfectos);
        deslizadorMusica.onValueChanged.AddListener(ControladorAudio.Instancia.AjustarVolumenMusica);
        deslizadorEfectos.onValueChanged.AddListener(ControladorAudio.Instancia.AjustarVolumenEfectos);

        botonJugar.Select();
    }
}
