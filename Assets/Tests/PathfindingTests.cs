using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Core;
using Pathfinding.Algorithms;
using Pathfinding.DataStructures;

namespace Tests
{
    /// <summary>
    /// Comprehensive test suite for the pathfinding system.
    /// Tests algorithms, data structures, and integration with the hex grid.
    /// </summary>
    public class PathfindingTests
    {
        // ========== PRIORITY QUEUE TESTS ==========

        [Test]
        public void PriorityQueue_EnqueueDequeue_ReturnsLowestPriority()
        {
            var queue = new PriorityQueue<string>();
            queue.Enqueue("Low", 1);
            queue.Enqueue("High", 10);
            queue.Enqueue("Medium", 5);

            Assert.AreEqual("Low", queue.Dequeue());
            Assert.AreEqual("Medium", queue.Dequeue());
            Assert.AreEqual("High", queue.Dequeue());
        }

        [Test]
        public void PriorityQueue_UpdatePriority_ReordersCorrectly()
        {
            var queue = new PriorityQueue<string>();
            queue.Enqueue("A", 10);
            queue.Enqueue("B", 20);
            queue.Enqueue("C", 30);

            // Update A's priority to be highest
            queue.UpdatePriority("A", 40);

            Assert.AreEqual("B", queue.Dequeue());
            Assert.AreEqual("C", queue.Dequeue());
            Assert.AreEqual("A", queue.Dequeue());
        }

        [Test]
        public void PriorityQueue_Contains_ReturnsTrueForEnqueuedItems()
        {
            var queue = new PriorityQueue<string>();
            queue.Enqueue("Test", 1);

            Assert.IsTrue(queue.Contains("Test"));
            Assert.IsFalse(queue.Contains("NotPresent"));
        }

        [Test]
        public void PriorityQueue_ValidateHeap_ReturnsTrue()
        {
            var queue = new PriorityQueue<string>();
            for (int i = 0; i < 100; i++)
            {
                queue.Enqueue($"Item{i}", Random.Range(0, 1000));
            }

            Assert.IsTrue(queue.ValidateHeap());
        }

        // ========== PATH RESULT TESTS ==========

        [Test]
        public void PathResult_CreateSuccess_SetsPropertiesCorrectly()
        {
            var path = new List<HexCell>();
            var result = PathResult.CreateSuccess(null, null, path, 10, 50, 5.5f);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(10, result.TotalCost);
            Assert.AreEqual(50, result.NodesExplored);
            Assert.AreEqual(5.5f, result.ComputationTimeMs, 0.01f);
            Assert.IsNull(result.FailureReason);
        }

        [Test]
        public void PathResult_CreateFailure_SetsPropertiesCorrectly()
        {
            var result = PathResult.CreateFailure(null, null, "No path found", 25, 3.0f);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("No path found", result.FailureReason);
            Assert.AreEqual(25, result.NodesExplored);
            Assert.AreEqual(3.0f, result.ComputationTimeMs, 0.01f);
        }

        // ========== PATHFINDING CONTEXT TESTS ==========

        [Test]
        public void PathfindingContext_CreateWithMovementLimit_SetsMaxMovement()
        {
            var context = PathfindingContext.CreateWithMovementLimit(10);

            Assert.AreEqual(10, context.MaxMovementPoints);
        }

        [Test]
        public void PathfindingContext_CreateForExploration_AllowsUnexplored()
        {
            var context = PathfindingContext.CreateForExploration();

            Assert.IsFalse(context.RequireExplored);
            Assert.AreEqual(-1, context.MaxMovementPoints);
        }

        [Test]
        public void PathfindingContext_CreateForCombat_ConfiguresCorrectly()
        {
            var context = PathfindingContext.CreateForCombat();

            Assert.IsTrue(context.AllowMoveThroughAllies);
            Assert.IsTrue(context.AvoidEnemyZones);
            Assert.IsTrue(context.PreferHighGround);
        }

        [Test]
        public void PathfindingContext_Clone_CreatesIndependentCopy()
        {
            var original = new PathfindingContext
            {
                MaxMovementPoints = 5,
                RequireExplored = false
            };

            var clone = original.Clone();
            clone.MaxMovementPoints = 10;

            Assert.AreEqual(5, original.MaxMovementPoints);
            Assert.AreEqual(10, clone.MaxMovementPoints);
        }

        // ========== HEX CELL PATHFINDING STATE TESTS ==========

        [Test]
        public void HexCellPathfindingState_ResetSearchState_ClearsTransientData()
        {
            var state = new HexCellPathfindingState();
            state.GCost = 10;
            state.HCost = 5;
            state.IsInOpenSet = true;
            state.IsInClosedSet = true;

            state.ResetSearchState();

            Assert.AreEqual(int.MaxValue, state.GCost);
            Assert.AreEqual(0, state.HCost);
            Assert.IsFalse(state.IsInOpenSet);
            Assert.IsFalse(state.IsInClosedSet);
        }

        [Test]
        public void HexCellPathfindingState_FCost_CalculatesCorrectly()
        {
            var state = new HexCellPathfindingState();
            state.GCost = 10;
            state.HCost = 5;

            Assert.AreEqual(15, state.FCost);
        }

        [Test]
        public void HexCellPathfindingState_IsTraversable_ChecksAllConditions()
        {
            var state = new HexCellPathfindingState();

            // Should be traversable by default
            Assert.IsTrue(state.IsTraversable());

            // Not traversable if not walkable
            state.IsWalkable = false;
            Assert.IsFalse(state.IsTraversable());

            // Not traversable if occupied
            state.IsWalkable = true;
            state.IsOccupied = true;
            Assert.IsFalse(state.IsTraversable());

            // Not traversable if reserved
            state.IsOccupied = false;
            state.IsReserved = true;
            Assert.IsFalse(state.IsTraversable());
        }

        [Test]
        public void HexCellPathfindingState_UpdateFromTerrain_SetsProperties()
        {
            var state = new HexCellPathfindingState();
            var terrain = ScriptableObject.CreateInstance<TerrainType>();
            terrain.isWalkable = false;
            terrain.movementCost = 3;
            terrain.defenseBonus = 0.5f;

            state.UpdateFromTerrain(terrain);

            Assert.IsFalse(state.IsWalkable);
            Assert.AreEqual(3, state.MovementCost);
            Assert.AreEqual(0.5f, state.DefenseBonus, 0.01f);
        }

        // ========== INTEGRATION TESTS (Require actual HexGrid setup) ==========

        // NOTE: The following tests would require a proper HexGrid setup in a Unity Test Runner
        // They are commented out as placeholders for when you want to add them

        /*
        [Test]
        public void AStar_FindPath_StraightLine_ReturnsOptimalPath()
        {
            // Setup: Create a simple 5x5 grid
            // Create start at (0,0) and goal at (4,0)
            // Expected: Path should be straight line with 5 cells

            // var grid = CreateTestGrid(5, 5);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[4, 0];
            // var algorithm = new AStarPathfinding();
            // var context = new PathfindingContext();

            // var result = algorithm.FindPath(start, goal, context);

            // Assert.IsTrue(result.Success);
            // Assert.AreEqual(5, result.PathLength);
        }

        [Test]
        public void AStar_FindPath_AroundObstacle_FindsAlternate()
        {
            // Setup: Create grid with obstacle blocking direct path
            // Expected: Path should go around obstacle

            // var grid = CreateTestGrid(5, 5);
            // var start = grid.Cells[0, 2];
            // var goal = grid.Cells[4, 2];
            //
            // // Block direct path
            // grid.Cells[2, 2].PathfindingState.IsWalkable = false;
            //
            // var algorithm = new AStarPathfinding();
            // var result = algorithm.FindPath(start, goal, new PathfindingContext());
            //
            // Assert.IsTrue(result.Success);
            // Assert.Greater(result.PathLength, 5); // Longer than straight line
        }

        [Test]
        public void AStar_FindPath_NoPath_ReturnsFail()
        {
            // Setup: Create grid with goal completely surrounded by unwalkable cells
            // Expected: No path found

            // var grid = CreateTestGrid(5, 5);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[2, 2];
            //
            // // Surround goal with obstacles
            // foreach (var neighbor in goal.GetNeighbors())
            // {
            //     neighbor.PathfindingState.IsWalkable = false;
            // }
            //
            // var algorithm = new AStarPathfinding();
            // var result = algorithm.FindPath(start, goal, new PathfindingContext());
            //
            // Assert.IsFalse(result.Success);
            // Assert.IsNotNull(result.FailureReason);
        }

        [Test]
        public void PathfindingManager_GetReachableCells_ReturnsCorrectCount()
        {
            // Setup: Create grid and test reachability calculation
            // Expected: Number of reachable cells matches expected for given movement

            // var grid = CreateTestGrid(5, 5);
            // var start = grid.Cells[2, 2]; // Center of grid
            //
            // var reachable = PathfindingManager.Instance.GetReachableCells(start, 2);
            //
            // Assert.Greater(reachable.Count, 0);
            // Assert.LessOrEqual(reachable.Count, 13); // Max 2 hex rings from center
        }
        */

        // ========== MULTI-TURN PATHFINDING TESTS ==========

        [Test]
        public void MultiTurnPathResult_CreateSuccess_SetsPropertiesCorrectly()
        {
            var completePath = new List<HexCell>();
            var pathPerTurn = new List<List<HexCell>>();
            var costPerTurn = new List<int> { 5, 4, 3 };

            pathPerTurn.Add(new List<HexCell>());
            pathPerTurn.Add(new List<HexCell>());
            pathPerTurn.Add(new List<HexCell>());

            var result = MultiTurnPathResult.CreateSuccess(
                null, null, completePath, pathPerTurn, costPerTurn, 5);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.TurnsRequired);
            Assert.AreEqual(12, result.TotalCost); // 5 + 4 + 3
            Assert.AreEqual(5, result.MovementPerTurn);
        }

        [Test]
        public void MultiTurnPathResult_CreateFailure_SetsPropertiesCorrectly()
        {
            var result = MultiTurnPathResult.CreateFailure(
                null, null, "Test failure", 5);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Test failure", result.FailureReason);
            Assert.AreEqual(0, result.TurnsRequired);
            Assert.AreEqual(5, result.MovementPerTurn);
        }

        [Test]
        public void MultiTurnPathResult_IsSingleTurnPath_ReturnsCorrectly()
        {
            // Single turn path
            var singleTurn = new List<List<HexCell>>
            {
                new List<HexCell>()
            };
            var singleResult = MultiTurnPathResult.CreateSuccess(
                null, null, new List<HexCell>(), singleTurn, new List<int> { 5 }, 10);

            Assert.IsTrue(singleResult.IsSingleTurnPath());

            // Multi-turn path
            var multiTurn = new List<List<HexCell>>
            {
                new List<HexCell>(),
                new List<HexCell>()
            };
            var multiResult = MultiTurnPathResult.CreateSuccess(
                null, null, new List<HexCell>(), multiTurn, new List<int> { 5, 3 }, 10);

            Assert.IsFalse(multiResult.IsSingleTurnPath());
        }

        [Test]
        public void MultiTurnPathResult_GetAverageMovementEfficiency_CalculatesCorrectly()
        {
            var pathPerTurn = new List<List<HexCell>>
            {
                new List<HexCell>(),
                new List<HexCell>()
            };
            var costPerTurn = new List<int> { 10, 8 }; // 100% and 80% of 10 movement

            var result = MultiTurnPathResult.CreateSuccess(
                null, null, new List<HexCell>(), pathPerTurn, costPerTurn, 10);

            float efficiency = result.GetAverageMovementEfficiency();
            Assert.AreEqual(0.9f, efficiency, 0.01f); // (1.0 + 0.8) / 2
        }

        [Test]
        public void MultiTurnPathResult_IsTurnAtCapacity_ChecksCorrectly()
        {
            var pathPerTurn = new List<List<HexCell>>
            {
                new List<HexCell>(),
                new List<HexCell>()
            };
            var costPerTurn = new List<int> { 10, 8 };

            var result = MultiTurnPathResult.CreateSuccess(
                null, null, new List<HexCell>(), pathPerTurn, costPerTurn, 10);

            Assert.IsTrue(result.IsTurnAtCapacity(0)); // Turn 0 uses all 10 movement
            Assert.IsFalse(result.IsTurnAtCapacity(1)); // Turn 1 uses only 8
        }

        [Test]
        public void MultiTurnPathResult_GetTurnBreakdown_ReturnsFormattedString()
        {
            var pathPerTurn = new List<List<HexCell>>
            {
                new List<HexCell>(),
                new List<HexCell>()
            };
            var costPerTurn = new List<int> { 5, 3 };

            var result = MultiTurnPathResult.CreateSuccess(
                null, null, new List<HexCell>(), pathPerTurn, costPerTurn, 5);

            string breakdown = result.GetTurnBreakdown();

            Assert.IsTrue(breakdown.Contains("2 turns"));
            Assert.IsTrue(breakdown.Contains("Total Cost: 8"));
        }

        // NOTE: The following tests would require a proper HexGrid setup in Unity Test Runner
        // They are placeholders for integration tests

        /*
        [Test]
        public void MultiTurnPathfinding_LongPath_SplitsIntoMultipleTurns()
        {
            // Setup: Create path that requires multiple turns
            // var grid = CreateTestGrid(20, 20);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[15, 15];
            // int movementPerTurn = 5;
            //
            // var result = PathfindingManager.Instance.FindMultiTurnPath(start, goal, movementPerTurn);
            //
            // Assert.IsTrue(result.Success);
            // Assert.Greater(result.TurnsRequired, 1);
            // Assert.AreEqual(result.CompletePath.Count, result.PathPerTurn.Sum(p => p.Count));
        }

        [Test]
        public void MultiTurnPathfinding_ShortPath_SingleTurn()
        {
            // Setup: Create path within single turn range
            // var grid = CreateTestGrid(10, 10);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[3, 3];
            // int movementPerTurn = 10;
            //
            // var result = PathfindingManager.Instance.FindMultiTurnPath(start, goal, movementPerTurn);
            //
            // Assert.IsTrue(result.Success);
            // Assert.IsTrue(result.IsSingleTurnPath());
        }

        [Test]
        public void GetMultiTurnReachableCells_ReturnsCorrectTurns()
        {
            // Setup: Test multi-turn reachability
            // var grid = CreateTestGrid(10, 10);
            // var start = grid.Cells[5, 5];
            // int movementPerTurn = 3;
            // int maxTurns = 3;
            //
            // var cellsByTurn = PathfindingManager.Instance.GetMultiTurnReachableCells(
            //     start, movementPerTurn, maxTurns);
            //
            // Assert.AreEqual(maxTurns, cellsByTurn.Count);
            // Assert.Greater(cellsByTurn[2].Count, cellsByTurn[1].Count); // More cells reachable in turn 3 than turn 2
        }

        [Test]
        public void EstimateTurnsToReach_ReturnsReasonableEstimate()
        {
            // Setup: Test turn estimation
            // var grid = CreateTestGrid(20, 20);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[15, 15];
            // int movementPerTurn = 5;
            //
            // int estimate = PathfindingManager.Instance.EstimateTurnsToReach(start, goal, movementPerTurn);
            //
            // Assert.Greater(estimate, 0);
            //
            // // Compare with actual
            // var actual = PathfindingManager.Instance.FindMultiTurnPath(start, goal, movementPerTurn);
            // if (actual.Success)
            // {
            //     // Estimate should be within reasonable range of actual
            //     Assert.LessOrEqual(Mathf.Abs(estimate - actual.TurnsRequired), 2);
            // }
        }

        [Test]
        public void MultiTurnPath_GetRemainingPath_ReturnsCorrectCells()
        {
            // Setup: Create multi-turn path and get remaining portion
            // var grid = CreateTestGrid(15, 15);
            // var start = grid.Cells[0, 0];
            // var goal = grid.Cells[10, 10];
            // int movementPerTurn = 4;
            //
            // var fullPath = PathfindingManager.Instance.FindMultiTurnPath(start, goal, movementPerTurn);
            //
            // Assert.IsTrue(fullPath.Success);
            // Assert.Greater(fullPath.TurnsRequired, 2);
            //
            // // Get remaining path after 1 turn
            // var remaining = fullPath.GetRemainingPath(1);
            //
            // Assert.Greater(remaining.Count, 0);
            // Assert.Less(remaining.Count, fullPath.CompletePath.Count);
        }
        */
    }
}

