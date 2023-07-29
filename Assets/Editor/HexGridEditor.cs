using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    void OnSceneGUI()
    {
        HexGrid hexGrid = (HexGrid)target;
        
        for (int z = 0; z < hexGrid.Height; z++)
        {
            for (int x = 0; x < hexGrid.Width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(hexGrid.HexSize, x, z, hexGrid.Orientation) + hexGrid.transform.position;

                int centerX = x;//- hexGrid.Width / 2 + x;
                int centerZ = z;//- hexGrid.Height / 2 + z;
                // Show the coordinates in a label
                Vector3 cubeCoord = HexMetrics.OffsetToCube(centerX, centerZ, hexGrid.Orientation);
                Handles.Label(centrePosition + Vector3.forward*0.5f, $"[{centerX}, {centerZ}]");
                Handles.Label(centrePosition, $"({cubeCoord.x}, {cubeCoord.y}, {cubeCoord.z})");
            }
        }
    }
}
