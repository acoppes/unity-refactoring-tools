using System;
using System.Linq;
using UnityEngine;

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
        
        public static void RefactorMonoBehaviour<T>(bool includeScenes, Func<GameObject, bool> callback) where T : UnityEngine.Component
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:prefab", null);
            var prefabs = guids.Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(g))).ToList();
            
            // Ignore prefabs without component T
            prefabs = prefabs.Where(p => p.GetComponentInChildren<T>(true) != null).ToList();

            // We sort by no variant prefabs first
            prefabs.Sort(delegate(GameObject a, GameObject b)
            {
                var aIsVariant = UnityEditor.PrefabUtility.IsPartOfVariantPrefab(a);
                var bIsVariant = UnityEditor.PrefabUtility.IsPartOfVariantPrefab(b);

                if (!aIsVariant && bIsVariant)
                    return -1;

                if (aIsVariant && !bIsVariant)
                    return 1;

                // if both no variants or both variants, we just use the name to compare just to be consistent.
                return a.name.CompareTo(b.name);
            });
            
            prefabs.ForEach(delegate(GameObject o)
            {
                Debug.Log(o.name);
            });
            
            try
            {
                var total = prefabs.Count;

                UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {total} prefabs with {typeof(T).Name}", "Start", 0);

                for (var i = 0; i < prefabs.Count; i++)
                {
                    var prefab = prefabs[i];
                    UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {prefabs.Count} assets of type {typeof(T).Name}",
                        prefab.name,
                        i / (float)total);
                    
                    var contents = UnityEditor.PrefabUtility.LoadPrefabContents(UnityEditor.AssetDatabase.GetAssetPath(prefab));

                    var result = callback(contents);

                    // Just to break the loop if something is wrong...
                    if (!result)
                    {
                        UnityEditor.PrefabUtility.UnloadPrefabContents(contents);
                        break;
                    }
                    
                    UnityEditor.PrefabUtility.SaveAsPrefabAsset(contents, UnityEditor.AssetDatabase.GetAssetPath(prefab));
                    UnityEditor.PrefabUtility.UnloadPrefabContents(contents);
                    
                    // UnityEditor.EditorUtility.SetDirty(prefab);
                }
            
                // UnityEditor.AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
            
            // Then iterate in all scenes (if include scenes is true)

        }
    }
}