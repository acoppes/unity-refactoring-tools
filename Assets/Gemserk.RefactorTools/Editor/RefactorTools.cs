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
        public class RefactorParameters
        {
            public bool considerScenes;
        }
    
        public class RefactorData
        {
            public bool isPrefab;
        }
        
        public static void DestroyMonoBehaviour<T>(bool destroyObject) where T : Component
        {
            RefactorMonoBehaviour<T>(new RefactorParameters
            {
                considerScenes = true
            }, delegate(GameObject gameObject, RefactorData parameters)
            {
                var components = gameObject.GetComponentsInChildren<T>();
                
                var objectsToDestroy = new List<GameObject>();
                
                foreach (var component in components)
                {
                    var componentGameObject = component.gameObject;
                    Object.DestroyImmediate(component);

                    if (!destroyObject) 
                        continue;

                    if (componentGameObject.transform.childCount != 0)
                        continue;

                    var otherComponents  = componentGameObject.GetComponents<Component>();
                    
                    // Transform is always there
                    if (otherComponents.Length > 1)
                        continue;
                    
                    if (!objectsToDestroy.Contains(componentGameObject))
                    {
                        objectsToDestroy.Add(componentGameObject);
                    }
                }

                foreach (var objectToDestroy in objectsToDestroy)
                {
                    // Avoid destroying the main GameObject if in prefab mode.
                    if (parameters.isPrefab && gameObject == objectToDestroy)
                        continue;
                    
                    Object.DestroyImmediate(objectToDestroy);
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
        
        public static void RefactorMonoBehaviour<T>(RefactorParameters parameters, 
            Func<GameObject, RefactorData, bool> callback) where T : Component
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

                    var result = callback(contents, new RefactorData
                    {
                        isPrefab = true
                    });

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

            if (!parameters.considerScenes)
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
                        
                        var result = callback(gameObject, new RefactorData
                        {
                            isPrefab = false
                        });
                        
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