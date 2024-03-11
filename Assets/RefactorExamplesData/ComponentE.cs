using UnityEngine;

namespace RefactorExamplesData
{
    public class ComponentE : MonoBehaviour, ICustomComponent
    {
        public int GetValue()
        {
            return 1;
        }
    }
}