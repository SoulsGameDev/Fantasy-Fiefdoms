using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public LayerMask gridLayerMask;

    public HexGrid grid;

    private void Awake()
    {
        grid = FindObjectOfType<HexGrid>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick(gridLayerMask);
        }
    }

    void CheckMouseClick(LayerMask layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Hit object: " + hit.transform.name + " at position " + hit.point);
            float localX = hit.point.x - hit.transform.position.x;
            float localZ = hit.point.z - hit.transform.position.z;
            Debug.Log("Hex position: " + HexMetrics.CoordinateToHex(localX, localZ, grid.HexSize, grid.Orientation));
        }
    }
}