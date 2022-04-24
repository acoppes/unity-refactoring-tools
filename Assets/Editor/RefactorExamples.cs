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
}
