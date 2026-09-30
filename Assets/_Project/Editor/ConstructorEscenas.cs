#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Arma las dos escenas del juego (MainMenu y Level_01) con todos sus objetos
public static class ConstructorEscenas
{
    private const string Escenas = UtilConstructor.Raiz + "/Scenes/";
    private const string Sonidos = UtilConstructor.Raiz + "/Audio/";

    private const string TextoControles =
        "<b><color=#FFE066>CÓMO JUGAR</color></b>\n" +
        "Moverse: flechas izquierda y derecha, o A y D\n" +
        "Saltar: flecha arriba, W o Espacio\n" +
        "(si mantienes el salto apretado, saltas más alto)\n" +
        "Dash: Shift o J (también derrota enemigos)\n" +
        "Pausa: Esc o P\n" +
        "Reiniciar el nivel: R";

    private const string TextoObjetivo =
        "<b><color=#FFE066>OBJETIVO</color></b>\n" +
        "Recoge todas las frutas y llega al trofeo.\n" +
        "Pisa a los cerdos o hazles dash para derrotarlos.\n" +
        "Cuidado: los picos y los cerdos quitan vida, y si caes al vacío pierdes.\n" +
        "Tienes 3 vidas.";

    public static void ConstruirNivel(AnimatorController controlador, AudioMixer mezclador, ParticleSystem[] particulas, int capaSuelo, int capaEnemigo, int capaJugador)
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CrearLuzGlobal();

        GameObject mundo = new GameObject("Mundo");
        ConstructorNivel.ConstruirTilemap(mundo.transform, capaSuelo);

        GameObject jugador = ConstructorPrefabs.ConstruirJugador(controlador, capaSuelo, capaJugador);
        GameObject enemigo = ConstructorPrefabs.ConstruirEnemigo(capaSuelo, capaEnemigo);
        GameObject fruta = ConstructorPrefabs.ConstruirFruta();
        GameObject picos = ConstructorPrefabs.ConstruirPicos();
        GameObject meta = ConstructorPrefabs.ConstruirMeta();

        GameObject entidades = new GameObject("Entidades");
        ConstructorNivel.ColocarEntidades(entidades.transform, jugador, enemigo, fruta, picos, meta, capaSuelo);
        GameObject jugadorEnEscena = GameObject.FindGameObjectWithTag("Player");

        ConstruirCamara(jugadorEnEscena.transform);
        ConstruirControladores(mezclador, particulas);
        ConstruirHud();
        ConstructorInterfaz.CrearSistemaEventos();

        EditorSceneManager.SaveScene(escena, Escenas + "Level_01.unity");
    }

    public static void ConstruirMenuPrincipal(AudioMixer mezclador)
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject objetoCamara = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera camara = objetoCamara.AddComponent<Camera>();
        camara.orthographic = true;
        camara.backgroundColor = new Color(0.45f, 0.7f, 0.95f);
        objetoCamara.AddComponent<AudioListener>();

        GameObject controladores = new GameObject("Controladores");
        AgregarControladorAudio(controladores, mezclador);

        Canvas canvas = ConstructorInterfaz.CrearCanvas("CanvasMenu");
        Image fondo = ConstructorInterfaz.CrearImagen("Fondo", canvas.transform, Color.white, UtilConstructor.CargarSprite(UtilConstructor.Raiz + "/Art/Background/Mountains.png"));
        ConstructorInterfaz.Anclar(fondo.rectTransform, Vector2.zero, Vector2.one);
        fondo.preserveAspect = false;

        CrearFondo(canvas.transform, "FondoTitulo", new Vector2(0f, 285f), new Vector2(1150f, 250f));
        CrearFondo(canvas.transform, "FondoDeslizadores", new Vector2(0f, -265f), new Vector2(820f, 190f));
        CrearFondo(canvas.transform, "FondoControles", new Vector2(-670f, -60f), new Vector2(560f, 430f));
        CrearFondo(canvas.transform, "FondoObjetivo", new Vector2(670f, -60f), new Vector2(560f, 430f));

        TextMeshProUGUI titulo = ConstructorInterfaz.CrearTexto("Titulo", canvas.transform, "UNIDAD 2", 130f, TextAlignmentOptions.Center, Color.white);
        titulo.rectTransform.anchoredPosition = new Vector2(0f, 330f);
        titulo.rectTransform.sizeDelta = new Vector2(1200f, 160f);
        titulo.fontStyle = FontStyles.Bold;
        TextMeshProUGUI subtitulo = ConstructorInterfaz.CrearTexto("Subtitulo", canvas.transform, "Arquitectura de Sistemas Integrados", 44f, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.9f));
        subtitulo.rectTransform.anchoredPosition = new Vector2(0f, 230f);
        subtitulo.rectTransform.sizeDelta = new Vector2(1200f, 70f);

        Button jugar = ConstructorInterfaz.CrearBoton("BotonJugar", canvas.transform, "JUGAR", new Vector2(0f, 80f), new Vector2(520f, 110f));
        Button salir = ConstructorInterfaz.CrearBoton("BotonSalir", canvas.transform, "SALIR", new Vector2(0f, -60f), new Vector2(520f, 110f));

        TextMeshProUGUI etiquetaMusica = ConstructorInterfaz.CrearTexto("EtiquetaMusica", canvas.transform, "Música", 36f, TextAlignmentOptions.Right, Color.white);
        etiquetaMusica.rectTransform.anchoredPosition = new Vector2(-330f, -220f);
        etiquetaMusica.rectTransform.sizeDelta = new Vector2(240f, 60f);
        Slider musica = ConstructorInterfaz.CrearDeslizador("DeslizadorMusica", canvas.transform, new Vector2(40f, -220f), new Vector2(420f, 50f));
        TextMeshProUGUI etiquetaEfectos = ConstructorInterfaz.CrearTexto("EtiquetaEfectos", canvas.transform, "Efectos", 36f, TextAlignmentOptions.Right, Color.white);
        etiquetaEfectos.rectTransform.anchoredPosition = new Vector2(-330f, -310f);
        etiquetaEfectos.rectTransform.sizeDelta = new Vector2(240f, 60f);
        Slider efectos = ConstructorInterfaz.CrearDeslizador("DeslizadorEfectos", canvas.transform, new Vector2(40f, -310f), new Vector2(420f, 50f));

        TextoEnCaja(canvas.transform, "TextoControles", TextoControles, new Vector2(-670f, -60f), new Vector2(520f, 400f));
        TextoEnCaja(canvas.transform, "TextoObjetivo", TextoObjetivo, new Vector2(670f, -60f), new Vector2(520f, 400f));

        MenuPrincipal menu = canvas.gameObject.AddComponent<MenuPrincipal>();
        UtilConstructor.AsignarReferencia(menu, "botonJugar", jugar);
        UtilConstructor.AsignarReferencia(menu, "botonSalir", salir);
        UtilConstructor.AsignarReferencia(menu, "deslizadorMusica", musica);
        UtilConstructor.AsignarReferencia(menu, "deslizadorEfectos", efectos);

        ConstructorInterfaz.CrearSistemaEventos();
        EditorSceneManager.SaveScene(escena, Escenas + "MainMenu.unity");
    }

    private static void TextoEnCaja(Transform padre, string nombre, string contenido, Vector2 posicion, Vector2 tamano)
    {
        TextMeshProUGUI texto = ConstructorInterfaz.CrearTexto(nombre, padre, contenido, 27f, TextAlignmentOptions.TopLeft, Color.white);
        texto.rectTransform.anchoredPosition = posicion;
        texto.rectTransform.sizeDelta = tamano;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.lineSpacing = 8f;
    }

    private static void CrearFondo(Transform padre, string nombre, Vector2 posicion, Vector2 tamano)
    {
        Image imagen = ConstructorInterfaz.CrearImagen(nombre, padre, new Color(0.05f, 0.08f, 0.14f, 0.7f), ConstructorInterfaz.SpriteCaja);
        imagen.rectTransform.anchoredPosition = posicion;
        imagen.rectTransform.sizeDelta = tamano;
        imagen.raycastTarget = false;
    }

    // Sin luz global 2D los sprites se ven negros en URP 2D
    private static void CrearLuzGlobal()
    {
        GameObject objeto = new GameObject("Luz Global 2D");
        Light2D luz = objeto.AddComponent<Light2D>();
        luz.lightType = Light2D.LightType.Global;
        luz.intensity = 1f;
    }

    private static void ConstruirCamara(Transform jugador)
    {
        GameObject objetoCamara = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera camara = objetoCamara.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = 7f;
        camara.backgroundColor = new Color(0.45f, 0.7f, 0.95f);
        camara.clearFlags = CameraClearFlags.SolidColor;
        objetoCamara.AddComponent<AudioListener>();
        objetoCamara.transform.position = new Vector3(0f, 3f, -10f);

        SeguimientoCamara seguimiento = objetoCamara.AddComponent<SeguimientoCamara>();
        UtilConstructor.AsignarReferencia(seguimiento, "objetivo", jugador);
        UtilConstructor.AsignarVector2(seguimiento, "limiteMin", ConstructorNivel.LimiteMin);
        UtilConstructor.AsignarVector2(seguimiento, "limiteMax", ConstructorNivel.LimiteMax);

        // El fondo es hijo de la camara para que siempre cubra la pantalla
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(objetoCamara.transform, false);
        fondo.transform.localPosition = new Vector3(0f, 0f, 20f);
        fondo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
        SpriteRenderer sr = fondo.AddComponent<SpriteRenderer>();
        sr.sprite = UtilConstructor.CargarSprite(UtilConstructor.Raiz + "/Art/Background/Mountains.png");
        sr.sortingOrder = -50;
    }

    private static void ConstruirControladores(AudioMixer mezclador, ParticleSystem[] particulas)
    {
        GameObject controladores = new GameObject("Controladores");
        controladores.AddComponent<ControladorJuego>();
        AgregarControladorAudio(controladores, mezclador);

        GameObject objetoEfectos = new GameObject("ControladorEfectos");
        objetoEfectos.transform.SetParent(controladores.transform);
        ControladorEfectos efectos = objetoEfectos.AddComponent<ControladorEfectos>();
        UtilConstructor.AsignarReferencia(efectos, "prefabPolvo", particulas[0]);
        UtilConstructor.AsignarReferencia(efectos, "prefabDash", particulas[1]);
        UtilConstructor.AsignarReferencia(efectos, "prefabChispa", particulas[2]);
        UtilConstructor.AsignarReferencia(efectos, "prefabGolpe", particulas[3]);
    }

    private static void AgregarControladorAudio(GameObject padre, AudioMixer mezclador)
    {
        GameObject objetoAudio = new GameObject("ControladorAudio");
        objetoAudio.transform.SetParent(padre.transform);
        ControladorAudio audio = objetoAudio.AddComponent<ControladorAudio>();

        AudioSource fuenteMusica = new GameObject("FuenteMusica").AddComponent<AudioSource>();
        fuenteMusica.transform.SetParent(objetoAudio.transform);
        fuenteMusica.playOnAwake = false;
        AudioSource fuenteEfectos = new GameObject("FuenteEfectos").AddComponent<AudioSource>();
        fuenteEfectos.transform.SetParent(objetoAudio.transform);
        fuenteEfectos.playOnAwake = false;

        // Cada fuente va a su grupo del mezclador para controlar el volumen por separado
        if (mezclador != null)
        {
            fuenteMusica.outputAudioMixerGroup = mezclador.FindMatchingGroups("Musica")[0];
            fuenteEfectos.outputAudioMixerGroup = mezclador.FindMatchingGroups("Efectos")[0];
        }

        UtilConstructor.AsignarReferencia(audio, "fuenteMusica", fuenteMusica);
        UtilConstructor.AsignarReferencia(audio, "fuenteEfectos", fuenteEfectos);
        UtilConstructor.AsignarReferencia(audio, "mezclador", mezclador);
        UtilConstructor.AsignarReferencia(audio, "musica", Clip("Music/bgm_main.wav"));
        UtilConstructor.AsignarReferencia(audio, "salto", Clip("SFX/jump.wav"));
        UtilConstructor.AsignarReferencia(audio, "aterrizaje", Clip("SFX/land.wav"));
        UtilConstructor.AsignarReferencia(audio, "dash", Clip("SFX/dash.wav"));
        UtilConstructor.AsignarReferencia(audio, "herido", Clip("SFX/player_hurt.wav"));
        UtilConstructor.AsignarReferencia(audio, "muerte", Clip("SFX/player_death.wav"));
        UtilConstructor.AsignarReferencia(audio, "fruta", Clip("SFX/fruit.wav"));
        UtilConstructor.AsignarReferencia(audio, "enemigoDerrotado", Clip("SFX/enemy_defeat.wav"));
        UtilConstructor.AsignarReferencia(audio, "nivelCompletado", Clip("SFX/level_complete.wav"));
        UtilConstructor.AsignarReferencia(audio, "pausaEntrar", Clip("SFX/pause_in.wav"));
        UtilConstructor.AsignarReferencia(audio, "pausaSalir", Clip("SFX/pause_out.wav"));
        UtilConstructor.AsignarReferencia(audio, "avisoMeta", Clip("SFX/goal_hint.wav"));
    }

    private static AudioClip Clip(string ruta) => AssetDatabase.LoadAssetAtPath<AudioClip>(Sonidos + ruta);

    private static void ConstruirHud()
    {
        Canvas canvas = ConstructorInterfaz.CrearCanvas("CanvasHUD");
        Transform raiz = canvas.transform;

        // Barra de vida arriba a la izquierda
        Vector2 arribaIzquierda = new Vector2(0f, 1f);
        RectTransform rectCorazon = ConstructorInterfaz.CrearRect("IconoCorazon", raiz, arribaIzquierda, arribaIzquierda, arribaIzquierda, new Vector2(40f, -35f), new Vector2(70f, 70f));
        Image corazon = rectCorazon.gameObject.AddComponent<Image>();
        corazon.sprite = UtilConstructor.CargarSprite(UtilConstructor.Raiz + "/Art/UI/Heart.png");
        corazon.raycastTarget = false;

        RectTransform fondoBarra = ConstructorInterfaz.CrearRect("FondoBarraVida", raiz, arribaIzquierda, arribaIzquierda, arribaIzquierda, new Vector2(125f, -45f), new Vector2(420f, 50f));
        Image fondo = fondoBarra.gameObject.AddComponent<Image>();
        fondo.sprite = ConstructorInterfaz.SpriteCaja;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        fondo.raycastTarget = false;

        Image relleno = ConstructorInterfaz.CrearImagen("RellenoVida", fondoBarra, new Color(0.9f, 0.2f, 0.25f), ConstructorInterfaz.SpriteCaja);
        ConstructorInterfaz.Anclar(relleno.rectTransform, Vector2.zero, Vector2.one);
        relleno.rectTransform.offsetMin = new Vector2(4f, 4f);
        relleno.rectTransform.offsetMax = new Vector2(-4f, -4f);
        relleno.type = Image.Type.Filled;
        relleno.fillMethod = Image.FillMethod.Horizontal;
        relleno.fillOrigin = (int)Image.OriginHorizontal.Left;
        relleno.fillAmount = 1f;
        relleno.raycastTarget = false;

        TextMeshProUGUI textoVida = ConstructorInterfaz.CrearTexto("TextoVida", fondoBarra, "3/3", 32f, TextAlignmentOptions.Center, Color.white);
        ConstructorInterfaz.Anclar(textoVida.rectTransform, Vector2.zero, Vector2.one);

        // Contador de frutas arriba a la derecha
        Vector2 arribaDerecha = new Vector2(1f, 1f);
        RectTransform fondoFrutas = ConstructorInterfaz.CrearRect("FondoFrutas", raiz, arribaDerecha, arribaDerecha, arribaDerecha, new Vector2(-30f, -30f), new Vector2(330f, 80f));
        Image imagenFondoFrutas = fondoFrutas.gameObject.AddComponent<Image>();
        imagenFondoFrutas.sprite = ConstructorInterfaz.SpriteCaja;
        imagenFondoFrutas.type = Image.Type.Sliced;
        imagenFondoFrutas.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
        imagenFondoFrutas.raycastTarget = false;

        RectTransform rectFruta = ConstructorInterfaz.CrearRect("IconoFruta", raiz, arribaDerecha, arribaDerecha, arribaDerecha, new Vector2(-260f, -25f), new Vector2(80f, 80f));
        Image iconoFruta = rectFruta.gameObject.AddComponent<Image>();
        iconoFruta.sprite = UtilConstructor.CargarSprite(UtilConstructor.Raiz + "/Art/Items/Fruit_Apple.png");
        iconoFruta.raycastTarget = false;
        TextMeshProUGUI textoFrutas = ConstructorInterfaz.CrearTexto("TextoFrutas", raiz, "0 / 0", 54f, TextAlignmentOptions.Left, Color.white);
        textoFrutas.rectTransform.anchorMin = arribaDerecha;
        textoFrutas.rectTransform.anchorMax = arribaDerecha;
        textoFrutas.rectTransform.pivot = arribaDerecha;
        textoFrutas.rectTransform.anchoredPosition = new Vector2(-40f, -35f);
        textoFrutas.rectTransform.sizeDelta = new Vector2(220f, 70f);

        // Mensajes en el centro abajo y los controles en la parte de abajo
        TextMeshProUGUI aviso = ConstructorInterfaz.CrearTexto("TextoAviso", raiz, string.Empty, 48f, TextAlignmentOptions.Center, new Color(1f, 0.93f, 0.4f));
        aviso.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        aviso.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        aviso.rectTransform.pivot = new Vector2(0.5f, 0f);
        aviso.rectTransform.anchoredPosition = new Vector2(0f, 120f);
        aviso.rectTransform.sizeDelta = new Vector2(1400f, 80f);

        RectTransform fondoControles = ConstructorInterfaz.CrearRect("FondoControles", raiz, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1780f, 60f));
        Image imagenFondoControles = fondoControles.gameObject.AddComponent<Image>();
        imagenFondoControles.sprite = ConstructorInterfaz.SpriteCaja;
        imagenFondoControles.type = Image.Type.Sliced;
        imagenFondoControles.color = new Color(0.1f, 0.1f, 0.15f, 0.6f);
        imagenFondoControles.raycastTarget = false;

        TextMeshProUGUI controles = ConstructorInterfaz.CrearTexto("TextoControles", raiz,
            "Mover: flechas o A/D   |   Saltar: flecha arriba, W o Espacio   |   Dash: Shift o J   |   Pausa: Esc   |   Reiniciar: R",
            27f, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.9f));
        controles.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        controles.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        controles.rectTransform.pivot = new Vector2(0.5f, 0f);
        controles.rectTransform.anchoredPosition = new Vector2(0f, 30f);
        controles.rectTransform.sizeDelta = new Vector2(1740f, 50f);

        // Pantalla roja cuando recibe danio
        Image superposicion = ConstructorInterfaz.CrearImagen("SuperposicionDanio", raiz, new Color(1f, 0f, 0f, 0f));
        ConstructorInterfaz.Anclar(superposicion.rectTransform, Vector2.zero, Vector2.one);
        superposicion.raycastTarget = false;

        ControladorHUD hud = canvas.gameObject.AddComponent<ControladorHUD>();
        UtilConstructor.AsignarReferencia(hud, "barraVida", relleno);
        UtilConstructor.AsignarReferencia(hud, "textoVida", textoVida);
        UtilConstructor.AsignarReferencia(hud, "superposicionDanio", superposicion);
        UtilConstructor.AsignarReferencia(hud, "textoFrutas", textoFrutas);
        UtilConstructor.AsignarReferencia(hud, "textoAviso", aviso);

        ConstruirPausa(raiz);
        ConstruirPanelFin(raiz, PanelFinPartida.Modo.Victoria, "PanelVictoria", "¡NIVEL COMPLETADO!", new Color(0.4f, 1f, 0.5f));
        ConstruirPanelFin(raiz, PanelFinPartida.Modo.FinDelJuego, "PanelFinDelJuego", "GAME OVER", new Color(1f, 0.4f, 0.4f));
    }

    private static void ConstruirPausa(Transform raiz)
    {
        GameObject panel = ConstructorInterfaz.CrearPanel("PanelPausa", raiz, "PAUSA", out TextMeshProUGUI titulo, "Continuar", "Reiniciar", "Menú principal");
        Transform caja = panel.transform.Find("Caja");
        Button continuar = ConstructorInterfaz.CrearBoton("BotonContinuar", caja, "CONTINUAR", new Vector2(0f, 20f), new Vector2(520f, 100f));
        Button reiniciar = ConstructorInterfaz.CrearBoton("BotonReiniciar", caja, "REINICIAR", new Vector2(0f, -100f), new Vector2(520f, 100f));
        Button menu = ConstructorInterfaz.CrearBoton("BotonMenu", caja, "MENÚ PRINCIPAL", new Vector2(0f, -220f), new Vector2(520f, 100f));

        // El script va en un objeto aparte porque el panel se apaga y el script tiene que seguir escuchando
        GameObject contenedor = new GameObject("MenuPausa");
        contenedor.transform.SetParent(raiz, false);
        MenuPausa pausa = contenedor.AddComponent<MenuPausa>();
        UtilConstructor.AsignarReferencia(pausa, "panel", panel);
        UtilConstructor.AsignarReferencia(pausa, "botonContinuar", continuar);
        UtilConstructor.AsignarReferencia(pausa, "botonReiniciar", reiniciar);
        UtilConstructor.AsignarReferencia(pausa, "botonMenu", menu);

        // El panel se guarda apagado: solo se muestra cuando llega el evento
        panel.SetActive(false);
    }

    private static void ConstruirPanelFin(Transform raiz, PanelFinPartida.Modo modo, string nombre, string textoTitulo, Color colorTitulo)
    {
        GameObject panel = ConstructorInterfaz.CrearPanel(nombre, raiz, textoTitulo, out TextMeshProUGUI titulo, "Reiniciar", "Menú");
        titulo.color = colorTitulo;
        Transform caja = panel.transform.Find("Caja");
        Button reiniciar = ConstructorInterfaz.CrearBoton("BotonReiniciar", caja, "JUGAR DE NUEVO", new Vector2(0f, -10f), new Vector2(520f, 100f));
        Button menu = ConstructorInterfaz.CrearBoton("BotonMenu", caja, "MENÚ PRINCIPAL", new Vector2(0f, -130f), new Vector2(520f, 100f));

        GameObject contenedor = new GameObject("Pantalla" + modo);
        contenedor.transform.SetParent(raiz, false);
        PanelFinPartida script = contenedor.AddComponent<PanelFinPartida>();
        UtilConstructor.AsignarEntero(script, "modo", (int)modo);
        UtilConstructor.AsignarReferencia(script, "panel", panel);
        UtilConstructor.AsignarReferencia(script, "titulo", titulo);
        UtilConstructor.AsignarReferencia(script, "botonReiniciar", reiniciar);
        UtilConstructor.AsignarReferencia(script, "botonMenu", menu);

        // El panel se guarda apagado: solo se muestra cuando llega el evento
        panel.SetActive(false);
    }
}
#endif
