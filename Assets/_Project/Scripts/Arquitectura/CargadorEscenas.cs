using UnityEngine;
using UnityEngine.SceneManagement;

// Cambia de escena. Siempre restaura el tiempo porque la pausa lo deja en 0
public static class CargadorEscenas
{
    public const string MenuPrincipal = "MainMenu";
    public const string Nivel01 = "Level_01";

    public static void Cargar(string nombreEscena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    public static void RecargarActual()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
