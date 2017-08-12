using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImportTexture3D))]
public class ImportTexture3DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImportTexture3D importTexture3D = (ImportTexture3D)target;
        if (GUILayout.Button("Import Texture3d To Material"))
        {
            importTexture3D.ImportTexture3dToMaterial();
        }
    }
}