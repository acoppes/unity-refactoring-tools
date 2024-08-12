using System;
using Gemserk.RefactorTools.Editor;
using RefactorExamplesData;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RefactorExamples
{
    [MenuItem("Refactors/Refactor Custom Data")]
    public static void Refactor1()
    {
        RefactorTools.RefactorAsset(delegate(CustomDataAsset asset)
        {
            asset.newValue = $"VALUE:{asset.previousValue}";
            return true;
        });
    }
    
    [MenuItem("Refactors/Refactor Custom MonoBehaviour")]
    public static void Refactor2()
    {
        RefactorTools.RefactorMonoBehaviour<CustomBehaviour>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<CustomBehaviour>(),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            var behaviours = gameObject.GetComponentsInChildren<CustomBehaviour>();
            foreach (var behaviour in behaviours)
            {
                behaviour.speed = new Speed
                {
                    baseValue = behaviour.speedBaseValue,
                    incrementValue = behaviour.speedIncrementValue
                };
            }
            return new RefactorTools.RefactorResult
            {
                completed = true
            };
        });
    }
    
    [MenuItem("Refactors/Refactor ComponentA to Parent")]
    public static void Refactor3()
    {
        RefactorTools.RefactorMonoBehaviour<ComponentA>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ComponentA>(),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            if (gameObject.transform.parent == null)
            {
                return new RefactorTools.RefactorResult
                {
                    completed = false
                };
            }
            
            var parentGameObject = gameObject.transform.parent.gameObject;
            
            var parentComponentA = parentGameObject.GetComponent<ComponentA>();

            if (parentComponentA == null)
            {
                parentComponentA = parentGameObject.AddComponent<ComponentA>();
            }
             
            var componentA = gameObject.GetComponent<ComponentA>();
            var json = JsonUtility.ToJson(componentA);
            JsonUtility.FromJsonOverwrite(json, parentComponentA);
            
            Object.DestroyImmediate(componentA);
            
            return new RefactorTools.RefactorResult
            {
                completed = true
            };
        });
    }
    
    [MenuItem("Refactors/Refactor ComponentB to Child")]
    public static void Refactor4()
    {
        RefactorTools.RefactorMonoBehaviour<ComponentB>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ComponentB>(),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            // will ignore this case
            if ("Child_WithComponentB".Equals(gameObject.name))
            {
                return new RefactorTools.RefactorResult
                {
                    completed = false
                };
            }
            
            GameObject childObject;

            if (gameObject.transform.childCount == 0)
            {
                childObject = new GameObject("Child_WithComponentB");
                childObject.transform.SetParent(gameObject.transform);
            }
            else
            {
                childObject = gameObject.transform.GetChild(0).gameObject;
            }

            var childComponentB = childObject.GetComponent<ComponentB>();

            if (childComponentB == null)
            {
                childComponentB = childObject.AddComponent<ComponentB>();
            }
             
            var componentB = gameObject.GetComponent<ComponentB>();
            var json = JsonUtility.ToJson(componentB);
            JsonUtility.FromJsonOverwrite(json, childComponentB);
            
            Object.DestroyImmediate(componentB);
            
            return new RefactorTools.RefactorResult
            {
                completed = true
            };
        });
    }
    
    [MenuItem("Refactors/Refactor ComponentB to Child With Reference")]
    public static void Refactor5()
    {
        RefactorTools.RefactorMonoBehaviour<ComponentB>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ComponentB>(),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            if (gameObject.GetComponent<ComponentC>() == null)
            {
                return new RefactorTools.RefactorResult
                {
                    completed = false
                };
            }
            
            // will ignore this case
            if ("Child_WithComponentB".Equals(gameObject.name))
            {
                return new RefactorTools.RefactorResult
                {
                    completed = false
                };
            }
            
            GameObject childObject;

            if (gameObject.transform.childCount == 0)
            {
                childObject = new GameObject("Child_WithComponentB");
                childObject.transform.SetParent(gameObject.transform);
            }
            else
            {
                childObject = gameObject.transform.GetChild(0).gameObject;
            }

            var childComponentB = childObject.GetComponent<ComponentB>();

            if (childComponentB == null)
            {
                childComponentB = childObject.AddComponent<ComponentB>();
            }
             
            var componentB = gameObject.GetComponent<ComponentB>();
            var json = JsonUtility.ToJson(componentB);
            JsonUtility.FromJsonOverwrite(json, childComponentB);

            var componentC = gameObject.GetComponent<ComponentC>();
            if (componentC != null && componentC.referenceToB == componentB)
            {
                componentC.referenceToB = childComponentB;
            }
            
            Object.DestroyImmediate(componentB);
            
            return new RefactorTools.RefactorResult
            {
                completed = true
            };
        });
    }
    
    [MenuItem("Refactors/Destroy DestroyableBehaviour")]
    public static void RefactorCustomMonoBehaviour()
    {
        RefactorTools.DestroyMonoBehaviour<DestroyableBehaviour>(true);
    }
    
    [MenuItem("Refactors/Refactor LogObjects With Component In Children")]
    public static void RefactorLogConsiderChildren()
    {
        RefactorTools.RefactorMonoBehaviour<ChildrenBehaviour>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ChildrenBehaviour>(AssetDatabaseExt.FindOptions.ConsiderChildren),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            Debug.Log(gameObject.name);
            
            var refactorResult = new RefactorTools.RefactorResult
            {
                completed = false
            };
            
            var childrenBehaviours = gameObject.GetComponentsInChildren<ChildrenBehaviour>();

            foreach (var childrenBehaviour in childrenBehaviours)
            {
                childrenBehaviour.value = "NEW VALUE";
                refactorResult.completed = true;
            }
            
            return refactorResult;
        });
    }
    
    [MenuItem("Refactors/Refactor LogObjects With Component In Children (consider disabled)")]
    public static void RefactorLogConsiderChildrenConsiderDisabled()
    {
        var generalResult = RefactorTools.RefactorMonoBehaviour<ChildrenBehaviour>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ChildrenBehaviour>(AssetDatabaseExt.FindOptions.ConsiderInactiveChildren),
            scenes = AssetDatabaseExt.FindAllScenes()
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData _)
        {
            Debug.Log(gameObject.name);
            
            var refactorResult = new RefactorTools.RefactorResult
            {
                completed = false
            };
            
            var childrenBehaviours = gameObject.GetComponentsInChildren<ChildrenBehaviour>(true);

            foreach (var childrenBehaviour in childrenBehaviours)
            {
                childrenBehaviour.value = "NEW VALUE";
                refactorResult.completed = true;
            }
            
            return refactorResult;
        });

        foreach (var failedToRefactorPrefab in generalResult.failedPrefabs)
        {
            Debug.LogError($"Failed to refactor prefab {failedToRefactorPrefab.name}, probably a missing script can't save prefab.", failedToRefactorPrefab);
        }
    }
    
    [MenuItem("Refactors/Replace Class Example")]
    public static void ReplaceClassExample()
    {
        var gameObject = Selection.activeGameObject;
        Debug.Log(RefactorTools.ReplaceScript<ComponentA, ComponentD>(gameObject, 
            AssetDatabaseExt.FindOptions.ConsiderInactiveChildren));
        EditorUtility.SetDirty(gameObject);
        AssetDatabase.SaveAssetIfDirty(gameObject);
    }
    
    [MenuItem("Refactors/Refactor Components implementing interface")]
    public static void RefactorInterface()
    {
        RefactorTools.RefactorMonoBehaviour<ICustomComponent>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<ICustomComponent>(),
            scenes = AssetDatabaseExt.FindAllScenes(),
            defaultDebugEnabled = true
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData data)
        {

            var components = gameObject.GetComponentsInChildren<ICustomComponent>();

            foreach (var component in components)
            {
                Debug.Log($"found {component.GetType()} with value {component.GetValue()}");
            }
            
            return new RefactorTools.RefactorResult
            {
                completed = false
            };
        });
    }

    private static int testFailureInFirstRefactor;
    
    [MenuItem("Refactors/Fail Refactor Multiple Prefabs")]
    public static void FailRefactorMultiplePrefabs()
    {
        testFailureInFirstRefactor = 0;
        
        RefactorTools.RefactorMonoBehaviour<CustomBehaviour>(new RefactorTools.RefactorParameters
        {
            prefabs = AssetDatabaseExt.FindPrefabs<CustomBehaviour>(),
            // scenes = AssetDatabaseExt.FindAllScenes(),
            defaultDebugEnabled = true,
            interruptOnFailure = true
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData data)
        {
            if (testFailureInFirstRefactor == 0)
            {
                testFailureInFirstRefactor++;
                throw new Exception($"Force Failure in prefab {AssetDatabase.GetAssetPath(gameObject)}");
            }

            return new RefactorTools.RefactorResult();
        });
    }
    
    [MenuItem("Refactors/Fail Refactoring Multiple Scenes Continue")]
    public static void FailRefactorMultipleScenes()
    {
        testFailureInFirstRefactor = 0;
        
        RefactorTools.RefactorMonoBehaviour<ComponentException>(new RefactorTools.RefactorParameters
        {
            // prefabs = AssetDatabaseExt.FindPrefabs<CustomBehaviour>(),
            scenes = AssetDatabaseExt.FindAllScenes(),
            defaultDebugEnabled = true,
            interruptOnFailure = true
        }, delegate(GameObject gameObject, 
            RefactorTools.RefactorData data)
        {
            if (testFailureInFirstRefactor == 0)
            {
                testFailureInFirstRefactor++;
                throw new Exception($"Force Failure in scene {data.scenePath}");
            }

            return new RefactorTools.RefactorResult()
            {
                completed = true
            };
        });
    }
}