using Gemserk.RefactorTools.Editor;
using RefactorExamplesData;
using UnityEditor;
using UnityEngine;

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
}