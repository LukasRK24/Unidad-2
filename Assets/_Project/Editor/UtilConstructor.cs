#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UtilConstructor
{
    public const string Raiz = "Assets/_Project";

    public static void AsegurarCarpeta(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }

    public static Sprite[] CargarSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => TrailingIndex(s.name))
            .ToArray();
    }

    public static Sprite CargarSprite(string path)
    {
        Sprite[] all = CargarSprites(path);
        return all.Length > 0 ? all[0] : null;
    }

    public static Sprite CargarSpritePorNombre(string path, string spriteName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s => s.name == spriteName);
    }

    private static int TrailingIndex(string name)
    {
        int underscore = name.LastIndexOf('_');
        if (underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int index)) return index;
        return 0;
    }

    public static int AsegurarCapa(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < 32; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return i;

        for (int i = 6; i < 32; i++)
        {
            SerializedProperty slot = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }
        return 0;
    }

    public static void AjustarPixelesPorUnidad(string path, float ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || Mathf.Approximately(importer.spritePixelsPerUnit, ppu)) return;
        importer.spritePixelsPerUnit = ppu;
        importer.SaveAndReimport();
    }

    public static void AsignarReferencia(Object target, string property, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(property);
        if (prop == null) { Debug.LogError($"Propiedad no encontrada: {target.GetType().Name}.{property}"); return; }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void AsignarDecimal(Object target, string property, float value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(property).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void AsignarVector2(Object target, string property, Vector2 value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(property).vector2Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void AsignarEntero(Object target, string property, int value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(property).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void AsignarSprites(Object target, string property, Sprite[] sprites)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty array = so.FindProperty(property);
        array.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static GameObject GuardarPrefab(GameObject instance, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }
}
#endif
