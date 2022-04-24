using UnityEngine;

namespace RefactorExamplesData
{
    [CreateAssetMenu(menuName = "Custom Data")]
    public class CustomDataAsset : ScriptableObject
    {
        public int previousValue;
        public string newValue;
    }
}
