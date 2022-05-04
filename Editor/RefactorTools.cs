using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Gemserk.RefactorTools.Editor
{
    public static class RefactorTools
    {
        public static void DestroyMonoBehaviour<T>() where T : Component
        {
            RefactorMonoBehaviour<T>(true, delegate(GameObject gameObject)
            {
                var components = gameObject.GetComponentsInChildren<T>();
                foreach (var component in components)
                {
                    Object.DestroyImmediate(component);
                }
                return true;
            });
        }
        
        public static void RefactorAsset<T>(Func<T, bool> callback) where T : Object
        {
            var assets = AssetDatabaseExt.FindAssets<T>();

            try
            {
                var total = assets.Count;

                EditorUtility.DisplayProgressBar($"Refactoring {total} assets of type {typeof(T).Name}", "Start", 0);

                for (var i = 0; i < assets.Count; i++)
                {
                    var asset = assets[i];
                    EditorUtility.DisplayProgressBar($"Refactoring {assets.Count} assets of type {typeof(T).Name}",
                        asset.name,
                        i / (float)total);

                    var result = callback(asset);
                    
                    if (result)
                    {
                        EditorUtility.SetDirty(asset);
                    }
                }
            
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        public static void RefactorMonoBehaviour<T>(bool includeScenes, 
            Func<GameObject, bool> callback) where T : Component
        {
            var prefabs = AssetDatabaseExt.FindPrefabs<T>();
            
            // Ignore prefabs without component T
            prefabs = prefabs.Where(p => p.GetComponentInChildren<T>(true) != null).ToList();

            // We sort by no variant prefabs first
            prefabs.Sort(delegate(GameObject a, GameObject b)
            {
                var aIsVariant = PrefabUtility.IsPartOfVariantPrefab(a);
                var bIsVariant = PrefabUtility.IsPartOfVariantPrefab(b);

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

                EditorUtility.DisplayProgressBar($"Refactoring {total} prefabs with {typeof(T).Name}", "Start", 0);

                for (var i = 0; i < prefabs.Count; i++)
                {
                    var prefab = prefabs[i];
                    EditorUtility.DisplayProgressBar($"Refactoring {prefabs.Count} assets of type {typeof(T).Name}",
                        prefab.name,
                        i / (float)total);
                    
                    var contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));

                    var result = callback(contents);

                    if (result)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, AssetDatabase.GetAssetPath(prefab));
                    }
                    
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            
            // Then iterate in all scenes (if include scenes is true)

            if (!includeScenes)
                return;

            var loadedScenesList = new List<string>();
            var loadedScenes = SceneManager.sceneCount;
            var activeScene = SceneManager.GetActiveScene().path;
            
            for (var i = 0; i < loadedScenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                loadedScenesList.Add(scene.path);
            }
            
            var allScenesGuids = new List<string>();

            // Here we filter by all assets of type scene but under Assets folder to avoid all other scenes from 
            // external packages.
            allScenesGuids.AddRange(AssetDatabase.FindAssets("t:scene", new []
            {
                "Assets"
            }));

            EditorUtility.DisplayProgressBar($"Refactoring {allScenesGuids.Count} scenes", "Starting...", 0);

            var allScenesCount = allScenesGuids.Count;
            for (var i = 0; i < allScenesCount; i++)
            {
                var sceneGuid = allScenesGuids[i];
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);

                try
                {
                    EditorUtility.DisplayProgressBar($"Refactoring {allScenesGuids.Count} scenes", scenePath,
                        i / (float) allScenesCount);
                    
                    var scene = EditorSceneManager.OpenScene(scenePath, 
                        OpenSceneMode.Single);

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
                        var gameObject = component.gameObject;
                        
                        var result = callback(gameObject);
                        if (result)
                        {
                            modified = true;
                            if (component != null)
                            {
                                EditorUtility.SetDirty(component);
                            }
                            else if (gameObject != null)
                            {
                                EditorUtility.SetDirty(gameObject);
                            }
                        }
                    }

                    if (modified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                    
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            var newActiveScene = EditorSceneManager.OpenScene(activeScene, 
                OpenSceneMode.Single);
            for (var i = 0; i < loadedScenes; i++)
            {
                if (loadedScenesList[i].Equals(activeScene))
                    continue;
                EditorSceneManager.OpenScene(loadedScenesList[i], 
                    OpenSceneMode.Additive);
            }
            SceneManager.SetActiveScene(newActiveScene);
        }
    }
}