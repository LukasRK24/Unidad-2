using UnityEngine;
using UnityEngine.Audio;

// Singleton de audio: escucha los eventos del juego y pone los sonidos y la musica.
// Todo pasa por el AudioMixer para poder subir o bajar el volumen de musica y efectos por separado
public class ControladorAudio : Singleton<ControladorAudio>
{
    public const string ParametroMusica = "VolumenMusica";
    public const string ParametroEfectos = "VolumenEfectos";
    private const string PrefMusica = "volumen_musica";
    private const string PrefEfectos = "volumen_efectos";

    [Header("Fuentes y mezclador")]
    [SerializeField] private AudioSource fuenteMusica;
    [SerializeField] private AudioSource fuenteEfectos;
    [SerializeField] private AudioMixer mezclador;

    [Header("Musica")]
    [SerializeField] private AudioClip musica;

    [Header("Efectos")]
    [SerializeField] private AudioClip salto;
    [SerializeField] private AudioClip aterrizaje;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip herido;
    [SerializeField] private AudioClip muerte;
    [SerializeField] private AudioClip fruta;
    [SerializeField] private AudioClip enemigoDerrotado;
    [SerializeField] private AudioClip nivelCompletado;
    [SerializeField] private AudioClip pausaEntrar;
    [SerializeField] private AudioClip pausaSalir;
    [SerializeField] private AudioClip avisoMeta;

    public float VolumenMusica { get; private set; } = 0.8f;
    public float VolumenEfectos { get; private set; } = 1f;

    protected override void Awake()
    {
        base.Awake();

        // Se leen los volumenes guardados antes de que el menu arme sus deslizadores
        VolumenMusica = PlayerPrefs.GetFloat(PrefMusica, VolumenMusica);
        VolumenEfectos = PlayerPrefs.GetFloat(PrefEfectos, VolumenEfectos);
    }

    private void OnEnable()
    {
        EventosJuego.AlCambiarVida += ManejarCambioVida;
        EventosJuego.AlSaltarJugador += ManejarSalto;
        EventosJuego.AlAterrizarJugador += ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador += ManejarDash;
        EventosJuego.AlHerirJugador += ManejarHerido;
        EventosJuego.AlMorirJugador += ManejarMuerte;
        EventosJuego.AlRecogerFruta += ManejarFruta;
        EventosJuego.AlDerrotarEnemigo += ManejarEnemigoDerrotado;
        EventosJuego.AlCompletarNivel += ManejarNivelCompletado;
        EventosJuego.AlCambiarPausa += ManejarPausa;
        EventosJuego.AlMostrarAviso += ManejarAviso;
    }

    private void OnDisable()
    {
        EventosJuego.AlCambiarVida -= ManejarCambioVida;
        EventosJuego.AlSaltarJugador -= ManejarSalto;
        EventosJuego.AlAterrizarJugador -= ManejarAterrizaje;
        EventosJuego.AlHacerDashJugador -= ManejarDash;
        EventosJuego.AlHerirJugador -= ManejarHerido;
        EventosJuego.AlMorirJugador -= ManejarMuerte;
        EventosJuego.AlRecogerFruta -= ManejarFruta;
        EventosJuego.AlDerrotarEnemigo -= ManejarEnemigoDerrotado;
        EventosJuego.AlCompletarNivel -= ManejarNivelCompletado;
        EventosJuego.AlCambiarPausa -= ManejarPausa;
        EventosJuego.AlMostrarAviso -= ManejarAviso;
    }

    private void Start()
    {
        AjustarVolumenMusica(VolumenMusica);
        AjustarVolumenEfectos(VolumenEfectos);

        // Musica de fondo en bucle
        if (musica != null && fuenteMusica != null)
        {
            fuenteMusica.clip = musica;
            fuenteMusica.loop = true;
            fuenteMusica.Play();
        }
    }

    public void AjustarVolumenMusica(float lineal)
    {
        VolumenMusica = Mathf.Clamp01(lineal);
        PlayerPrefs.SetFloat(PrefMusica, VolumenMusica);
        AplicarVolumen(ParametroMusica, VolumenMusica, fuenteMusica);
    }

    public void AjustarVolumenEfectos(float lineal)
    {
        VolumenEfectos = Mathf.Clamp01(lineal);
        PlayerPrefs.SetFloat(PrefEfectos, VolumenEfectos);
        AplicarVolumen(ParametroEfectos, VolumenEfectos, fuenteEfectos);
    }

    // El mezclador trabaja en decibelios, por eso se convierte el volumen de 0 a 1
    private void AplicarVolumen(string parametro, float lineal, AudioSource respaldo)
    {
        float decibelios = Mathf.Log10(Mathf.Max(lineal, 0.0001f)) * 20f;
        if (mezclador == null || !mezclador.SetFloat(parametro, decibelios))
        {
            respaldo.volume = lineal;
        }
    }

    private void Reproducir(AudioClip clip, float volumen = 1f, bool variarPitch = false)
    {
        if (clip == null || fuenteEfectos == null) return;
        
        // Fatiga auditiva: Variar pitch aleatoriamente
        if (variarPitch)
        {
            fuenteEfectos.pitch = Random.Range(0.9f, 1.1f);
        }
        else
        {
            fuenteEfectos.pitch = 1f;
        }
        
        fuenteEfectos.PlayOneShot(clip, volumen);
    }

    private float pitchMusicaBase = 1f;

    private void ManejarSalto(Vector3 posicion) => Reproducir(salto, 0.8f, true);
    private void ManejarAterrizaje(Vector3 posicion) => Reproducir(aterrizaje, 0.6f, true);
    private void ManejarDash(Vector3 posicion, float direccion) => Reproducir(dash, 0.85f, true);
    private void ManejarHerido(Vector3 posicion) => Reproducir(herido, 1f, true);
    private void ManejarMuerte() => Reproducir(muerte, 1f, false);
    private void ManejarFruta(Vector3 posicion) => Reproducir(fruta, 1f, true);
    private void ManejarEnemigoDerrotado(Vector3 posicion) => Reproducir(enemigoDerrotado, 1f, true);
    private void ManejarNivelCompletado() => Reproducir(nivelCompletado, 1f, false);
    private void ManejarAviso(string mensaje) => Reproducir(avisoMeta, 0.7f, false);

    private void ManejarCambioVida(int actual, int maxima)
    {
        // Musica Adaptativa (Tension): Sube el tempo (pitch) si queda 1 vida
        pitchMusicaBase = (actual == 1 && actual < maxima) ? 1.15f : 1f;
        
        if (fuenteMusica != null && ControladorJuego.Instancia != null && !ControladorJuego.Instancia.EnPausa)
        {
            fuenteMusica.pitch = pitchMusicaBase;
        }
    }

    private void ManejarPausa(bool enPausa) 
    {
        Reproducir(enPausa ? pausaEntrar : pausaSalir);
        
        // Muffling (Filtro LowPass improvisado) mediante Pitch al pausar
        if (fuenteMusica != null)
        {
            fuenteMusica.pitch = enPausa ? 0.75f : pitchMusicaBase;
        }
    }
}
