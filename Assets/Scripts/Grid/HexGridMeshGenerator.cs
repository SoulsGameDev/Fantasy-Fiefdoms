using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexGridMeshGenerator : MonoBehaviour
{
    [field: SerializeField] public LayerMask gridLayer { get; private set; }
    [field:SerializeField] public HexGrid hexGrid { get; private set; }
    public Transform explosionTest;


    private void Awake()
    {
        if(hexGrid == null)
            hexGrid = GetComponentInParent<HexGrid>();
        if (hexGrid == null)
            Debug.LogError("HexGridMeshGenerator could not find a HexGrid component in its parent or itself.");
    }

    private void OnEnable()
    {
        MouseController.Instance.OnLeftMouseClick += OnLeftMouseClick;
        MouseController.Instance.OnRightMouseClick += OnRightMouseClick;
    }

    private void OnDisable()
    {
        MouseController.Instance.OnLeftMouseClick -= OnLeftMouseClick;
        MouseController.Instance.OnRightMouseClick -= OnRightMouseClick;
    }


    public void CreateHexMesh()
    {
        CreateHexMesh(hexGrid.Width, hexGrid.Height, hexGrid.HexSize, hexGrid.Orientation, gridLayer);
    }

    public void CreateHexMesh(HexGrid hexGrid, LayerMask layerMask)
    {
        this.hexGrid = hexGrid;
        this.gridLayer = layerMask;
        CreateHexMesh(hexGrid.Width, hexGrid.Height, hexGrid.HexSize, hexGrid.Orientation, layerMask);
    }

    public void CreateHexMesh(int width, int height, float hexSize, HexOrientation orientation, LayerMask layerMask)
    {
        ClearHexGridMesh();
        Vector3[] vertices = new Vector3[7 * width * height];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(hexSize, x, z, orientation);
                vertices[(z * width + x) * 7] = centrePosition;
                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    vertices[(z * width + x) * 7 + s + 1] = centrePosition + HexMetrics.Corners(hexSize, orientation)[s % 6];
                }
            }
        }

        int[] triangles = new int[3 * 6 * width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 0] = (z * width + x) * 7;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 1] = (z * width + x) * 7 + s + 1;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 2] = (z * width + x) * 7 + cornerIndex;


                }
            }
        }


        Mesh mesh = new Mesh();
        mesh.name = "Hex Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateUVDistributionMetrics();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        int gridLayerIndex = GetLayerIndex(layerMask);
        Debug.Log("Layer Index: " + gridLayerIndex);

        gameObject.layer = gridLayerIndex;
    }

    public void ClearHexGridMesh()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
            return;
        GetComponent<MeshFilter>().sharedMesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        Debug.Log("Layer Mask Value: " + layerMaskValue);
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & layerMaskValue) != 0)
            {
                Debug.Log("Layer Index Loop: " + i);
                return i;
            }
        }
        return 0;
    }

    private void OnLeftMouseClick(RaycastHit hit)
    {
        Debug.Log("Hit object: " + hit.transform.name + " at position " + hit.point);
        float localX = hit.point.x - hit.transform.position.x;
        float localZ = hit.point.z - hit.transform.position.z;
        //Debug.Log("Hex position: " + HexMetrics.CoordinateToAxial(localX, localZ, grid.HexSize, grid.Orientation));
        Debug.Log("Offset Position: " + HexMetrics.CoordinateToOffset(localX, localZ, hexGrid.HexSize, hexGrid.Orientation));
    }

    private void OnRightMouseClick(RaycastHit hit)
    {
        float localX = hit.point.x - hit.transform.position.x;
        float localZ = hit.point.z - hit.transform.position.z;
        
        Vector2 location = HexMetrics.CoordinateToOffset(localX, localZ, hexGrid.HexSize, hexGrid.Orientation);
        Vector3 center = HexMetrics.Center(hexGrid.HexSize, (int)location.x, (int)location.y, hexGrid.Orientation);
        Debug.Log("Right Clicked on Hex: " + location);
        Instantiate(explosionTest, center, Quaternion.identity);
    }

}
