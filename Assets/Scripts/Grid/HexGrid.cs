using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    //TODO: Add properties for grid size, hex size, and hex prefab
    [field:SerializeField] public HexOrientation Orientation { get; private set; }
    [field:SerializeField] public int Width { get; private set; }
    [field:SerializeField] public int Height { get; private set; }
    [field:SerializeField] public float HexSize { get; private set; }

    [SerializeField] private List<HexCell> cells = new List<HexCell>();
    //TODO: Methods to get, change, add , and remove tiles

    
    public event System.Action OnGenerationComplete;

    private void Start()
    {
        StartCoroutine(GenerateHexCells());
    }

    private IEnumerator GenerateHexCells()
    {
        float targetFrameTime = 1f / 60f; // Approximately 0.01667 seconds for 60fps
        float accumulatedTime = 0f;

        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + transform.position;
                HexCell cell = new HexCell();
                cell.SetCoordinates(new Vector2(x, z), Orientation);
                cell.Grid = this;
                cell.HexSize = HexSize;
                //Temporary until we have a proper terrain generation system
                cell.SetTerrainType(ResourceManager.Instance.TerrainTypes[Random.Range(0,ResourceManager.Instance.TerrainTypes.Count)]);
                cells.Add(cell);

                accumulatedTime += Time.deltaTime;

                if (accumulatedTime >= targetFrameTime)
                {
                    yield return null; // Wait for the next frame
                    accumulatedTime = 0f; // Reset the accumulated time
                }
            }
        }

        // Trigger the event after the generation is complete
        OnGenerationComplete?.Invoke();
    }

    private void OnDrawGizmos()
    {
        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + transform.position;
                for (int s = 0; s < HexMetrics.Corners(HexSize, Orientation).Length; s++)
                {
                    Gizmos.DrawLine(
                        centrePosition + HexMetrics.Corners(HexSize, Orientation)[s % 6], 
                        centrePosition + HexMetrics.Corners(HexSize, Orientation)[(s + 1) % 6]
                        );
                }
            }
        }
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}
