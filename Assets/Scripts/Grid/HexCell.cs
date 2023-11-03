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

    [SerializeField]
    private CellState cellState;
    private ICellState state;
    public ICellState State { 
        get { return state; } 
        private set 
        {
            state = value;
            cellState = state.State;
        } 
    }

    //private PlayerInput playerInput;

    private Transform terrain;
    public Transform Terrain { get { return terrain; } }

    public void InitializeState(ICellState initalState = null)
    {
        if (initalState == null)
            ChangeState(new VisibleState());
        else
            ChangeState(initalState);

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
        HexTerrain hexTerrrain = terrain.GetComponentInChildren<HexTerrain>();
        hexTerrrain.OnMouseEnterAction += OnMouseEnter;
        hexTerrrain.OnMouseExitAction += OnMouseExit;
        terrain.gameObject.SetActive(false);
    }


    public void SetNeighbours(List<HexCell> neighbours)
    {
        Neighbours = neighbours;
    }

    public void ClearTerrain()
    {
        if(terrain != null)
        {
            HexTerrain hexTerrrain = terrain.GetComponent<HexTerrain>();
            hexTerrrain.OnMouseEnterAction -= OnMouseEnter;
            hexTerrrain.OnMouseExitAction -= OnMouseExit;
            UnityEngine.Object.Destroy(terrain.gameObject);
        }
    }

    public void ChangeState(ICellState newState)
    {
        if(newState == null)
        {
            Debug.LogError("Trying to set null state.");
            return;
        }

        if(State != newState)
        {
            //Debug.Log($"Changing state from {State} to {newState}");
            if(State != null)
                State.Exit(this);
            State = newState;
            State.Enter(this);
        }
    }

    private void OnMouseEnter()
    {
        ChangeState(State.OnMouseEnter());
    }

    private void OnMouseExit()
    {
        ChangeState(State.OnMouseExit());
    }

    public void OnSelect()
    {
        ChangeState(State.OnSelect());
    }

    public void OnDeselect()
    {
        ChangeState(State.OnDeselect());
    }

    public void OnFocus()
    {
        ChangeState(State.OnFocus());
    }

}
