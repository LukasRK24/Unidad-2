using UnityEngine;

// Singleton: solo lleva el estado de la partida (frutas, pausa y fin de nivel).
// No dibuja nada ni mueve nada, eso lo hacen los demas scripts al escuchar sus eventos
public class ControladorJuego : Singleton<ControladorJuego>
{
    public int FrutasRecogidas { get; private set; }
    public int FrutasTotal { get; private set; }
    public bool EnPausa { get; private set; }
    public bool Terminado { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        EventosJuego.AlRecogerFruta += ManejarFruta;
        EventosJuego.AlLlegarMeta += ManejarMeta;
        EventosJuego.AlMorirJugador += ManejarMuerte;
        EventosJuego.AlPedirPausa += AlternarPausa;
        EventosJuego.AlPedirReinicio += CargadorEscenas.RecargarActual;
    }

    private void OnDisable()
    {
        EventosJuego.AlRecogerFruta -= ManejarFruta;
        EventosJuego.AlLlegarMeta -= ManejarMeta;
        EventosJuego.AlMorirJugador -= ManejarMuerte;
        EventosJuego.AlPedirPausa -= AlternarPausa;
        EventosJuego.AlPedirReinicio -= CargadorEscenas.RecargarActual;
    }

    protected override void OnDestroy()
    {
        Time.timeScale = 1f;
        base.OnDestroy();
    }

    private void Start()
    {
        // Contamos las frutas de la escena para que el contador nunca salga 0 / 0
        FrutasTotal = FindObjectsByType<Coleccionable>().Length;
        EventosJuego.CambiarFrutas(FrutasRecogidas, FrutasTotal);
    }

    private void ManejarFruta(Vector3 posicion)
    {
        FrutasRecogidas++;
        EventosJuego.CambiarFrutas(FrutasRecogidas, FrutasTotal);
    }

    private void ManejarMeta()
    {
        if (Terminado) return;

        // Si faltan frutas no se termina, solo se avisa
        if (FrutasRecogidas < FrutasTotal)
        {
            EventosJuego.MostrarAviso("Te faltan " + (FrutasTotal - FrutasRecogidas) + " frutas para terminar");
            return;
        }

        Terminado = true;
        EventosJuego.CompletarNivel();
    }

    private void ManejarMuerte()
    {
        Terminado = true;
    }

    public void AlternarPausa()
    {
        // No se puede pausar cuando ya termino la partida
        if (Terminado) return;

        EnPausa = !EnPausa;
        Time.timeScale = EnPausa ? 0f : 1f;
        EventosJuego.CambiarPausa(EnPausa);
    }
}
