using Gemserk.RefactorTools.Editor;
using SuperNamespaace;
using UnityEditor;
using UnityEngine;

public static class AssetDatabaseExamples
{
    [MenuItem("Examples/AssetDatabase/Find Assets Of Type (generics)")]
    public static void FindAssetsOfTypeGenerics()
    {
        var assets = AssetDatabaseExt.FindAssets<AssetType1>();
        foreach (var asset in assets)
        {
            Debug.Log(asset.name);
        }
    }
    
    [MenuItem("Examples/AssetDatabase/Find Assets Of Type")]
    public static void FindAssetsOfType()
    {
        var assets = AssetDatabaseExt.FindAssets(typeof(AssetType2));
        foreach (var asset in assets)
        {
            Debug.Log(asset.name);
        }
    }
}