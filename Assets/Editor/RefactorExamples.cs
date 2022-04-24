using System;
using System.Collections.Generic;
using System.Linq;

public static class RefactorExamples
{
    public static List<T> FindAssets<T>(string[] folders = null) where T : UnityEngine.Object
    {
        var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T)}", folders);
        return guids.Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(
            UnityEditor.AssetDatabase.GUIDToAssetPath(g))).ToList();
    }

    public static void RefactorAsset<T>(Func<T, bool> callback) where T : UnityEngine.Object
    {
        var assets = FindAssets<T>();
        try
        {
            var total = assets.Count;

            UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {total} assets of type {typeof(T).Name}", "Start", 0);

            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {assets.Count} assets of type {typeof(T).Name}",
                    asset.name,
                    i / (float)total);

                var result = callback(asset);
                // Just to break the loop if something is wrong...
                if (!result)
                {
                    break;
                }
            }
        }
        finally
        {
            UnityEditor.EditorUtility.ClearProgressBar();
        }
    }
    
    [UnityEditor.MenuItem("Refactors/Refactor Custom Data")]
    public static void RefactorCustomData()
    {
        RefactorAsset(delegate(CustomDataAsset asset)
        {
            asset.newValue = $"VALUE:{asset.previousValue}";
            return true;
        });
    }
}
