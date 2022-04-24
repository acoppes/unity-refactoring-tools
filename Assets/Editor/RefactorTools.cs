using System;
using System.Collections.Generic;
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
                }
            }
            finally
            {
                UnityEditor.EditorUtility.ClearProgressBar();
            }
            
            // Then iterate in all scenes (if include scenes is true)

            if (!includeScenes)
                return;
            
             var allScenesGuids = new List<string>();

            // Here we filter by all assets of type scene but under Assets folder to avoid all other scenes from 
            // external packages.
            allScenesGuids.AddRange(UnityEditor.AssetDatabase.FindAssets("t:scene", new []
            {
                "Assets"
            }));

            UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {allScenesGuids.Count} scenes", "Starting...", 0);

            var allScenesCount = allScenesGuids.Count;
            for (var i = 0; i < allScenesCount; i++)
            {
                var sceneGuid = allScenesGuids[i];
                var scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(sceneGuid);

                try
                {
                    UnityEditor.EditorUtility.DisplayProgressBar($"Refactoring {allScenesGuids.Count} scenes", scenePath,
                        i / (float) allScenesCount);
                    
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, 
                        UnityEditor.SceneManagement.OpenSceneMode.Single);

                    var componentsList = new List<T>();

                    // We can iterate over root objects and collect stuff to run the refactor over
                    var rootObjects = scene.GetRootGameObjects();
                    for (var j = 0; j < rootObjects.Length; j++)
                    {
                        var go = rootObjects[j];
                        var components = go.GetComponentsInChildren<T>(true);
                        componentsList.AddRange(components.ToList());
                    }

                    var modified = false;

                    foreach (var component in componentsList)
                    {
                        var result = callback(component.gameObject);
                        if (result)
                        {
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    }
                    
                }
                finally
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}