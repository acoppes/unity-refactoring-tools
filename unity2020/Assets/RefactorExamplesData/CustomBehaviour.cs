using System;
using UnityEngine;

namespace RefactorExamplesData
{
    [Serializable]
    public struct Speed
    {
        public float baseValue;
        public float incrementValue;
    }
    
    public class CustomBehaviour : MonoBehaviour
    {
        public float speedBaseValue;
        public float speedIncrementValue;

        public Speed speed;
    }
}
