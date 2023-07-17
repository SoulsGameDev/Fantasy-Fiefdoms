using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGridMeshGenerator))]
public class HexGridMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridMeshGenerator hexGridMeshGenerator = (HexGridMeshGenerator)target;

        if (GUILayout.Button("Generate Hex Mesh"))
        {
            hexGridMeshGenerator.CreateHexMesh();
        }

        if(GUILayout.Button("Clear Hex Mesh"))
        {
            hexGridMeshGenerator.ClearHexGridMesh();
        }
    }
}
