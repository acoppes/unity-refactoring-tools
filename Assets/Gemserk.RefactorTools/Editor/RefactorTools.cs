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
            public bool defaultDebugEnabled;
            public bool interruptOnFailure;
        }
    
        public struct RefactorData
        {
            public enum Source
            {
                Prefab, 
                Scene
            }
            
            public Source source;
            public string sourcePath;

            public bool isPrefab => source == Source.Prefab;
            public bool isScene => source == Source.Scene;
        }

        public struct RefactorResult
        {
            public bool completed;
        }

        public struct RefactorMonoBehaviourResult
        {
            public List<GameObject> failedPrefabs;
            public List<string> failedScenes;
        }
        
        public static void DestroyMonoBehaviour<T>(bool destroyObject)
        {
            DestroyMonoBehaviour<T>(destroyObject, new RefactorParameters
            {
                prefabs = AssetDatabaseExt.FindPrefabs<T>(),
                scenes = AssetDatabaseExt.FindAllScenes()
            });
        }
        
        public static void DestroyMonoBehaviour<T>(bool destroyObject, RefactorParameters refactorParameters)
        {
            RefactorMonoBehaviour<T>(refactorParameters, delegate(GameObject gameObject, RefactorData parameters)
            {
                var components = gameObject.GetComponentsInChildren<T>();
                
                var objectsToDestroy = new List<GameObject>();
                
                foreach (var t in components)
                {
                    var component = t as Component;
                    
                    // Shouldn't be null since we can only get Components from gameobject.
                    if (component == null)
                    {
                        continue;
                    }
                    
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
        
        private static void SortByRootPrefabFirst(List<GameObject> prefabs)
        {
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
                return string.Compare(a.name, b.name, StringComparison.Ordinal);
            });
        }


        public static bool ReplaceScript<T1, T2>(GameObject gameObject, AssetDatabaseExt.FindOptions findOptions = AssetDatabaseExt.FindOptions.None) 
            where T1 : Component 
            where T2: Component
        {
            var previousTypeList = gameObject.GetComponents<T1>();

            if (findOptions == AssetDatabaseExt.FindOptions.ConsiderChildren)
            {
                previousTypeList = gameObject.GetComponentsInChildren<T1>();
            } else if (findOptions == AssetDatabaseExt.FindOptions.ConsiderInactiveChildren)
            {
                previousTypeList = gameObject.GetComponentsInChildren<T1>(true);
            }

            var newScriptType = typeof(T2);
            
            var monoScripts = AssetDatabaseExt.FindAssets<MonoScript>(newScriptType.Name).ToList();

            if (monoScripts.Count == 0)
            {
                Debug.LogWarning($"No MonoScripts for type {newScriptType.Name} found.", gameObject);
                return false;
            }
            
            if (previousTypeList.Length == 0)
            {
                Debug.LogWarning($"No MonoScripts for type {typeof(T1).Name} found.", gameObject);
                return false;
            }
            
            var newClassMonoScript = monoScripts.First(m => m.name.Equals(newScriptType.Name, StringComparison.OrdinalIgnoreCase));
            
            foreach (var component in previousTypeList)
            {
                var componentSerializedObject = new SerializedObject(component);
                var scriptProperty = componentSerializedObject.FindProperty("m_Script");
                scriptProperty.objectReferenceValue = newClassMonoScript;
                componentSerializedObject.ApplyModifiedProperties();
            }

            return true;
        }

        public static RefactorMonoBehaviourResult RefactorMonoBehaviour<T>(RefactorParameters parameters, 
            Func<GameObject, RefactorData, RefactorResult> callback)
        {
            var scenes = parameters.scenes;
            
            if (scenes != null && scenes.Count > 0)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return new RefactorMonoBehaviourResult()
                    {
                        
                    };
                }
            }
            
            var generalResult = new RefactorMonoBehaviourResult()
            {
                failedPrefabs = new List<GameObject>(),
                failedScenes = new List<string>()
            };
            
            if (parameters.prefabs != null && parameters.prefabs.Count > 0)
            {
                var prefabs = new List<GameObject>(parameters.prefabs);

                SortByRootPrefabFirst(prefabs);

                try
                {
                    var total = prefabs.Count;

                    EditorUtility.DisplayProgressBar($"Refactoring {total} prefabs with {typeof(T).Name}", "Start", 0);

                    if (parameters.defaultDebugEnabled)
                    {
                        Debug.Log($"Refactoring {total} prefabs with {typeof(T).Name}");
                    }

                    for (var i = 0; i < prefabs.Count; i++)
                    {
                        var prefab = prefabs[i];
                        EditorUtility.DisplayProgressBar($"Refactoring {prefabs.Count} assets of type {typeof(T).Name}",
                            prefab.name,
                            i / (float)total);

                        var assetPath = AssetDatabase.GetAssetPath(prefab);

                        if (parameters.defaultDebugEnabled)
                        {
                            Debug.Log($"Opening {assetPath} for refactor");
                        }

                        var contents = PrefabUtility.LoadPrefabContents(assetPath);

                        try
                        {
                            var result = callback(contents, new RefactorData
                            {
                                source = RefactorData.Source.Prefab,
                                sourcePath = assetPath
                            });
                            
                            if (result.completed)
                            {
                                PrefabUtility.SaveAsPrefabAsset(contents, assetPath, out var success);
                                if (!success)
                                {
                                    generalResult.failedPrefabs.Add(prefab);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            generalResult.failedPrefabs.Add(prefab);
                            
                            if (parameters.defaultDebugEnabled)
                            {
                                Debug.Log($"Failed to refactor prefab {assetPath}");
                                Debug.LogException(e);
                            }

                            if (parameters.interruptOnFailure)
                            {
                                EditorUtility.ClearProgressBar();
                                return generalResult;
                            }
                        }
                        
                        PrefabUtility.UnloadPrefabContents(contents);
                    }
                }
                catch (Exception e)
                {
                    if (parameters.defaultDebugEnabled)
                    {
                        Debug.Log($"Failed to refactor prefabs");
                        Debug.LogException(e);
                    }
                    
                    if (parameters.interruptOnFailure)
                    {
                        return generalResult;
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            
            // Then iterate in all scenes (if include scenes is true)
            
            if (scenes != null && scenes.Count > 0)
            {
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

                if (parameters.defaultDebugEnabled)
                {
                    Debug.Log($"Refactoring {scenesCount} scenes");
                }

                for (var i = 0; i < scenesCount; i++)
                {
                    var scenePath = scenes[i];
                    var sceneErrors = 0;

                    try
                    {
                        EditorUtility.DisplayProgressBar($"Refactoring {scenesCount} scenes", scenePath,
                            i / (float)scenesCount);

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

                        foreach (var t in componentsList)
                        {
                            var component = t as Component;

                            // Shouldn't be null since we can only get Components from gameobject.
                            if (component == null)
                            {
                                continue;
                            }

                            var gameObject = component.gameObject;

                            try
                            {
                                var result = callback(gameObject, new RefactorData
                                {
                                    source = RefactorData.Source.Scene,
                                    sourcePath = scenePath
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
                            catch (Exception e)
                            {
                                sceneErrors++;
                                
                                if (parameters.defaultDebugEnabled)
                                {
                                    Debug.Log($"Failed to refactor {gameObject.name} in {scenePath}", gameObject);
                                    Debug.LogException(e);
                                }
                                
                                if (parameters.interruptOnFailure)
                                {
                                    EditorUtility.ClearProgressBar();
                                    return generalResult;
                                }
                            }
                        }

                        if (modified)
                        {
                            if (parameters.defaultDebugEnabled)
                            {
                                if (sceneErrors > 0)
                                {
                                    Debug.Log($"Completed refactor for scene: {scenePath} with {sceneErrors} failed refactors.");
                                }
                                else
                                {
                                    Debug.Log($"Completed refactor for scene: {scenePath}");
                                }
                            }

                            EditorSceneManager.MarkSceneDirty(scene);
                            if (!EditorSceneManager.SaveScene(scene))
                            {
                                generalResult.failedScenes.Add(scene.path);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        generalResult.failedScenes.Add(scenePath);
                        
                        if (parameters.defaultDebugEnabled)
                        {
                            Debug.Log($"Failed to refactor scene: {scenePath}");
                            Debug.LogException(e);
                        }
                        
                        if (parameters.interruptOnFailure)
                        {
                            return generalResult;
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
            
            return generalResult;
        }
    }
}