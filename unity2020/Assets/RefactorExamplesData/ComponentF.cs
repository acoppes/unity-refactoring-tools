using UnityEngine;

namespace RefactorExamplesData
{
    public class ComponentF : MonoBehaviour, ICustomComponent
    {
        public int GetValue()
        {
            return 2;
        }
    }
}