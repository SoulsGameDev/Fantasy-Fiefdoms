using System;
using System.Collections.Generic;
using UnityEngine;

// =====================================================
// EXAMPLE COMMANDS - Templates for various game actions
// =====================================================

/// <summary>
/// Example: Command for moving a unit from one cell to another
/// This demonstrates how to implement undo/redo for unit movement
/// </summary>
public class MoveUnitCommand : CommandBase
{
    private GameObject unit; // Replace with your Unit class
    private HexCell fromCell;
    private HexCell toCell;
    private Vector3 originalPosition;
    private Vector3 targetPosition;

    public override string Description => $"Move {unit.name} from {fromCell.OffsetCoordinates} to {toCell.OffsetCoordinates}";

    public MoveUnitCommand(GameObject unit, HexCell fromCell, HexCell toCell)
    {
        this.unit = unit;
        this.fromCell = fromCell;
        this.toCell = toCell;
        this.originalPosition = unit.transform.position;
    }

    public override bool CanExecute()
    {
        // Add validation logic here
        // - Is the path walkable?
        // - Does unit have enough movement points?
        // - Is target cell occupied?
        return unit != null && fromCell != null && toCell != null && !isExecuted;
    }

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        try
        {
            // TODO: Implement actual movement logic
            // - Deduct movement points
            // - Update cell occupation
            // - Play movement animation

            // Example: Simple position change
            Vector3 targetPos = HexMetrics.Center(
                toCell.HexSize,
                (int)toCell.OffsetCoordinates.x,
                (int)toCell.OffsetCoordinates.y,
                HexOrientation.PointyTop // Adjust based on your orientation
            );
            targetPosition = targetPos + toCell.Grid.transform.position;
            unit.transform.position = targetPosition;

            isExecuted = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to execute MoveUnitCommand: {e.Message}");
            return false;
        }
    }

    public override bool Undo()
    {
        if (!CanUndo()) return false;

        try
        {
            // Restore original position
            unit.transform.position = originalPosition;

            // TODO: Restore movement points, cell occupation, etc.

            isExecuted = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to undo MoveUnitCommand: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Example: Command for revealing fog of war in an area
/// Demonstrates batch operations on multiple cells
/// </summary>
public class RevealFogCommand : CommandBase
{
    private List<HexCell> cellsToReveal;
    private List<HexCell> cellsRevealed = new List<HexCell>(); // Track which were actually revealed

    public override string Description => $"Reveal fog of war ({cellsToReveal.Count} cells)";

    public RevealFogCommand(List<HexCell> cells)
    {
        this.cellsToReveal = new List<HexCell>(cells);
    }

    public RevealFogCommand(HexCell centerCell, int radius)
    {
        // TODO: Implement GetCellsInRadius method
        // this.cellsToReveal = GetCellsInRadius(centerCell, radius);
        this.cellsToReveal = new List<HexCell> { centerCell };
    }

    public override bool CanExecute()
    {
        return cellsToReveal != null && cellsToReveal.Count > 0 && !isExecuted;
    }

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        try
        {
            cellsRevealed.Clear();

            foreach (var cell in cellsToReveal)
            {
                if (cell.GetCurrentState() == CellState.Invisible)
                {
                    cell.RevealFromFog();
                    cellsRevealed.Add(cell);
                }
            }

            isExecuted = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to execute RevealFogCommand: {e.Message}");
            return false;
        }
    }

    public override bool Undo()
    {
        if (!CanUndo()) return false;

        try
        {
            // Return revealed cells back to fog of war
            foreach (var cell in cellsRevealed)
            {
                cell.ForceState(CellState.Invisible);
            }

            isExecuted = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to undo RevealFogCommand: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Example: Command for building a structure on a hex cell
/// Demonstrates resource management and validation in commands
/// </summary>
public class BuildStructureCommand : CommandBase
{
    private HexCell cell;
    private string structureType; // Replace with your Structure class
    private GameObject structureInstance;
    private int resourceCost;
    private bool hadEnoughResources;

    public override string Description => $"Build {structureType} at {cell.OffsetCoordinates}";

    public BuildStructureCommand(HexCell cell, string structureType, int resourceCost)
    {
        this.cell = cell;
        this.structureType = structureType;
        this.resourceCost = resourceCost;
    }

    public override bool CanExecute()
    {
        // Add validation:
        // - Is cell explored?
        // - Is cell empty?
        // - Does player have enough resources?
        // - Is terrain suitable?

        // Example check
        bool isExplored = cell.IsExplored();
        // bool hasResources = PlayerResources.Instance.GetResource("gold") >= resourceCost;

        return cell != null && isExplored && !isExecuted;
    }

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        try
        {
            // TODO: Implement actual building logic
            // - Deduct resources
            // - Instantiate structure prefab
            // - Update cell data

            // Example:
            // PlayerResources.Instance.DeductResource("gold", resourceCost);
            // structureInstance = GameObject.Instantiate(structurePrefab, cell.transform);

            Debug.Log($"Built {structureType} at {cell.OffsetCoordinates}");

            isExecuted = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to execute BuildStructureCommand: {e.Message}");
            return false;
        }
    }

    public override bool Undo()
    {
        if (!CanUndo()) return false;

        try
        {
            // Remove structure and refund resources
            if (structureInstance != null)
            {
                UnityEngine.Object.Destroy(structureInstance);
            }

            // TODO: Refund resources
            // PlayerResources.Instance.AddResource("gold", resourceCost);

            isExecuted = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to undo BuildStructureCommand: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Example: Complex action combining multiple commands
/// Demonstrates how to use MacroCommand for composite actions
/// </summary>
public static class ComplexCommandExamples
{
    /// <summary>
    /// Example: Move unit and reveal fog of war at destination
    /// </summary>
    public static ICommand CreateMoveAndRevealCommand(GameObject unit, HexCell from, HexCell to)
    {
        var macro = new MacroCommand($"Move {unit.name} and reveal fog");

        // Add movement command
        macro.AddCommand(new MoveUnitCommand(unit, from, to));

        // Add fog reveal command for destination area
        // var cellsToReveal = GetCellsInRadius(to, 3); // Vision range
        var cellsToReveal = new List<HexCell> { to };
        macro.AddCommand(new RevealFogCommand(cellsToReveal));

        return macro;
    }

    /// <summary>
    /// Example: Select new cell and deselect old cell
    /// </summary>
    public static ICommand CreateSwitchSelectionCommand(HexCell oldCell, HexCell newCell)
    {
        var macro = new MacroCommand("Switch selection");

        // Deselect old cell
        if (oldCell != null && oldCell.GetCurrentState() != CellState.Visible)
        {
            macro.AddCommand(new StateChangeCommand(oldCell, CellState.Visible));
        }

        // Select new cell
        macro.AddCommand(new StateChangeCommand(newCell, CellState.Selected));

        return macro;
    }
}
