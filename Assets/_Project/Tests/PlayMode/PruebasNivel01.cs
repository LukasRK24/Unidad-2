using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

// Pruebas automaticas: cargan el nivel y simulan lo que haria un jugador
public class PruebasNivel01
{
    private MovimientoJugador movimiento;
    private VidaJugador vida;
    private Rigidbody2D cuerpo;

    [UnitySetUp]
    public IEnumerator CargarNivel()
    {
        SceneManager.LoadScene("Level_01");
        yield return null;
        yield return null;

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        movimiento = jugador.GetComponent<MovimientoJugador>();
        vida = jugador.GetComponent<VidaJugador>();
        cuerpo = jugador.GetComponent<Rigidbody2D>();

        // Se apaga el input real para controlar al jugador desde la prueba
        jugador.GetComponent<ControladorEntradaJugador>().enabled = false;
    }

    private void Teletransportar(Vector2 posicion)
    {
        cuerpo.position = posicion;
        cuerpo.linearVelocity = Vector2.zero;
        movimiento.transform.position = posicion;
        Physics2D.SyncTransforms();
    }

    private static T Buscar<T>(string nombre) where T : Component
    {
        foreach (T objeto in Object.FindObjectsByType<T>(FindObjectsInactive.Include))
        {
            if (objeto.name == nombre) return objeto;
        }
        return null;
    }

    [UnityTest]
    public IEnumerator Nivel_Tiene_Todos_Los_Sistemas()
    {
        Assert.IsNotNull(ControladorJuego.Instancia);
        Assert.IsNotNull(ControladorAudio.Instancia);
        Assert.IsNotNull(ControladorEfectos.Instancia);
        Assert.IsNotNull(Camera.main.GetComponent<SeguimientoCamara>());
        Assert.AreEqual(9, ControladorJuego.Instancia.FrutasTotal);
        Assert.AreEqual(3, Object.FindObjectsByType<EnemigoPatrulla>().Length);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Jugador_Aterriza_Y_Esta_En_Suelo()
    {
        yield return new WaitForSeconds(1f);
        Assert.IsTrue(movimiento.EnSuelo, "El jugador deberia estar sobre el suelo del Tilemap");
        Assert.That(cuerpo.position.y, Is.EqualTo(1.75f).Within(0.2f));
    }

    [UnityTest]
    public IEnumerator Jugador_Corre_Y_Salta_Y_Avisa_Por_Evento()
    {
        bool salto = false;
        EventosJuego.AlSaltarJugador += posicion => salto = true;
        yield return new WaitForSeconds(0.6f);

        float inicioX = cuerpo.position.x;
        movimiento.Mover(1f);
        yield return new WaitForSeconds(0.8f);
        Assert.Greater(cuerpo.position.x, inicioX + 3f, "Debe avanzar a la derecha");

        float alturaSuelo = cuerpo.position.y;
        movimiento.MantenerSalto(true);
        movimiento.PedirSalto();
        yield return new WaitForSeconds(0.25f);
        Assert.IsTrue(salto, "Debe publicarse AlSaltarJugador");
        Assert.Greater(cuerpo.position.y, alturaSuelo + 1f, "Debe elevarse al saltar");
    }

    [UnityTest]
    public IEnumerator Dash_Mueve_Rapido_Al_Jugador()
    {
        yield return new WaitForSeconds(0.6f);
        float inicioX = cuerpo.position.x;
        movimiento.PedirDash();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(movimiento.EnDash);
        Assert.Greater(cuerpo.position.x - inicioX, 1.2f);
    }

    [UnityTest]
    public IEnumerator Recoger_Fruta_Actualiza_El_HUD_Por_Eventos()
    {
        Coleccionable fruta = Object.FindFirstObjectByType<Coleccionable>();
        Teletransportar(fruta.transform.position);
        yield return new WaitForSeconds(0.3f);

        Assert.AreEqual(1, ControladorJuego.Instancia.FrutasRecogidas);
        Assert.AreEqual("1 / 9", Buscar<TMP_Text>("TextoFrutas").text);
    }

    [UnityTest]
    public IEnumerator Danio_Actualiza_La_Barra_Y_La_Invulnerabilidad_Evita_Doble_Golpe()
    {
        vida.RecibirDanio(1, Vector2.zero);
        vida.RecibirDanio(1, Vector2.zero);
        yield return null;

        Assert.AreEqual(2, vida.VidaActual, "La invulnerabilidad evita el doble danio");
        Assert.That(Buscar<Image>("RellenoVida").fillAmount, Is.EqualTo(2f / 3f).Within(0.01f));
        Assert.AreEqual("2/3", Buscar<TMP_Text>("TextoVida").text);
    }

    [UnityTest]
    public IEnumerator Los_Picos_Lastiman_Al_Jugador()
    {
        Peligro picos = null;
        foreach (Peligro peligro in Object.FindObjectsByType<Peligro>())
        {
            if (peligro.name == "Picos") { picos = peligro; break; }
        }

        Teletransportar(picos.transform.position);
        yield return new WaitForSeconds(0.3f);
        Assert.Less(vida.VidaActual, 3);
    }

    [UnityTest]
    public IEnumerator Al_Morir_Aparece_El_Panel_De_Game_Over()
    {
        Assert.IsNull(GameObject.Find("PanelFinDelJuego"));
        vida.Matar();
        yield return new WaitForSeconds(1.3f);
        Assert.IsNotNull(GameObject.Find("PanelFinDelJuego"), "Debe aparecer el panel de Game Over");
        Assert.IsTrue(vida.EstaMuerto);
    }

    [UnityTest]
    public IEnumerator Caer_Al_Vacio_Mata_Al_Jugador()
    {
        Teletransportar(new Vector2(22f, -3f));
        yield return new WaitForSeconds(1.5f);
        Assert.IsTrue(vida.EstaMuerto);
    }

    [UnityTest]
    public IEnumerator Pausa_Congela_El_Tiempo_Y_Muestra_El_Panel()
    {
        EventosJuego.PedirPausa();
        yield return null;
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsNotNull(GameObject.Find("PanelPausa"));

        EventosJuego.PedirPausa();
        yield return null;
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsNull(GameObject.Find("PanelPausa"));
    }

    [UnityTest]
    public IEnumerator Pisar_Un_Enemigo_Lo_Derrota_Y_No_Danha_Al_Jugador()
    {
        bool derrotado = false;
        EventosJuego.AlDerrotarEnemigo += posicion => derrotado = true;

        EnemigoPatrulla enemigo = Object.FindFirstObjectByType<EnemigoPatrulla>();
        yield return new WaitForSeconds(0.5f);
        Vector2 encima = (Vector2)enemigo.transform.position + new Vector2(0f, 2.6f);
        Teletransportar(encima);
        cuerpo.linearVelocity = new Vector2(0f, -6f);
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(derrotado, "Pisar al enemigo lo derrota");
        Assert.AreEqual(3, vida.VidaActual, "El pisoton no debe danar al jugador");
    }

    [UnityTest]
    public IEnumerator Tocar_Un_Enemigo_De_Costado_Lastima()
    {
        EnemigoPatrulla enemigo = Object.FindFirstObjectByType<EnemigoPatrulla>();
        yield return new WaitForSeconds(0.5f);
        Teletransportar((Vector2)enemigo.transform.position + new Vector2(-1.2f, 0f));
        yield return new WaitForSeconds(0.6f);
        Assert.Less(vida.VidaActual, 3);
    }

    [UnityTest]
    public IEnumerator Llegar_A_La_Meta_Sin_Frutas_Solo_Avisa()
    {
        Meta meta = Object.FindFirstObjectByType<Meta>();
        Teletransportar(meta.transform.position);
        yield return new WaitForSeconds(0.4f);
        Assert.IsFalse(ControladorJuego.Instancia.Terminado);
        StringAssert.Contains("frutas", Buscar<TMP_Text>("TextoAviso").text);
    }

    [UnityTest]
    public IEnumerator Con_Todas_Las_Frutas_La_Meta_Completa_El_Nivel()
    {
        bool completado = false;
        EventosJuego.AlCompletarNivel += () => completado = true;

        foreach (Coleccionable fruta in Object.FindObjectsByType<Coleccionable>())
        {
            Teletransportar(fruta.transform.position);
            yield return new WaitForSeconds(0.15f);
        }
        Assert.AreEqual(9, ControladorJuego.Instancia.FrutasRecogidas);

        Teletransportar(Object.FindFirstObjectByType<Meta>().transform.position);
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(completado, "Debe publicarse AlCompletarNivel");
        yield return new WaitForSeconds(1f);
        Assert.IsNotNull(GameObject.Find("PanelVictoria"));
    }

    [UnityTest]
    public IEnumerator La_Camara_Sigue_Al_Jugador_Dentro_De_Los_Limites()
    {
        yield return new WaitForSeconds(0.5f);
        Teletransportar(new Vector2(50f, 4f));
        yield return new WaitForSeconds(1.5f);
        Assert.That(Camera.main.transform.position.x, Is.EqualTo(50f).Within(4f));
        Assert.Greater(Camera.main.transform.position.x, 30f);
    }

    [UnityTest]
    public IEnumerator El_Animator_Pasa_Por_Idle_Run_Y_Jump()
    {
        Animator animador = movimiento.GetComponentInChildren<Animator>();
        yield return new WaitForSeconds(0.8f);
        Assert.IsTrue(animador.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        movimiento.Mover(1f);
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(animador.GetCurrentAnimatorStateInfo(0).IsName("Run"));

        movimiento.Mover(0f);
        movimiento.PedirSalto();
        yield return new WaitForSeconds(0.15f);
        Assert.IsTrue(animador.GetCurrentAnimatorStateInfo(0).IsName("Jump"));
    }
}

public class PruebasMenuPrincipal
{
    [UnityTest]
    public IEnumerator Menu_Tiene_Botones_Y_Parametros_Del_Mezclador()
    {
        SceneManager.LoadScene("MainMenu");
        yield return null;
        yield return null;

        Assert.IsNotNull(GameObject.Find("BotonJugar"));
        Assert.IsNotNull(GameObject.Find("BotonSalir"));
        Assert.IsNotNull(ControladorAudio.Instancia);

        Slider musica = GameObject.Find("DeslizadorMusica").GetComponent<Slider>();
        musica.value = 0.25f;
        yield return null;
        Assert.That(ControladorAudio.Instancia.VolumenMusica, Is.EqualTo(0.25f).Within(0.001f));

        UnityEngine.Audio.AudioMixer mezclador = Resources.FindObjectsOfTypeAll<UnityEngine.Audio.AudioMixer>()[0];
        Assert.IsTrue(mezclador.GetFloat(ControladorAudio.ParametroMusica, out float decibelios));
        Assert.That(decibelios, Is.EqualTo(-12.04f).Within(0.1f));
    }
}

public class PruebasCompletarNivel
{
    // Un bot corre a la derecha y salta si hay un hueco o una pared: asi se comprueba que el nivel se puede terminar
    [UnityTest]
    public IEnumerator Un_Bot_Puede_Recorrer_El_Nivel_Hasta_La_Meta()
    {
        SceneManager.LoadScene("Level_01");
        yield return null;
        yield return null;

        foreach (EnemigoPatrulla enemigo in Object.FindObjectsByType<EnemigoPatrulla>()) Object.Destroy(enemigo.gameObject);
        foreach (Peligro peligro in Object.FindObjectsByType<Peligro>())
        {
            if (peligro.name == "Picos") Object.Destroy(peligro.gameObject);
        }

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        jugador.GetComponent<ControladorEntradaJugador>().enabled = false;
        MovimientoJugador movimiento = jugador.GetComponent<MovimientoJugador>();
        Rigidbody2D cuerpo = jugador.GetComponent<Rigidbody2D>();
        int suelo = LayerMask.GetMask("Suelo");
        float metaX = Object.FindFirstObjectByType<Meta>().transform.position.x;

        float transcurrido = 0f;
        float saltoSostenido = 0f;
        while (transcurrido < 40f && cuerpo.position.x < metaX - 1f && !jugador.GetComponent<VidaJugador>().EstaMuerto)
        {
            Vector2 pos = cuerpo.position;
            movimiento.Mover(1f);
            saltoSostenido -= Time.deltaTime;
            movimiento.MantenerSalto(saltoSostenido > 0f);

            bool hayHueco = !Physics2D.Raycast(pos + new Vector2(1.6f, -0.6f), Vector2.down, 2f, suelo);
            bool hayPared = Physics2D.Raycast(pos + new Vector2(0f, -0.5f), Vector2.right, 1.1f, suelo);
            if (movimiento.EnSuelo && (hayHueco || hayPared))
            {
                movimiento.PedirSalto();
                saltoSostenido = 0.3f;
            }

            transcurrido += Time.deltaTime;
            yield return null;
        }

        Assert.Greater(cuerpo.position.x, metaX - 2f, "El bot quedo atascado en x=" + cuerpo.position.x.ToString("F1") + ", y=" + cuerpo.position.y.ToString("F1"));
    }
}
