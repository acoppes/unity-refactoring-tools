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
        public struct RefactorParameters
        {
            public List<GameObject> prefabs;
            public List<string> scenes;
        }
    
        public struct RefactorData
        {
            public bool isPrefab;
            public string scenePath;
            public bool inScene;
        }

        public struct RefactorResult
        {
            public bool completed;
        }
        
        public static void DestroyMonoBehaviour<T>(bool destroyObject) where T : Component
        {
            RefactorMonoBehaviour<T>(new RefactorParameters
            {
                prefabs = AssetDatabaseExt.FindPrefabs<T>(),
                scenes = AssetDatabaseExt.FindAllScenes()
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
                
                return new RefactorResult
                {
                    completed = true
                };
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
            Func<GameObject, RefactorData, RefactorResult> callback) where T : Component
        {
            var prefabs = parameters.prefabs;

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

                    if (result.completed)
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

            var scenes = parameters.scenes;
            
            if (scenes.Count == 0)
                return;

            var loadedScenesList = new List<string>();
            var loadedScenes = SceneManager.sceneCount;
            var activeScene = SceneManager.GetActiveScene().path;
            
            for (var i = 0; i < loadedScenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                loadedScenesList.Add(scene.path);
            }

            var scenesCount = scenes.Count;

            EditorUtility.DisplayProgressBar($"Refactoring {scenesCount} scenes", "Starting...", 0);

            for (var i = 0; i < scenesCount; i++)
            {
                var scenePath = scenes[i];
                
                try
                {
                    EditorUtility.DisplayProgressBar($"Refactoring {scenesCount} scenes", scenePath,
                        i / (float) scenesCount);
                    
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
                            isPrefab = false,
                            scenePath = scenePath,
                            inScene = true
                        });
                        
                        if (result.completed)
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