using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Core
{
    /// <summary>
    /// Contains the result of a multi-turn pathfinding operation.
    /// Splits a long path into segments that can be completed within each turn's movement budget.
    /// </summary>
    public class MultiTurnPathResult
    {
        /// <summary>
        /// Whether a valid path was found
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The complete path from start to goal
        /// </summary>
        public List<HexCell> CompletePath { get; private set; }

        /// <summary>
        /// Path segments for each turn.
        /// Each list contains the cells that can be reached in that turn.
        /// </summary>
        public List<List<HexCell>> PathPerTurn { get; private set; }

        /// <summary>
        /// Movement cost for each turn segment
        /// </summary>
        public List<int> CostPerTurn { get; private set; }

        /// <summary>
        /// Number of turns required to complete the path
        /// </summary>
        public int TurnsRequired { get; private set; }

        /// <summary>
        /// Total movement cost of the complete path
        /// </summary>
        public int TotalCost { get; private set; }

        /// <summary>
        /// Movement points available per turn
        /// </summary>
        public int MovementPerTurn { get; private set; }

        /// <summary>
        /// The cells where the unit will end each turn (waypoints)
        /// </summary>
        public List<HexCell> TurnEndpoints { get; private set; }

        /// <summary>
        /// Starting cell of the path
        /// </summary>
        public HexCell StartCell { get; private set; }

        /// <summary>
        /// Target cell of the path
        /// </summary>
        public HexCell GoalCell { get; private set; }

        /// <summary>
        /// Reason for failure if Success is false
        /// </summary>
        public string FailureReason { get; private set; }

        /// <summary>
        /// The underlying single-turn path result (if applicable)
        /// </summary>
        public PathResult BasePathResult { get; private set; }

        /// <summary>
        /// Private constructor - use factory methods
        /// </summary>
        private MultiTurnPathResult()
        {
            CompletePath = new List<HexCell>();
            PathPerTurn = new List<List<HexCell>>();
            CostPerTurn = new List<int>();
            TurnEndpoints = new List<HexCell>();
        }

        /// <summary>
        /// Creates a successful multi-turn path result
        /// </summary>
        public static MultiTurnPathResult CreateSuccess(
            HexCell start,
            HexCell goal,
            List<HexCell> completePath,
            List<List<HexCell>> pathPerTurn,
            List<int> costPerTurn,
            int movementPerTurn,
            PathResult baseResult = null)
        {
            // Calculate turn endpoints (last cell of each turn segment)
            var endpoints = new List<HexCell>();
            foreach (var segment in pathPerTurn)
            {
                if (segment.Count > 0)
                {
                    endpoints.Add(segment[segment.Count - 1]);
                }
            }

            // Calculate total cost
            int totalCost = 0;
            foreach (int cost in costPerTurn)
            {
                totalCost += cost;
            }

            return new MultiTurnPathResult
            {
                Success = true,
                StartCell = start,
                GoalCell = goal,
                CompletePath = completePath,
                PathPerTurn = pathPerTurn,
                CostPerTurn = costPerTurn,
                TurnsRequired = pathPerTurn.Count,
                TotalCost = totalCost,
                MovementPerTurn = movementPerTurn,
                TurnEndpoints = endpoints,
                BasePathResult = baseResult,
                FailureReason = null
            };
        }

        /// <summary>
        /// Creates a failed multi-turn path result
        /// </summary>
        public static MultiTurnPathResult CreateFailure(
            HexCell start,
            HexCell goal,
            string reason,
            int movementPerTurn)
        {
            return new MultiTurnPathResult
            {
                Success = false,
                StartCell = start,
                GoalCell = goal,
                FailureReason = reason,
                MovementPerTurn = movementPerTurn,
                TurnsRequired = 0,
                TotalCost = 0
            };
        }

        /// <summary>
        /// Creates a multi-turn result from a single-turn path result by splitting it
        /// </summary>
        public static MultiTurnPathResult CreateFromSinglePath(
            PathResult singlePath,
            int movementPerTurn,
            PathfindingContext context)
        {
            if (!singlePath.Success)
            {
                return CreateFailure(
                    singlePath.StartCell,
                    singlePath.GoalCell,
                    singlePath.FailureReason,
                    movementPerTurn);
            }

            var pathPerTurn = new List<List<HexCell>>();
            var costPerTurn = new List<int>();

            List<HexCell> currentTurnPath = new List<HexCell>();
            int currentTurnCost = 0;

            // Add start cell to first turn
            currentTurnPath.Add(singlePath.Path[0]);

            // Split path into turn segments
            for (int i = 1; i < singlePath.Path.Count; i++)
            {
                HexCell cell = singlePath.Path[i];
                int stepCost = context.GetEffectiveMovementCost(cell);

                // Check if adding this cell would exceed turn's movement budget
                if (currentTurnCost + stepCost > movementPerTurn)
                {
                    // Save current turn segment
                    pathPerTurn.Add(new List<HexCell>(currentTurnPath));
                    costPerTurn.Add(currentTurnCost);

                    // Start new turn
                    currentTurnPath.Clear();
                    currentTurnPath.Add(singlePath.Path[i - 1]); // Previous cell is starting point
                    currentTurnCost = 0;
                }

                // Add cell to current turn
                currentTurnPath.Add(cell);
                currentTurnCost += stepCost;
            }

            // Add final turn segment
            if (currentTurnPath.Count > 1) // More than just the starting cell
            {
                pathPerTurn.Add(currentTurnPath);
                costPerTurn.Add(currentTurnCost);
            }

            return CreateSuccess(
                singlePath.StartCell,
                singlePath.GoalCell,
                singlePath.Path,
                pathPerTurn,
                costPerTurn,
                movementPerTurn,
                singlePath);
        }

        /// <summary>
        /// Gets the path segment for a specific turn (0-based)
        /// </summary>
        public List<HexCell> GetTurnPath(int turnIndex)
        {
            if (turnIndex >= 0 && turnIndex < PathPerTurn.Count)
            {
                return PathPerTurn[turnIndex];
            }
            return new List<HexCell>();
        }

        /// <summary>
        /// Gets the cost for a specific turn
        /// </summary>
        public int GetTurnCost(int turnIndex)
        {
            if (turnIndex >= 0 && turnIndex < CostPerTurn.Count)
            {
                return CostPerTurn[turnIndex];
            }
            return 0;
        }

        /// <summary>
        /// Gets the endpoint (last cell) for a specific turn
        /// </summary>
        public HexCell GetTurnEndpoint(int turnIndex)
        {
            if (turnIndex >= 0 && turnIndex < TurnEndpoints.Count)
            {
                return TurnEndpoints[turnIndex];
            }
            return null;
        }

        /// <summary>
        /// Checks if the path can be completed in a single turn
        /// </summary>
        public bool IsSingleTurnPath()
        {
            return Success && TurnsRequired == 1;
        }

        /// <summary>
        /// Gets the remaining path after completing a certain number of turns
        /// </summary>
        public List<HexCell> GetRemainingPath(int completedTurns)
        {
            if (!Success || completedTurns >= TurnsRequired)
            {
                return new List<HexCell>();
            }

            var remaining = new List<HexCell>();
            for (int i = completedTurns; i < PathPerTurn.Count; i++)
            {
                // Skip first cell of continuation turns (it's the ending of previous turn)
                int startIndex = (i == completedTurns) ? 0 : 1;
                for (int j = startIndex; j < PathPerTurn[i].Count; j++)
                {
                    remaining.Add(PathPerTurn[i][j]);
                }
            }
            return remaining;
        }

        /// <summary>
        /// Gets formatted information about each turn
        /// </summary>
        public string GetTurnBreakdown()
        {
            if (!Success)
            {
                return $"Path Failed: {FailureReason}";
            }

            var breakdown = $"Multi-Turn Path: {TurnsRequired} turns, Total Cost: {TotalCost}\n";
            for (int i = 0; i < TurnsRequired; i++)
            {
                var segment = PathPerTurn[i];
                var cost = CostPerTurn[i];
                var endpoint = TurnEndpoints[i];

                breakdown += $"  Turn {i + 1}: {segment.Count} cells, Cost: {cost}/{MovementPerTurn}, " +
                            $"Endpoint: {endpoint.OffsetCoordinates}\n";
            }
            return breakdown;
        }

        /// <summary>
        /// Returns a string representation
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"MultiTurnPath[{TurnsRequired} turns, {CompletePath.Count} cells, Cost: {TotalCost}]";
            }
            else
            {
                return $"MultiTurnPath[Failed: {FailureReason}]";
            }
        }

        /// <summary>
        /// Gets the average movement efficiency per turn (lower is better)
        /// </summary>
        public float GetAverageMovementEfficiency()
        {
            if (!Success || TurnsRequired == 0)
                return 0f;

            float totalEfficiency = 0f;
            foreach (int cost in CostPerTurn)
            {
                totalEfficiency += (float)cost / MovementPerTurn;
            }
            return totalEfficiency / TurnsRequired;
        }

        /// <summary>
        /// Checks if a specific turn segment is at full movement capacity
        /// </summary>
        public bool IsTurnAtCapacity(int turnIndex)
        {
            if (turnIndex < 0 || turnIndex >= CostPerTurn.Count)
                return false;

            return CostPerTurn[turnIndex] == MovementPerTurn;
        }
    }
}
