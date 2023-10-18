using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    [SerializeField]private HexOrientation orientation;
    [field:SerializeField] public HexGrid Grid { get; set; }
    [field:SerializeField] public float HexSize { get; set; }
    [field:SerializeField] public TerrainType TerrainType { get; private set; }
    [field:SerializeField] public Vector2 OffsetCoordinates { get; set; }
    [field:SerializeField] public Vector3 CubeCoordinates { get; private set; }
    [field:SerializeField] public Vector2 AxialCoordinates { get; private set; }
    [field:NonSerialized]public List<HexCell> Neighbours { get; private set; }

    private HexCellInteractionState interactionState;
    private HexCellStateManager stateManager;
    //private PlayerInput playerInput;

    private Transform terrain;

    private void Awake(){
        // Get the PlayerInput component
       // playerInput = GetComponent<PlayerInput>();

        // Initialize the state to Invisible
        interactionState.SetState(CellState.Invisible);

        // Create the state manager
        stateManager = new HexCellStateManager(interactionState);

        // Subscribe to the OnStateChanged event
        interactionState.OnStateChanged += OnStateChanged;
    }
    private void OnStateChanged(CellState state)
    {
        // Update the appearance of the cell based on the new state
        // ...
    }

    public void SetCoordinates(Vector2 offsetCoordinates, HexOrientation orientation)
    {
        this.orientation = orientation;
        OffsetCoordinates = offsetCoordinates;
        CubeCoordinates = HexMetrics.OffsetToCube(offsetCoordinates, orientation);
        AxialCoordinates = HexMetrics.CubeToAxial(CubeCoordinates);

    }

    public void SetTerrainType(TerrainType terrainType)
    {
        TerrainType = terrainType;
    }

    public void CreateTerrain()
    {
        if(TerrainType == null)
        {
            Debug.LogError("TerrainType is null");
            return;
        }
        if(Grid == null)
        {
            Debug.LogError("Grid is null");
            return;
        }
        if (HexSize == 0)
        {
            Debug.LogError("HexSize is 0");
            return;
        }
        if (TerrainType.Prefab == null)
        {
            Debug.LogError("TerrainType Prefab is null");
            return;
        }

        Vector3 centrePosition = HexMetrics.Center(
            HexSize, 
            (int)OffsetCoordinates.x, 
            (int)OffsetCoordinates.y, orientation
            ) + Grid.transform.position;

        terrain = UnityEngine.Object.Instantiate(
            TerrainType.Prefab,
            centrePosition, 
            Quaternion.identity, 
            Grid.transform
            );
        terrain.gameObject.layer = LayerMask.NameToLayer("Grid");

        //TODO: Adjust the size of the prefab to the size of the grid cell
        
        if(orientation == HexOrientation.FlatTop)
        {
            terrain.Rotate(new Vector3(0, 30, 0));
        }
        //Temporary random rotation to make the terrain look more natural
        int randomRotation = UnityEngine.Random.Range(0, 6);
        terrain.Rotate(new Vector3(0, randomRotation*60, 0));
    }


    public void SetNeighbours(List<HexCell> neighbours)
    {
        Neighbours = neighbours;
    }

    public void ClearTerrain()
    {
        if(terrain != null)
        {
            UnityEngine.Object.Destroy(terrain.gameObject);
        }
    }

}
