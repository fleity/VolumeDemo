using UnityEngine;
using System.Collections;

[System.Serializable]
public class Texture3DAsset : ScriptableObject
{
    [HideInInspector]
    public int resolution;
    [HideInInspector]
    public Color[] colors;
}
