using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class HexGrid : MonoBehaviour
{
    //TODO: Add properties for grid size, hex size, and hex prefab
    [field:SerializeField] public HexOrientation Orientation { get; private set; }
    [field:SerializeField] public int Width { get; private set; }
    [field:SerializeField] public int Height { get; private set; }
    [field:SerializeField] public float HexSize { get; private set; }

    [field:SerializeField] public int BatchSize { get; private set; }

    [SerializeField] private List<HexCell> cells = new List<HexCell>();
    private MapGenerator mapGenerator;

    private Task<List<HexCell>> hexGenerationTask;
    //TODO: Methods to get, change, add , and remove hexes
    private Vector3 gridOrigin;
    
    //The following events are linked to map loading information
    //The Map Information has been generated by the task
    public event System.Action OnMapInfoGenerated;
    //One of the batches has been sucessfully instantiated
    public event System.Action<float> OnCellBatchGenerated;
    //All cell instantiation has been completed
    public event System.Action OnCellInstancesGenerated;

    private void Awake()
    {
        gridOrigin = transform.position;
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    private void OnEnable()
    {
        if(mapGenerator != null)
        {
            mapGenerator.OnTerrainMapGenerated += SetHexCellTerrainTypes;
        }
    }

    private void OnDisable()
    {
        if (mapGenerator != null)
        {
            mapGenerator.OnTerrainMapGenerated -= SetHexCellTerrainTypes;
        }
        if (hexGenerationTask != null && hexGenerationTask.Status == TaskStatus.Running)
        {
            hexGenerationTask.Dispose();
        }
    }

    private void SetHexCellTerrainTypes(TerrainType[,] terrainMap)
    {
        Debug.Log("Setting Hex Cell Terrain Types");
        ClearHexCells();
        hexGenerationTask = Task.Run(() => GenerateHexCellData(terrainMap));
        hexGenerationTask.ContinueWith(task =>
        {
            Debug.Log("Hex Cell Data Generated");
            cells = task.Result;
            MainThreadDispatcher.Instance.Enqueue(() => StartCoroutine(InstantiateCells(cells)));
        });
    }

    private void ClearHexCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].ClearTerrain();
        }
        cells.Clear();
    }
    
    //This will become map generation
    //No Unity API allowed - including lloking up transform data, Instantiation, etc.
    private List<HexCell> GenerateHexCellData(TerrainType[,] terrainMap)
    {
        Debug.Log("Generating Hex Cell Data");
        List<HexCell> hexCells = new List<HexCell>();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int flippedX = Width - x - 1;
                int flippedY = Height - y - 1;

                //Vector3 centrePosition = HexMetrics.Center(HexSize, x, -y, Orientation) + gridOrigin;
                HexCell cell = new HexCell();
                cell.SetCoordinates(new Vector2(x, y), Orientation);
                cell.Grid = this;
                cell.HexSize = HexSize;
                cell.SetTerrainType(terrainMap[flippedX, flippedY]);
                hexCells.Add(cell);
            }
        }

        return hexCells;
    }

    //Handled by coroutine and currently the most expensive operation
    private IEnumerator InstantiateCells(List<HexCell> hexCells)
    {
        Debug.Log("Instantiating Hex Cells");
        int batchCount = 0;
        int totalBatches = Mathf.CeilToInt(hexCells.Count / BatchSize);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].CreateTerrain();
            // Yield every batchSize hex cells
            if (i % BatchSize == 0 && i != 0) 
            {
                batchCount++;
                OnCellBatchGenerated?.Invoke((float)batchCount / totalBatches);
                yield return null;
            }
        }

        OnCellInstancesGenerated?.Invoke();
    }

    Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };

    private void OnDrawGizmos()
    {
        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + transform.position;
                for (int s = 0; s < HexMetrics.Corners(HexSize, Orientation).Length; s++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(
                        centrePosition + HexMetrics.Corners(HexSize, Orientation)[s % 6], 
                        centrePosition + HexMetrics.Corners(HexSize, Orientation)[(s + 1) % 6]
                        );
                    Gizmos.color = colors[s % 6];
                    Gizmos.DrawSphere(centrePosition + HexMetrics.Corners(HexSize, Orientation)[s % 6], HexSize * 0.1f);
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
