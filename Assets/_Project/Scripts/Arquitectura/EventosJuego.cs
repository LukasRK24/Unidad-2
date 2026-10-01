using System;
using UnityEngine;

// Patron Observer: aqui pasan todos los avisos del juego.
// Quien avisa no sabe quien escucha, asi el jugador no conoce la interfaz ni el audio.
// Al lado de cada evento dice quien lo dispara (D) y quien lo escucha (E).
public static class EventosJuego
{
    // D: VidaJugador | E: ControladorHUD
    public static event Action<int, int> AlCambiarVida;
    // D: ControladorJuego | E: ControladorHUD
    public static event Action<int, int> AlCambiarFrutas;
    // D: MovimientoJugador | E: ControladorAudio, ControladorEfectos, EfectosJugador
    public static event Action<Vector3> AlSaltarJugador;
    public static event Action<Vector3> AlAterrizarJugador;
    public static event Action<Vector3, float> AlHacerDashJugador;
    // D: VidaJugador | E: ControladorAudio, ControladorEfectos, ControladorHUD, EfectosJugador, AnimadorJugador
    public static event Action<Vector3> AlHerirJugador;
    // D: VidaJugador | E: ControladorJuego, ControladorAudio, PanelFinPartida, MovimientoJugador, AnimadorJugador
    public static event Action AlMorirJugador;
    // D: Coleccionable | E: ControladorJuego, ControladorAudio, ControladorEfectos
    public static event Action<Vector3> AlRecogerFruta;
    // D: EnemigoPatrulla | E: ControladorAudio, ControladorEfectos
    public static event Action<Vector3> AlDerrotarEnemigo;
    // D: Meta | E: ControladorJuego
    public static event Action AlLlegarMeta;
    // D: ControladorJuego | E: ControladorAudio, PanelFinPartida
    public static event Action AlCompletarNivel;
    // D: ControladorJuego | E: ControladorHUD, ControladorAudio
    public static event Action<string> AlMostrarAviso;
    // D: VidaJugador, MovimientoJugador, EnemigoPatrulla | E: SeguimientoCamara
    public static event Action<float, float> AlSacudirCamara;
    // D: ControladorEntradaJugador, botones del menu | E: ControladorJuego
    public static event Action AlPedirPausa;
    public static event Action AlPedirReinicio;
    // D: ControladorJuego | E: MenuPausa, ControladorAudio
    public static event Action<bool> AlCambiarPausa;

    public static void CambiarVida(int actual, int maxima) => AlCambiarVida?.Invoke(actual, maxima);
    public static void CambiarFrutas(int recogidas, int total) => AlCambiarFrutas?.Invoke(recogidas, total);
    public static void SaltarJugador(Vector3 posicion) => AlSaltarJugador?.Invoke(posicion);
    public static void AterrizarJugador(Vector3 posicion) => AlAterrizarJugador?.Invoke(posicion);
    public static void HacerDashJugador(Vector3 posicion, float direccion) => AlHacerDashJugador?.Invoke(posicion, direccion);
    public static void HerirJugador(Vector3 posicion) => AlHerirJugador?.Invoke(posicion);
    public static void MorirJugador() => AlMorirJugador?.Invoke();
    public static void RecogerFruta(Vector3 posicion) => AlRecogerFruta?.Invoke(posicion);
    public static void DerrotarEnemigo(Vector3 posicion) => AlDerrotarEnemigo?.Invoke(posicion);
    public static void LlegarMeta() => AlLlegarMeta?.Invoke();
    public static void CompletarNivel() => AlCompletarNivel?.Invoke();
    public static void MostrarAviso(string mensaje) => AlMostrarAviso?.Invoke(mensaje);
    public static void SacudirCamara(float duracion, float fuerza) => AlSacudirCamara?.Invoke(duracion, fuerza);
    public static void PedirPausa() => AlPedirPausa?.Invoke();
    public static void PedirReinicio() => AlPedirReinicio?.Invoke();
    public static void CambiarPausa(bool enPausa) => AlCambiarPausa?.Invoke(enPausa);
}
