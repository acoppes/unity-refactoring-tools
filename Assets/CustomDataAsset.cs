using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom Data")]
public class CustomDataAsset : ScriptableObject
{
    public int previousValue;
    public string newValue;
}
