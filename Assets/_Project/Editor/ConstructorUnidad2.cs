#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;

// Orquestador: genera prefabs, animaciones, mezclador, partículas y las escenas MainMenu y Level_01.
public static class ConstructorUnidad2
{
    private const string TmpPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

    [MenuItem("Unidad 2/0) Importar TMP Essentials")]
    public static void ImportarTmpEssentials()
    {
        if (Directory.Exists("Assets/TextMesh Pro")) return;
        AssetDatabase.ImportPackage(TmpPackage, false);
    }

    [MenuItem("Unidad 2/1) Generar proyecto completo")]
    public static void ConstruirTodo()
    {
        if (!Directory.Exists("Assets/TextMesh Pro"))
        {
            Debug.LogError("Falta TMP Essentials. Ejecuta primero: Unidad 2 > 0) Importar TMP Essentials");
            return;
        }

        ConfigurarProyecto();

        int ground = UtilConstructor.AsegurarCapa("Suelo");
        int enemy = UtilConstructor.AsegurarCapa("Enemigo");
        int player = UtilConstructor.AsegurarCapa("Jugador");
        AssetDatabase.Refresh();

        foreach (string folder in new[] { "Scenes", "Prefabs", "Prefabs/VFX", "Animations" })
            UtilConstructor.AsegurarCarpeta(UtilConstructor.Raiz + "/" + folder);

        AnimatorController controller = ConstructorAnimaciones.Build();
        AudioMixer mixer = ConstructorAudio.Build();
        ParticleSystem[] vfx = ConstructorParticulas.Build();

        ConstructorEscenas.ConstruirMenuPrincipal(mixer);
        ConstructorEscenas.ConstruirNivel(controller, mixer, vfx, ground, enemy, player);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(UtilConstructor.Raiz + "/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene(UtilConstructor.Raiz + "/Scenes/Level_01.unity", true)
        };

        AssetDatabase.SaveAssets();
        Debug.Log("Unidad 2: proyecto generado correctamente.");
    }

    private static void ConfigurarProyecto()
    {
        PlayerSettings.productName = "Unidad 2";
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EditorSettings.externalVersionControl = "Visible Meta Files";
    }

    public static void ImportarTmpPorLinea()
    {
        if (Directory.Exists("Assets/TextMesh Pro"))
        {
            EditorApplication.Exit(0);
            return;
        }
        AssetDatabase.importPackageCompleted += name => EditorApplication.Exit(0);
        AssetDatabase.importPackageFailed += (name, error) => { Debug.LogError(error); EditorApplication.Exit(1); };
        AssetDatabase.ImportPackage(TmpPackage, false);
    }

    public static void ConstruirTodoPorLinea()
    {
        ConstruirTodo();
        EditorApplication.Exit(0);
    }

    public static void CompilarWindowsPorLinea()
    {
        string carpeta = "Builds/Hito2_Final_Windows";
        Directory.CreateDirectory(carpeta);
        BuildPipeline.BuildPlayer(
            new[] { UtilConstructor.Raiz + "/Scenes/MainMenu.unity", UtilConstructor.Raiz + "/Scenes/Level_01.unity" },
            carpeta + "/Unidad2.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        EditorApplication.Exit(0);
    }

    public static void CompilarLinuxPorLinea()
    {
        string folder = "Builds/Hito2_Final";
        Directory.CreateDirectory(folder);
        BuildPipeline.BuildPlayer(
            new[] { UtilConstructor.Raiz + "/Scenes/MainMenu.unity", UtilConstructor.Raiz + "/Scenes/Level_01.unity" },
            folder + "/Unidad2.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.None);
        EditorApplication.Exit(0);
    }
}
#endif
