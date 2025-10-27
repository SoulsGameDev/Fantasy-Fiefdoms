using UnityEngine;
using UnityEditor;
using Pathfinding.Core;
using Pathfinding.Algorithms;
using System.Collections.Generic;

[CustomEditor(typeof(PathfindingManager))]
public class PathfindingManagerEditor : Editor
{
    private bool showAlgorithmInfo = true;
    private bool showPerformanceStats = true;
    private bool showCacheManagement = true;
    private bool showTestingTools = false;

    // Algorithm descriptions for the UI
    private static readonly Dictionary<PathfindingManager.AlgorithmType, string> algorithmDescriptions =
        new Dictionary<PathfindingManager.AlgorithmType, string>
        {
            { PathfindingManager.AlgorithmType.AStar, "Optimal pathfinding with heuristic guidance. Best for single-source, single-target paths. Balanced speed and optimality." },
            { PathfindingManager.AlgorithmType.Dijkstra, "Finds ALL shortest paths from source. Best when you need paths to multiple destinations from same start." },
            { PathfindingManager.AlgorithmType.BFS, "Fast unweighted pathfinding. Ignores terrain costs. Best for quick range checks and simple distance calculations." },
            { PathfindingManager.AlgorithmType.BestFirst, "Fast greedy pathfinding (NON-OPTIMAL). Rushes toward goal. Use when speed is critical and approximate paths are acceptable." },
            { PathfindingManager.AlgorithmType.BidirectionalAStar, "Searches from both start and goal simultaneously. ~2x faster for long paths. Best for distant goals." },
            { PathfindingManager.AlgorithmType.FlowField, "Calculate once, use for many units. O(1) path lookup per unit. Ideal for group movement to same destination." },
            { PathfindingManager.AlgorithmType.JPS, "Ultra-fast for open maps (10-40x faster than A*). Jumps over cells in straight lines. Best for maps with large open areas." }
        };

    private static readonly Dictionary<PathfindingManager.AlgorithmType, string> algorithmUseCases =
        new Dictionary<PathfindingManager.AlgorithmType, string>
        {
            { PathfindingManager.AlgorithmType.AStar, "General purpose, player units, AI movement" },
            { PathfindingManager.AlgorithmType.Dijkstra, "Multiple destinations, influence maps, range displays" },
            { PathfindingManager.AlgorithmType.BFS, "Simple range checks, exploration radius, ability ranges" },
            { PathfindingManager.AlgorithmType.BestFirst, "Non-critical AI, decorative units, ambient creatures" },
            { PathfindingManager.AlgorithmType.BidirectionalAStar, "Long-distance travel, strategic map movement" },
            { PathfindingManager.AlgorithmType.FlowField, "RTS unit groups, army movement, swarm AI" },
            { PathfindingManager.AlgorithmType.JPS, "Open battlefields, terrain with consistent costs, speed-critical paths" }
        };

    private SerializedProperty defaultAlgorithmProp;
    private SerializedProperty enableCachingProp;
    private SerializedProperty cacheDurationProp;
    private SerializedProperty maxCacheSizeProp;
    private SerializedProperty logPerformanceProp;

    private void OnEnable()
    {
        defaultAlgorithmProp = serializedObject.FindProperty("defaultAlgorithm");
        enableCachingProp = serializedObject.FindProperty("enableCaching");
        cacheDurationProp = serializedObject.FindProperty("cacheDuration");
        maxCacheSizeProp = serializedObject.FindProperty("maxCacheSize");
        logPerformanceProp = serializedObject.FindProperty("logPerformance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PathfindingManager manager = (PathfindingManager)target;

        EditorGUILayout.Space(10);
        DrawTitle("Pathfinding Manager", "Advanced pathfinding system with 7 algorithms");

        EditorGUILayout.Space(5);

        // Algorithm Selection Section
        DrawAlgorithmSection(manager);

        EditorGUILayout.Space(10);

        // Cache Management Section
        DrawCacheSection(manager);

        EditorGUILayout.Space(10);

        // Performance Monitoring Section
        DrawPerformanceSection(manager);

        EditorGUILayout.Space(10);

        // Testing Tools Section
        DrawTestingSection(manager);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTitle(string title, string subtitle)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField(title, titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField(subtitle, subtitleStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawAlgorithmSection(PathfindingManager manager)
    {
        showAlgorithmInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showAlgorithmInfo, "Algorithm Selection");

        if (showAlgorithmInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Current algorithm display
            EditorGUILayout.LabelField("Current Algorithm", EditorStyles.boldLabel);
            if (Application.isPlaying && manager.CurrentAlgorithm != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Active:", manager.CurrentAlgorithm.AlgorithmName);
                EditorGUILayout.LabelField("Threading:", manager.CurrentAlgorithm.SupportsThreading ? "Yes" : "No");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(manager.CurrentAlgorithm.Description, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Algorithm info available during Play mode", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            // Algorithm selector
            EditorGUILayout.PropertyField(defaultAlgorithmProp, new GUIContent("Default Algorithm"));

            var selectedAlgorithm = (PathfindingManager.AlgorithmType)defaultAlgorithmProp.enumValueIndex;

            // Show description for selected algorithm
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(algorithmDescriptions[selectedAlgorithm], MessageType.None);

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Best Use Cases:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(algorithmUseCases[selectedAlgorithm], MessageType.None);

            // Runtime algorithm switching
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Switch Algorithm"))
                {
                    manager.SetAlgorithm(selectedAlgorithm);
                    Debug.Log($"Switched to {selectedAlgorithm} algorithm");
                }

                if (GUILayout.Button("Show All Algorithms Info"))
                {
                    Debug.Log(manager.GetAlgorithmInfo());
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);

            // Quick reference table
            if (GUILayout.Button("Show Algorithm Comparison Table"))
            {
                ShowAlgorithmComparisonWindow();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawCacheSection(PathfindingManager manager)
    {
        showCacheManagement = EditorGUILayout.BeginFoldoutHeaderGroup(showCacheManagement, "Cache Management");

        if (showCacheManagement)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(enableCachingProp, new GUIContent("Enable Caching"));

            if (enableCachingProp.boolValue)
            {
                EditorGUILayout.PropertyField(cacheDurationProp, new GUIContent("Cache Duration (seconds)"));
                EditorGUILayout.PropertyField(maxCacheSizeProp, new GUIContent("Max Cache Size"));

                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Cached paths remain valid for the specified duration. When cache is full, all entries are cleared.", MessageType.Info);

                if (Application.isPlaying)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Cache Controls", EditorStyles.boldLabel);

                    if (GUILayout.Button("Clear Cache Now"))
                    {
                        manager.ClearCache();
                        Debug.Log("Pathfinding cache cleared");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Caching is disabled. All paths will be computed fresh.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPerformanceSection(PathfindingManager manager)
    {
        showPerformanceStats = EditorGUILayout.BeginFoldoutHeaderGroup(showPerformanceStats, "Performance Monitoring");

        if (showPerformanceStats)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(logPerformanceProp, new GUIContent("Log Performance"));

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

                // Display current statistics
                string stats = manager.GetStatistics();
                EditorGUILayout.HelpBox(stats, MessageType.Info);

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Reset Statistics"))
                {
                    manager.ResetStatistics();
                    Debug.Log("Performance statistics reset");
                }

                if (GUILayout.Button("Log to Console"))
                {
                    Debug.Log(manager.GetStatistics());
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Performance statistics available during Play mode", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTestingSection(PathfindingManager manager)
    {
        showTestingTools = EditorGUILayout.BeginFoldoutHeaderGroup(showTestingTools, "Testing Tools (Play Mode Only)");

        if (showTestingTools)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Select two cells in the scene, then use the buttons below to test pathfinding.", MessageType.Info);

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Clear All Reachability Visualization"))
                {
                    HexGrid grid = FindObjectOfType<HexGrid>();
                    if (grid != null)
                    {
                        manager.ClearReachability(grid);
                        Debug.Log("Cleared reachability visualization");
                    }
                    else
                    {
                        Debug.LogWarning("No HexGrid found in scene");
                    }
                }

                if (GUILayout.Button("Clear All Path Visualization"))
                {
                    HexGrid grid = FindObjectOfType<HexGrid>();
                    if (grid != null)
                    {
                        manager.ClearPaths(grid);
                        Debug.Log("Cleared path visualization");
                    }
                    else
                    {
                        Debug.LogWarning("No HexGrid found in scene");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Testing tools only available during Play mode", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ShowAlgorithmComparisonWindow()
    {
        AlgorithmComparisonWindow.ShowWindow();
    }
}

// Separate window for algorithm comparison
public class AlgorithmComparisonWindow : EditorWindow
{
    private Vector2 scrollPosition;

    public static void ShowWindow()
    {
        AlgorithmComparisonWindow window = GetWindow<AlgorithmComparisonWindow>("Algorithm Comparison");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Pathfinding Algorithm Comparison", titleStyle);
        EditorGUILayout.Space(10);

        DrawAlgorithmComparison("A* (A-Star)",
            "Optimal: Yes | Threading: Yes | Speed: Medium-Fast",
            "The gold standard for pathfinding. Uses heuristic to guide search toward goal. Guarantees shortest path.",
            "Player units, AI pathfinding, tactical movement, general-purpose paths",
            Color.green);

        DrawAlgorithmComparison("Dijkstra",
            "Optimal: Yes | Threading: Yes | Speed: Medium",
            "Finds ALL shortest paths from a source. No heuristic - explores uniformly. Returns complete distance map.",
            "Influence maps, multiple destinations, range displays, area-of-effect calculations",
            Color.cyan);

        DrawAlgorithmComparison("BFS (Breadth-First Search)",
            "Optimal: For unweighted | Threading: Yes | Speed: Fast",
            "Ignores terrain costs - treats all moves as equal. Very fast for simple distance checks.",
            "Ability ranges, explosion radius, movement within X steps, flood-fill operations",
            Color.blue);

        DrawAlgorithmComparison("Best-First Search",
            "Optimal: NO | Threading: Yes | Speed: Very Fast",
            "Greedy algorithm that rushes toward goal. Does NOT guarantee shortest path. Fast but approximate.",
            "Ambient AI, decorative units, when speed > optimality, placeholder movement",
            Color.yellow);

        DrawAlgorithmComparison("Bidirectional A*",
            "Optimal: Yes | Threading: Yes | Speed: Fast (for long paths)",
            "Searches from both start and goal simultaneously. Meets in the middle. ~2x speedup for distant goals.",
            "Long-distance travel, strategic map movement, cross-map pathfinding",
            new Color(0.5f, 1f, 0.5f));

        DrawAlgorithmComparison("Flow Field",
            "Optimal: Yes | Threading: Yes | Speed: Very Fast (amortized)",
            "Pre-computes direction field to goal. Every unit looks up direction instantly. Ideal for many units.",
            "RTS games, army movement, swarms, many units moving to same target",
            new Color(1f, 0.5f, 0f));

        DrawAlgorithmComparison("JPS (Jump Point Search)",
            "Optimal: Yes | Threading: Yes | Speed: Ultra-Fast (10-40x)",
            "Optimized A* that jumps over cells in straight lines. Extremely fast on open maps with uniform terrain.",
            "Open battlefields, consistent terrain costs, when maximum speed is required",
            new Color(1f, 0f, 1f));

        EditorGUILayout.Space(20);

        EditorGUILayout.EndScrollView();
    }

    private void DrawAlgorithmComparison(string name, string specs, string description, string useCases, Color color)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Color indicator
        Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(5));
        EditorGUI.DrawRect(colorRect, color);

        EditorGUILayout.Space(3);

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 12;
        EditorGUILayout.LabelField(name, nameStyle);

        EditorGUILayout.LabelField(specs, EditorStyles.miniLabel);

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Best For:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(useCases, EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}
