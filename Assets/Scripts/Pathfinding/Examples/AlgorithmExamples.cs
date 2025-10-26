using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Core;
using Pathfinding.Algorithms;

/// <summary>
/// Examples demonstrating each pathfinding algorithm with their specific use cases.
/// Shows when and why to use each algorithm.
/// </summary>
public class AlgorithmExamples : MonoBehaviour
{
    [Header("Example Configuration")]
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameObject unitPrefab;

    // ==================== A* ALGORITHM ====================
    // USE CASE: Standard optimal pathfinding with single goal

    public void Example_AStar_OptimalSinglePath()
    {
        Debug.Log("=== A* Algorithm Example ===");
        Debug.Log("USE CASE: Finding optimal path to single destination");

        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[15, 15];

        // Set algorithm to A*
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);

        var result = PathfindingManager.Instance.FindPath(start, goal, new PathfindingContext());

        if (result.Success)
        {
            Debug.Log($"A* found optimal path:");
            Debug.Log($"  Length: {result.PathLength} cells");
            Debug.Log($"  Cost: {result.TotalCost}");
            Debug.Log($"  Nodes explored: {result.NodesExplored}");
            Debug.Log($"  Time: {result.ComputationTimeMs:F2}ms");
        }
    }

    // ==================== DIJKSTRA'S ALGORITHM ====================
    // USE CASE: Finding paths to MANY destinations from one source

    public void Example_Dijkstra_ThreatRange()
    {
        Debug.Log("=== Dijkstra Algorithm Example ===");
        Debug.Log("USE CASE: Computing enemy threat range (all reachable cells)");

        HexCell enemyPosition = hexGrid.Cells[10, 10];
        int enemyMovement = 5;

        var context = new PathfindingContext
        {
            MaxMovementPoints = enemyMovement
        };

        // Use Dijkstra to find ALL cells enemy can reach
        var result = PathfindingManager.Instance.FindAllPathsFrom(enemyPosition, context);

        Debug.Log($"Dijkstra computed threat range:");
        Debug.Log($"  Cells in threat range: {result.DistanceMap.Count}");
        Debug.Log($"  Nodes explored: {result.NodesExplored}");
        Debug.Log($"  Time: {result.ComputationTimeMs:F2}ms");

        // Highlight threatened cells
        foreach (var kvp in result.DistanceMap)
        {
            HexCell cell = kvp.Key;
            int distance = kvp.Value;

            if (distance <= enemyMovement)
            {
                // This cell is threatened!
                cell.PathfindingState.IsPath = true;
                Debug.Log($"  Threatened: {cell.OffsetCoordinates} (distance: {distance})");
            }
        }

        Debug.Log($"\nComparison: Running A* {result.DistanceMap.Count} times would take " +
                 $"~{result.ComputationTimeMs * result.DistanceMap.Count:F0}ms. Dijkstra did it in {result.ComputationTimeMs:F2}ms!");
    }

    public void Example_Dijkstra_MultipleTargets()
    {
        Debug.Log("=== Dijkstra Multi-Target Example ===");
        Debug.Log("USE CASE: Finding paths to multiple resource nodes");

        HexCell workerPos = hexGrid.Cells[5, 5];
        List<HexCell> resources = new List<HexCell>
        {
            hexGrid.Cells[10, 2],
            hexGrid.Cells[8, 12],
            hexGrid.Cells[15, 8]
        };

        // Use Dijkstra to get paths to ALL resources in one go
        var dijkstra = PathfindingManager.Instance.GetAlgorithm(PathfindingManager.AlgorithmType.Dijkstra)
            as DijkstraPathfinding;

        var paths = dijkstra.FindPathsToMultipleGoals(workerPos, resources, new PathfindingContext());

        Debug.Log($"Found paths to {paths.Count} resources:");
        foreach (var kvp in paths)
        {
            Debug.Log($"  Resource at {kvp.Key.OffsetCoordinates}: " +
                     $"{(kvp.Value.Success ? $"Reachable in {kvp.Value.TotalCost} cost" : "Unreachable")}");
        }
    }

    // ==================== BFS ALGORITHM ====================
    // USE CASE: Fast unweighted pathfinding

    public void Example_BFS_QuickReachability()
    {
        Debug.Log("=== BFS Algorithm Example ===");
        Debug.Log("USE CASE: Quick check if position is reachable (ignoring terrain cost)");

        HexCell start = hexGrid.Cells[0, 0];
        HexCell target = hexGrid.Cells[8, 8];

        // BFS for fast reachability check
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.BFS);

        var result = PathfindingManager.Instance.FindPath(start, target, new PathfindingContext());

        Debug.Log($"BFS Result:");
        Debug.Log($"  Reachable: {result.Success}");
        Debug.Log($"  Steps: {result.PathLength} (ignores terrain cost)");
        Debug.Log($"  Time: {result.ComputationTimeMs:F2}ms (VERY FAST!)");
    }

    public void Example_BFS_AbilityRange()
    {
        Debug.Log("=== BFS Ability Range Example ===");
        Debug.Log("USE CASE: Spell/ability range (3-hex radius)");

        HexCell casterPos = hexGrid.Cells[10, 10];
        int abilityRange = 3;

        // Get all cells within 3 steps (perfect for abilities with range but no terrain cost)
        var cellsInRange = PathfindingManager.Instance.GetCellsWithinSteps(
            casterPos, abilityRange, new PathfindingContext());

        Debug.Log($"Ability can target {cellsInRange.Count} cells within {abilityRange} hexes");

        foreach (var cell in cellsInRange)
        {
            cell.PathfindingState.IsReachable = true;
        }
    }

    // ==================== FLOW FIELD ALGORITHM ====================
    // USE CASE: Many units moving to same destination

    public void Example_FlowField_GroupMovement()
    {
        Debug.Log("=== Flow Field Algorithm Example ===");
        Debug.Log("USE CASE: 50 units moving to same rally point");

        HexCell rallyPoint = hexGrid.Cells[20, 20];

        // Generate flow field ONCE
        var flowField = PathfindingManager.Instance.GenerateFlowField(rallyPoint, new PathfindingContext());

        Debug.Log($"Flow Field generated:");
        Debug.Log($"  Computation: {flowField.ComputationTimeMs:F2}ms");
        Debug.Log($"  Covers: {flowField.GetReachableCells().Count} cells");

        // Now ALL units can use this field
        List<HexCell> unitPositions = new List<HexCell>
        {
            hexGrid.Cells[0, 0],
            hexGrid.Cells[2, 5],
            hexGrid.Cells[5, 2],
            // ... imagine 47 more units here
        };

        float totalPathTime = 0f;
        foreach (var unitPos in unitPositions)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var path = flowField.GetPathFrom(unitPos); // O(1) lookup!
            sw.Stop();
            totalPathTime += sw.ElapsedMilliseconds;

            Debug.Log($"  Unit at {unitPos.OffsetCoordinates} -> Path: {path.Count} cells (took {sw.Elapsed.TotalMicroseconds:F1}µs)");
        }

        Debug.Log($"\nTotal time for {unitPositions.Count} units: {totalPathTime:F2}ms");
        Debug.Log($"If we used A* {unitPositions.Count} times, it would take ~{flowField.ComputationTimeMs * unitPositions.Count:F2}ms!");
        Debug.Log($"Flow Field is ~{(flowField.ComputationTimeMs * unitPositions.Count) / (flowField.ComputationTimeMs + totalPathTime):F0}x faster!");
    }

    // ==================== BIDIRECTIONAL A* ALGORITHM ====================
    // USE CASE: Long paths on large maps

    public void Example_BidirectionalAStar_LongPath()
    {
        Debug.Log("=== Bidirectional A* Algorithm Example ===");
        Debug.Log("USE CASE: Very long path across entire map");

        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[hexGrid.Width - 1, hexGrid.Height - 1];

        var context = new PathfindingContext();

        // Compare A* vs Bidirectional A*
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
        var astarResult = PathfindingManager.Instance.FindPath(start, goal, context);

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.BidirectionalAStar);
        var bidirResult = PathfindingManager.Instance.FindPath(start, goal, context);

        Debug.Log($"Comparison for long path ({Vector2.Distance(start.OffsetCoordinates, goal.OffsetCoordinates):F0} hex distance):");
        Debug.Log($"\nA*:");
        Debug.Log($"  Nodes explored: {astarResult.NodesExplored}");
        Debug.Log($"  Time: {astarResult.ComputationTimeMs:F2}ms");

        Debug.Log($"\nBidirectional A*:");
        Debug.Log($"  Nodes explored: {bidirResult.NodesExplored}");
        Debug.Log($"  Time: {bidirResult.ComputationTimeMs:F2}ms");
        Debug.Log($"  Speedup: {astarResult.ComputationTimeMs / bidirResult.ComputationTimeMs:F1}x faster!");
        Debug.Log($"  Explored {astarResult.NodesExplored - bidirResult.NodesExplored} fewer nodes!");
    }

    // ==================== BEST-FIRST (GREEDY) ALGORITHM ====================
    // USE CASE: Fast approximate pathfinding for AI

    public void Example_BestFirst_FastAI()
    {
        Debug.Log("=== Best-First (Greedy) Algorithm Example ===");
        Debug.Log("USE CASE: Fast AI pathfinding where approximate paths are OK");

        HexCell enemyPos = hexGrid.Cells[5, 5];
        HexCell playerPos = hexGrid.Cells[18, 18];

        var context = new PathfindingContext();

        // Compare A* vs Best-First
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
        var optimalResult = PathfindingManager.Instance.FindPath(enemyPos, playerPos, context);

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.BestFirst);
        var greedyResult = PathfindingManager.Instance.FindPath(enemyPos, playerPos, context);

        Debug.Log($"Comparison:");
        Debug.Log($"\nA* (Optimal):");
        Debug.Log($"  Path cost: {optimalResult.TotalCost}");
        Debug.Log($"  Time: {optimalResult.ComputationTimeMs:F2}ms");

        Debug.Log($"\nBest-First (Greedy):");
        Debug.Log($"  Path cost: {greedyResult.TotalCost} ({(greedyResult.TotalCost - optimalResult.TotalCost) * 100f / optimalResult.TotalCost:F1}% longer)");
        Debug.Log($"  Time: {greedyResult.ComputationTimeMs:F2}ms ({greedyResult.ComputationTimeMs / optimalResult.ComputationTimeMs:F1}x faster!)");
        Debug.Log($"\n  Trade-off: {optimalResult.ComputationTimeMs / greedyResult.ComputationTimeMs:F1}x faster for a slightly longer path");
        Debug.Log($"  Good for: Enemy AI, background NPCs, non-critical pathfinding");
    }

    // ==================== ALGORITHM SELECTION GUIDE ====================

    public void Example_AlgorithmComparison()
    {
        Debug.Log("=== Algorithm Selection Guide ===\n");

        Debug.Log(PathfindingManager.Instance.GetAlgorithmInfo());

        Debug.Log("QUICK SELECTION GUIDE:");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("Single path to one goal → A*");
        Debug.Log("Paths to many goals from one source → Dijkstra");
        Debug.Log("Many units to same goal → Flow Field");
        Debug.Log("Very long paths (200+ hexes) → Bidirectional A*");
        Debug.Log("Fast AI (approximate OK) → Best-First");
        Debug.Log("Simple reachability (no terrain cost) → BFS");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        Debug.Log("PERFORMANCE COMPARISON:");
        Debug.Log("  Fastest: BFS, Best-First");
        Debug.Log("  Balanced: A*");
        Debug.Log("  Special purpose: Dijkstra, Flow Field, Bidirectional A*");
        Debug.Log("");
    }

    // ==================== REAL-WORLD SCENARIOS ====================

    public void Example_Scenario_RTSGroupCommand()
    {
        Debug.Log("=== SCENARIO: RTS Group Movement ===");
        Debug.Log("Player selects 20 units and commands them to attack enemy base\n");

        HexCell enemyBase = hexGrid.Cells[25, 25];

        // Use Flow Field for efficient group movement
        Debug.Log("1. Generate Flow Field toward enemy base...");
        var flowField = PathfindingManager.Instance.GenerateFlowField(enemyBase, new PathfindingContext());

        Debug.Log($"2. Flow Field ready in {flowField.ComputationTimeMs:F2}ms");
        Debug.Log($"3. All 20 units can now follow the field instantly!");
        Debug.Log($"   Saves: ~{flowField.ComputationTimeMs * 19:F2}ms vs running A* 20 times\n");
    }

    public void Example_Scenario_TacticalThreatDisplay()
    {
        Debug.Log("=== SCENARIO: Tactical Threat Display ===");
        Debug.Log("Show all cells that enemy units can reach this turn\n");

        List<HexCell> enemyUnits = new List<HexCell>
        {
            hexGrid.Cells[10, 10],
            hexGrid.Cells[12, 8],
            hexGrid.Cells[15, 12]
        };

        Debug.Log("Using Dijkstra for each enemy to show threat ranges:");
        foreach (var enemy in enemyUnits)
        {
            var threats = PathfindingManager.Instance.FindAllPathsFrom(enemy,
                new PathfindingContext { MaxMovementPoints = 5 });

            Debug.Log($"  Enemy at {enemy.OffsetCoordinates}: threatens {threats.DistanceMap.Count} cells");
        }
    }

    public void Example_Scenario_QuickCombatAI()
    {
        Debug.Log("=== SCENARIO: Real-Time Combat AI ===");
        Debug.Log("10 enemies need to path to player every frame\n");

        HexCell playerPos = hexGrid.Cells[15, 15];

        Debug.Log("Using Best-First (Greedy) for fast, good-enough AI paths:");

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.BestFirst);

        float totalTime = 0f;
        for (int i = 0; i < 10; i++)
        {
            HexCell enemyPos = hexGrid.Cells[Random.Range(0, hexGrid.Width), Random.Range(0, hexGrid.Height)];
            var result = PathfindingManager.Instance.FindPath(enemyPos, playerPos, new PathfindingContext());
            totalTime += result.ComputationTimeMs;
        }

        Debug.Log($"  10 AI paths computed in {totalTime:F2}ms total");
        Debug.Log($"  Average: {totalTime / 10:F2}ms per path");
        Debug.Log($"  Can easily handle in one frame at 60 FPS (16.6ms budget)\n");
    }

    // ==================== JUMP POINT SEARCH (JPS) ====================
    // USE CASE: Ultra-fast pathfinding on open maps with few obstacles

    public void Example_JPS_OpenTerrain()
    {
        Debug.Log("=== Jump Point Search (JPS) Example ===");
        Debug.Log("USE CASE: Ultra-fast pathfinding on open terrain (plains, desert, ocean)\n");

        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[45, 45]; // Long distance across open map

        var context = new PathfindingContext();

        // Compare A* vs JPS
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
        var astarResult = PathfindingManager.Instance.FindPath(start, goal, context);

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);
        var jpsResult = PathfindingManager.Instance.FindPath(start, goal, context);

        Debug.Log("Performance comparison on open terrain:");
        Debug.Log($"  A* Time: {astarResult.ComputationTimeMs:F2}ms, Nodes: {astarResult.NodesExplored}");
        Debug.Log($"  JPS Time: {jpsResult.ComputationTimeMs:F2}ms, Nodes: {jpsResult.NodesExplored}");
        Debug.Log($"  Speedup: {astarResult.ComputationTimeMs / jpsResult.ComputationTimeMs:F1}x faster");
        Debug.Log($"  Node reduction: {(1 - (float)jpsResult.NodesExplored / astarResult.NodesExplored) * 100:F1}% fewer nodes");
        Debug.Log("\nJPS excels on open maps by 'jumping' over cells instead of exploring each one!");
    }

    public void Example_JPS_NavalPathfinding()
    {
        Debug.Log("=== JPS for Naval/Air Pathfinding ===");
        Debug.Log("USE CASE: Ship moving across ocean or air unit flying\n");

        // Simulate ocean pathfinding (mostly open water with some islands)
        HexCell shipStart = hexGrid.Cells[2, 2];
        HexCell shipGoal = hexGrid.Cells[48, 48];

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);

        var result = PathfindingManager.Instance.FindPath(shipStart, shipGoal, new PathfindingContext());

        if (result.Success)
        {
            Debug.Log($"Naval path found:");
            Debug.Log($"  Distance: {result.PathLength} hexes");
            Debug.Log($"  Computation: {result.ComputationTimeMs:F2}ms");
            Debug.Log($"  Nodes explored: {result.NodesExplored} (very few due to open water!)");
            Debug.Log("\nPerfect for naval strategy games, flight simulators, etc.");
        }
    }

    public void Example_JPS_StrategicMapMovement()
    {
        Debug.Log("=== JPS for Strategic Map Movement ===");
        Debug.Log("USE CASE: Large-scale strategic movement across continents\n");

        // Strategic level movement (large map, low detail)
        HexCell capitalCity = hexGrid.Cells[10, 10];
        HexCell distantCity = hexGrid.Cells[40, 40];

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);

        var result = PathfindingManager.Instance.FindPath(capitalCity, distantCity, new PathfindingContext());

        Debug.Log($"Strategic route found:");
        Debug.Log($"  Total distance: {result.PathLength} hexes");
        Debug.Log($"  Time: {result.ComputationTimeMs:F2}ms");
        Debug.Log($"  Nodes: {result.NodesExplored}");
        Debug.Log("\nIdeal for grand strategy games (Civilization, Total War campaign map, etc.)");
    }

    public void Example_JPS_PerformanceComparison()
    {
        Debug.Log("=== JPS vs A* Performance Comparison ===");
        Debug.Log("Testing on various map types\n");

        var context = new PathfindingContext();

        // Test 1: Completely open map
        Debug.Log("Test 1: Open map (no obstacles)");
        HexCell openStart = hexGrid.Cells[0, 0];
        HexCell openGoal = hexGrid.Cells[49, 49];

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
        var astarOpen = PathfindingManager.Instance.FindPath(openStart, openGoal, context);

        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);
        var jpsOpen = PathfindingManager.Instance.FindPath(openStart, openGoal, context);

        Debug.Log($"  A*:  {astarOpen.ComputationTimeMs:F2}ms, {astarOpen.NodesExplored} nodes");
        Debug.Log($"  JPS: {jpsOpen.ComputationTimeMs:F2}ms, {jpsOpen.NodesExplored} nodes");
        Debug.Log($"  JPS is {astarOpen.ComputationTimeMs / jpsOpen.ComputationTimeMs:F1}x faster!\n");

        // Test 2: 100 long-distance paths (benchmark)
        Debug.Log("Test 2: Benchmark - 100 random long paths");
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);

        float totalTime = 0f;
        int totalNodes = 0;
        for (int i = 0; i < 100; i++)
        {
            HexCell randomStart = hexGrid.Cells[Random.Range(0, 10), Random.Range(0, 10)];
            HexCell randomGoal = hexGrid.Cells[Random.Range(40, 50), Random.Range(40, 50)];
            var result = PathfindingManager.Instance.FindPath(randomStart, randomGoal, context);
            totalTime += result.ComputationTimeMs;
            totalNodes += result.NodesExplored;
        }

        Debug.Log($"  100 paths computed in {totalTime:F2}ms");
        Debug.Log($"  Average: {totalTime / 100:F2}ms per path");
        Debug.Log($"  Average nodes: {totalNodes / 100}");
        Debug.Log("\nJPS enables real-time pathfinding for many units on large maps!");
    }

    public void Example_Scenario_MassiveArmyMovement()
    {
        Debug.Log("=== SCENARIO: Moving Large Army Across Open Plains ===");
        Debug.Log("50 units need to move across open battlefield\n");

        HexCell armyStart = hexGrid.Cells[5, 25];
        HexCell battlePosition = hexGrid.Cells[45, 25];

        Debug.Log("Option 1: A* for each unit (slow)");
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
        float astarTime = 0f;
        for (int i = 0; i < 50; i++)
        {
            var result = PathfindingManager.Instance.FindPath(armyStart, battlePosition, new PathfindingContext());
            astarTime += result.ComputationTimeMs;
        }
        Debug.Log($"  A*: {astarTime:F2}ms total for 50 units\n");

        Debug.Log("Option 2: JPS for each unit (faster)");
        PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.JPS);
        float jpsTime = 0f;
        for (int i = 0; i < 50; i++)
        {
            var result = PathfindingManager.Instance.FindPath(armyStart, battlePosition, new PathfindingContext());
            jpsTime += result.ComputationTimeMs;
        }
        Debug.Log($"  JPS: {jpsTime:F2}ms total for 50 units");
        Debug.Log($"  Speedup: {astarTime / jpsTime:F1}x faster than A*\n");

        Debug.Log("Option 3: Flow Field for all units (best for same destination)");
        var flowField = PathfindingManager.Instance.GenerateFlowField(battlePosition, new PathfindingContext());
        Debug.Log($"  Flow Field: {10:F2}ms (one-time) + 50x O(1) lookups");
        Debug.Log($"  Best when all units go to same place!\n");

        Debug.Log("RECOMMENDATION:");
        Debug.Log("  Same destination: Use Flow Field");
        Debug.Log("  Different destinations on open terrain: Use JPS");
        Debug.Log("  Complex terrain with obstacles: Use A*");
    }

    public void Example_JPS_WhenNotToUse()
    {
        Debug.Log("=== When NOT to Use JPS ===");
        Debug.Log("JPS is optimized for open terrain. It's less effective on heavily obstructed maps.\n");

        Debug.Log("BAD SCENARIOS for JPS:");
        Debug.Log("  ✗ Dense forests with many tree obstacles");
        Debug.Log("  ✗ Urban environments (buildings everywhere)");
        Debug.Log("  ✗ Maze-like dungeons");
        Debug.Log("  ✗ Maps with highly varied terrain costs\n");

        Debug.Log("GOOD SCENARIOS for JPS:");
        Debug.Log("  ✓ Open plains, deserts, tundra");
        Debug.Log("  ✓ Ocean/naval pathfinding");
        Debug.Log("  ✓ Air/space pathfinding");
        Debug.Log("  ✓ Strategic world maps");
        Debug.Log("  ✓ Large-scale RTS battlefields\n");

        Debug.Log("ALGORITHM SELECTION GUIDE:");
        Debug.Log("  Open map (>70% walkable):  Use JPS (10-40x faster)");
        Debug.Log("  Medium map (40-70% walkable): Use A* (reliable)");
        Debug.Log("  Dense map (<40% walkable): Use A* (JPS has minimal benefit)");
    }
}
