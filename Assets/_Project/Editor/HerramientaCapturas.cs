#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Utilidad de verificación: renderiza capturas del nivel y del menú sin entrar a Play Mode.
public static class HerramientaCapturas
{
    public static void CapturarTodo()
    {
        string dir = "Screenshots";
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-shotDir") dir = args[i + 1];
        Directory.CreateDirectory(dir);

        EditorSceneManager.OpenScene(UtilConstructor.Raiz + "/Scenes/Level_01.unity");
        Camera cam = Camera.main;
        Canvas hud = GameObject.Find("CanvasHUD").GetComponent<Canvas>();
        hud.renderMode = RenderMode.ScreenSpaceCamera;
        hud.worldCamera = cam;
        hud.planeDistance = 5f;
        foreach (string panel in new[] { "PanelPausa", "PanelFinDelJuego", "PanelVictoria" })
            hud.transform.Find(panel)?.gameObject.SetActive(false);

        Vector2[] points = { new Vector2(8f, 3f), new Vector2(34f, 3f), new Vector2(72f, 7f), new Vector2(90f, 11f) };
        for (int i = 0; i < points.Length; i++)
        {
            cam.transform.position = new Vector3(points[i].x, points[i].y, -10f);
            Fotografiar(cam, Path.Combine(dir, $"level_{i}.png"));
        }

        foreach (string panel in new[] { "PanelPausa", "PanelFinDelJuego", "PanelVictoria" })
        {
            Transform t = hud.transform.Find(panel);
            if (t == null) continue;
            t.gameObject.SetActive(true);
            cam.transform.position = new Vector3(8f, 3f, -10f);
            Fotografiar(cam, Path.Combine(dir, panel + ".png"));
            t.gameObject.SetActive(false);
        }

        EditorSceneManager.OpenScene(UtilConstructor.Raiz + "/Scenes/MainMenu.unity");
        Camera menuCam = Camera.main;
        Canvas menu = GameObject.Find("CanvasMenu").GetComponent<Canvas>();
        menu.renderMode = RenderMode.ScreenSpaceCamera;
        menu.worldCamera = menuCam;
        menu.planeDistance = 5f;
        Fotografiar(menuCam, Path.Combine(dir, "menu.png"));

        EditorApplication.Exit(0);
    }

    private static void Fotografiar(Camera cam, string path)
    {
        Canvas.ForceUpdateCanvases();
        RenderTexture rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);
    }
}
#endif
