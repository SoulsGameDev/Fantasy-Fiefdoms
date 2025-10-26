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
    }
}
