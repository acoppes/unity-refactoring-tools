using RefactorExamplesData;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

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
        RefactorTools.RefactorMonoBehaviour<CustomBehaviour>(true, delegate(GameObject gameObject)
        {
            var behaviours = gameObject.GetComponentsInChildren<CustomBehaviour>();
            foreach (var behaviour in behaviours)
            {
                behaviour.newValue = $"VALUE:{behaviour.previousValue}";
            }
            return true;
        });
    }
    
    [MenuItem("Refactors/Refactor ComponentA to Parent")]
    public static void Refactor3()
    {
        RefactorTools.RefactorMonoBehaviour<ComponentA>(true, delegate(GameObject gameObject)
        {
            if (gameObject.transform.parent == null)
                return false;
            
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
            
            return true;
        });
    }
    
    [MenuItem("Refactors/Refactor ComponentB to Child")]
    public static void Refactor4()
    {
        RefactorTools.RefactorMonoBehaviour<ComponentB>(true, delegate(GameObject gameObject)
        {
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
            
            return true;
        });
    }
}
