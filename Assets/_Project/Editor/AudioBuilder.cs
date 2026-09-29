#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// Crea el AudioMixer (Master > Music, SFX) con parámetros expuestos MusicVolume y SfxVolume.
public static class AudioBuilder
{
    public const string MixerPath = BuilderUtil.Root + "/Audio/MainMixer.mixer";

    public static AudioMixer Build()
    {
        AssetDatabase.DeleteAsset(MixerPath);

        Assembly editorAsm = typeof(Editor).Assembly;
        Type controllerType = editorAsm.GetType("UnityEditor.Audio.AudioMixerController");
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        object controller = controllerType.GetMethod("CreateMixerControllerAtPath", all).Invoke(null, new object[] { MixerPath });
        object master = controllerType.GetProperty("masterGroup", all).GetValue(controller);

        object music = CreateGroup(controllerType, controller, master, "Music");
        object sfx = CreateGroup(controllerType, controller, master, "SFX");

        Type paramType = editorAsm.GetType("UnityEditor.Audio.ExposedAudioParameter");
        Array exposed = Array.CreateInstance(paramType, 2);
        exposed.SetValue(MakeParameter(paramType, music, AudioManager.MusicParameter), 0);
        exposed.SetValue(MakeParameter(paramType, sfx, AudioManager.SfxParameter), 1);
        controllerType.GetProperty("exposedParameters", all).SetValue(controller, exposed);

        EditorUtility.SetDirty((UnityEngine.Object)controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(MixerPath);
        return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
    }

    private static object CreateGroup(Type controllerType, object controller, object parent, string name)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        object group = controllerType.GetMethod("CreateNewGroup", all).Invoke(controller, new object[] { name, false });
        controllerType.GetMethod("AddChildToParent", all).Invoke(controller, new[] { group, parent });
        return group;
    }

    private static object MakeParameter(Type paramType, object group, string name)
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
