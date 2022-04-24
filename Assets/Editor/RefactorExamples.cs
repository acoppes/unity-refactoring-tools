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

            if (parentGameObject.GetComponent<ComponentA>() != null)
                return false;
             
            var componentA = gameObject.GetComponent<ComponentA>();
            var parentComponentA = parentGameObject.AddComponent<ComponentA>();

            var json = JsonUtility.ToJson(componentA);
            JsonUtility.FromJsonOverwrite(json, parentComponentA);
            
            Object.DestroyImmediate(componentA);
            
            return true;
        });
    }
}
