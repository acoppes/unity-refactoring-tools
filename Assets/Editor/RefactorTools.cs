using System;
using System.Linq;

namespace Utils.Editor
{
    public static class RefactorTools
    {
        public static void RefactorAsset<T>(Func<T, bool> callback) where T : UnityEngine.Object
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T)}", null);
            var assets = guids.Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(g))).ToList();
        
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
                
                    UnityEditor.EditorUtility.SetDirty(asset);
                }
            
                UnityEditor.AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }
        
        public static void RefactorMonoBehaviour<T>(bool includeScenes, Func<T, bool> callback) where T : UnityEngine.Object
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T)}", null);
            var assets = guids.Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(g))).ToList();
            
            // First, iterate in prefabs, no variants
            
            // Then, iterate in prefab variants
            
            // Then iterate in all scenes (if include scenes is true)

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
                
                    UnityEditor.EditorUtility.SetDirty(asset);
                }
            
                UnityEditor.AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }
    }
}