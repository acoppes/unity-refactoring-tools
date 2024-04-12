using Gemserk.RefactorTools.Editor;
using RefactorExamplesData;
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
    
    [MenuItem("Examples/AssetDatabase/Find Assets Of Type Matching Name")]
    public static void FindAssetsOfTypeMatchingName()
    {
        var assets = AssetDatabaseExt.FindAssets(typeof(AssetType2), "Super");
        foreach (var asset in assets)
        {
            Debug.Log(asset.name);
        }
    }
    
    [MenuItem("Examples/AssetDatabase/Find Assets Of interface Type")]
    public static void FindAssetsOfInterfaceType()
    {
        var assets = AssetDatabaseExt.FindAssetsAll(typeof(IAsset));
        foreach (var asset in assets)
        {
            Debug.Log(asset.name);
        }
    }
    
    [MenuItem("Examples/AssetDatabase/Find Prefabs with text")]
    public static void FindPrefabsWithText()
    {
        var assets = AssetDatabaseExt.FindPrefabs<ComponentA>(AssetDatabaseExt.FindOptions.ConsiderInactiveChildren, "Definition");
        foreach (var asset in assets)
        {
            Debug.Log(asset.name);
        }
    }
}