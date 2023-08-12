using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class HexGrid : MonoBehaviour
{
    //TODO: Add properties for grid size, hex size, and hex prefab
    [field:SerializeField] public HexOrientation Orientation { get; private set; }
    [field:SerializeField] public int Width { get; private set; }
    [field:SerializeField] public int Height { get; private set; }
    [field:SerializeField] public float HexSize { get; private set; }

    [field:SerializeField] public int BatchSize { get; private set; }

    [SerializeField] private List<HexCell> cells = new List<HexCell>();

    private Task<List<HexCell>> hexGenerationTask;
    //TODO: Methods to get, change, add , and remove tiles
    private Vector3 gridOrigin;
    
    public event System.Action OnMapInfoGenerated;
    public event System.Action<float> OnCellBatchGenerated;
    public event System.Action OnCellInstancesGenerated;

    private void Awake()
    {
        gridOrigin = transform.position;
    }

    private void Start()
    {
        hexGenerationTask = Task.Run(() => GenerateHexCellData());
    }

    private void Update()
    {
        if (hexGenerationTask != null && hexGenerationTask.IsCompleted)
        {
            cells = hexGenerationTask.Result;
            OnMapInfoGenerated?.Invoke();
            StartCoroutine(InstantiateCells());
            hexGenerationTask = null; // Clear the task
        }
    }

    private List<HexCell> GenerateHexCellData()
    {
        System.Random rng = new System.Random();
        List<HexCell> hexCells = new List<HexCell>();

        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(HexSize, x, z, Orientation) + gridOrigin;
                HexCell cell = new HexCell();
                cell.SetCoordinates(new Vector2(x, z), Orientation);
                cell.Grid = this;
                cell.HexSize = HexSize;
                //Temporary until we have a proper terrain generation system
                int randomTerrainTypeIndex = rng.Next(0, ResourceManager.Instance.TerrainTypes.Count);
                TerrainType terrain = ResourceManager.Instance.TerrainTypes[randomTerrainTypeIndex];
                cell.SetTerrainType(terrain);
                hexCells.Add(cell);
            }
        }

        return hexCells;
    }

    private IEnumerator InstantiateCells()
    {
        int batchCount = 0;
        int totalBatches = Mathf.CeilToInt(cells.Count / BatchSize);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].CreateTerrain();
            // Yield every batchSize hex cells
            if (i % BatchSize == 0) 
            {
                batchCount++;
                OnCellBatchGenerated?.Invoke((float)batchCount / totalBatches);
                yield return null;
            }
        }

        OnCellInstancesGenerated?.Invoke();
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
