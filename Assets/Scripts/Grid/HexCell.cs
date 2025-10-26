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
    private HexCellPathfindingState pathfindingState;

    private Transform terrain;

    // Constructor - initializes state machine
    public HexCell()
    {
        // Initialize states
        interactionState = new HexCellInteractionState(CellState.Invisible);
        stateManager = new HexCellStateManager(interactionState);
        pathfindingState = new HexCellPathfindingState();

        // Subscribe to granular state events for specific behaviors
        interactionState.OnEnterVisible += OnEnterVisible;
        interactionState.OnExitVisible += OnExitVisible;
        interactionState.OnEnterHighlighted += OnEnterHighlighted;
        interactionState.OnExitHighlighted += OnExitHighlighted;
        interactionState.OnEnterSelected += OnEnterSelected;
        interactionState.OnExitSelected += OnExitSelected;
        interactionState.OnEnterFocused += OnEnterFocused;
        interactionState.OnExitFocused += OnExitFocused;

        // Subscribe to general state change for logging/debugging
        interactionState.OnStateChanged += OnStateChanged;
    }

    // General state change handler (for logging/debugging)
    private void OnStateChanged(CellState from, CellState to)
    {
        // Debug.Log($"HexCell {OffsetCoordinates}: {from} -> {to}");
    }

    // ===== State Enter/Exit Handlers =====
    // These implement hierarchical visual feedback

    private void OnEnterVisible()
    {
        // Cell is now visible (fog of war revealed)
        SetFogOfWar(false);
    }

    private void OnExitVisible()
    {
        // Cell is returning to fog of war (rare, but possible in some game modes)
        SetFogOfWar(true);
    }

    private void OnEnterHighlighted()
    {
        // Mouse hover - show highlight overlay, tile info, movement cost, etc.
        SetHighlightEffect(true);
        // TODO: Show UI tooltip with tile info
        // TODO: Show movement range if unit selected
    }

    private void OnExitHighlighted()
    {
        // Mouse exit - remove highlight
        SetHighlightEffect(false);
        // TODO: Hide UI tooltip
    }

    private void OnEnterSelected()
    {
        // Cell selected - show selection indicator, enable actions
        SetSelectionEffect(true);
        // TODO: Show available actions UI
        // TODO: Play selection sound
        // TODO: Show movement/attack range if applicable
    }

    private void OnExitSelected()
    {
        // Deselected - remove selection indicator
        SetSelectionEffect(false);
        // TODO: Hide actions UI
    }

    private void OnEnterFocused()
    {
        // Camera focus - move camera, show special highlight
        SetFocusEffect(true);
        // TODO: Move camera to this cell
        // TODO: Play focus sound
        // TODO: Show detailed info panel
    }

    private void OnExitFocused()
    {
        // Unfocus - remove special effects
        SetFocusEffect(false);
        // TODO: Return camera to previous position
    }

    // ===== Visual Effect Methods =====

    private void SetFogOfWar(bool fogActive)
    {
        if (terrain == null) return;

        if (fogActive)
        {
            // TODO: Add fog/cloud overlay, hide terrain details
            // terrain.gameObject.SetActive(false); // or use shader/material
        }
        else
        {
            // TODO: Remove fog, show terrain
            terrain.gameObject.SetActive(true);
        }
    }

    private void SetHighlightEffect(bool enabled)
    {
        if (terrain == null) return;

        // TODO: Implement highlight visual (border, glow, overlay)
        // Example: Change emission color, add outline shader, show overlay quad
        // var renderer = terrain.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.material.SetColor("_EmissionColor", enabled ? Color.yellow * 0.3f : Color.black);
        // }
    }

    private void SetSelectionEffect(bool enabled)
    {
        if (terrain == null) return;

        // TODO: Implement selection visual (stronger indicator than highlight)
        // Example: Thicker border, different color, pulsing effect
        // var renderer = terrain.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.material.SetColor("_SelectionColor", enabled ? Color.green : Color.clear);
        // }
    }

    private void SetFocusEffect(bool enabled)
    {
        if (terrain == null) return;

        // TODO: Implement focus visual (most prominent effect)
        // Example: Bright glow, camera movement, special particles
        // var renderer = terrain.GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.material.SetColor("_FocusColor", enabled ? Color.cyan : Color.clear);
        // }
    }

    // ===== Public Interface Methods =====

    public void HandleInput(InputEvent inputEvent)
    {
        stateManager.HandleInput(inputEvent);
    }

    public void RevealFromFog()
    {
        stateManager.RevealFromFog();
    }

    public void Deselect()
    {
        stateManager.Deselect();
    }

    public CellState GetCurrentState()
    {
        return interactionState.State;
    }

    public bool IsExplored()
    {
        return interactionState.IsExplored();
    }

    public bool IsInteractive()
    {
        return interactionState.IsInteractive();
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
