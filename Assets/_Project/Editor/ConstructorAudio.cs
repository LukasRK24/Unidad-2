#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// Crea el AudioMixer (Master > Musica, Efectos) con los parametros expuestos VolumenMusica y VolumenEfectos.
public static class ConstructorAudio
{
    public const string MixerPath = UtilConstructor.Raiz + "/Audio/MainMixer.mixer";

    public static AudioMixer Build()
    {
        AssetDatabase.DeleteAsset(MixerPath);

        Assembly editorAsm = typeof(Editor).Assembly;
        Type controllerType = editorAsm.GetType("UnityEditor.Audio.AudioMixerController");
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        object controller = controllerType.GetMethod("CreateMixerControllerAtPath", all).Invoke(null, new object[] { MixerPath });
        object master = controllerType.GetProperty("masterGroup", all).GetValue(controller);

        object music = CrearGrupo(controllerType, controller, master, "Musica");
        object sfx = CrearGrupo(controllerType, controller, master, "Efectos");

        Type paramType = editorAsm.GetType("UnityEditor.Audio.ExposedAudioParameter");
        Array exposed = Array.CreateInstance(paramType, 2);
        exposed.SetValue(CrearParametro(paramType, music, ControladorAudio.ParametroMusica), 0);
        exposed.SetValue(CrearParametro(paramType, sfx, ControladorAudio.ParametroEfectos), 1);
        controllerType.GetProperty("exposedParameters", all).SetValue(controller, exposed);

        EditorUtility.SetDirty((UnityEngine.Object)controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(MixerPath);
        return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
    }

    private static object CrearGrupo(Type controllerType, object controller, object parent, string name)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object group = controllerType.GetMethod("CreateNewGroup", all).Invoke(controller, new object[] { name, false });
        controllerType.GetMethod("AddChildToParent", all).Invoke(controller, new[] { group, parent });
        return group;
    }

    private static object CrearParametro(Type paramType, object group, string name)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object guid = group.GetType().GetMethod("GetGUIDForVolume", all).Invoke(group, null);
        object parameter = Activator.CreateInstance(paramType);
        paramType.GetField("guid", all).SetValue(parameter, guid);
        paramType.GetField("name", all).SetValue(parameter, name);
        return parameter;
    }
}
#endif
