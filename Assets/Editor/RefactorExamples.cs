using RefactorExamplesData;
using UnityEngine;
using Utils.Editor;

public static class RefactorExamples
{
    [UnityEditor.MenuItem("Refactors/Refactor Custom Data")]
    public static void RefactorCustomData()
    {
        RefactorTools.RefactorAsset(delegate(CustomDataAsset asset)
        {
            asset.newValue = $"VALUE:{asset.previousValue}";
            return true;
        });
    }
    
    [UnityEditor.MenuItem("Refactors/Refactor Custom MonoBehaviour")]
    public static void RefactorCustomMonoBehaviour()
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
}
