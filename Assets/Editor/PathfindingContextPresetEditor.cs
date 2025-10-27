using UnityEngine;
using UnityEditor;
using Pathfinding.Core;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(PathfindingContextPreset))]
public class PathfindingContextPresetEditor : Editor
{
    private SerializedProperty presetNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty maxMovementPointsProp;
    private SerializedProperty maxSearchNodesProp;
    private SerializedProperty allowMoveThroughAlliesProp;
    private SerializedProperty allowMoveThroughEnemiesProp;
    private SerializedProperty requireExploredProp;
    private SerializedProperty allowDiagonalMovementProp;
    private SerializedProperty preferHighGroundProp;
    private SerializedProperty avoidEnemyZonesProp;
    private SerializedProperty terrainCostMultipliersProp;
    private SerializedProperty storeDiagnosticDataProp;
    private SerializedProperty useCachingProp;

    private bool showMovementSettings = true;
    private bool showTraversalRules = true;
    private bool showStrategicPreferences = true;
    private bool showTerrainCosts = true;
    private bool showPerformance = true;

    private void OnEnable()
    {
        presetNameProp = serializedObject.FindProperty("presetName");
        descriptionProp = serializedObject.FindProperty("description");
        maxMovementPointsProp = serializedObject.FindProperty("maxMovementPoints");
        maxSearchNodesProp = serializedObject.FindProperty("maxSearchNodes");
        allowMoveThroughAlliesProp = serializedObject.FindProperty("allowMoveThroughAllies");
        allowMoveThroughEnemiesProp = serializedObject.FindProperty("allowMoveThroughEnemies");
        requireExploredProp = serializedObject.FindProperty("requireExplored");
        allowDiagonalMovementProp = serializedObject.FindProperty("allowDiagonalMovement");
        preferHighGroundProp = serializedObject.FindProperty("preferHighGround");
        avoidEnemyZonesProp = serializedObject.FindProperty("avoidEnemyZones");
        terrainCostMultipliersProp = serializedObject.FindProperty("terrainCostMultipliers");
        storeDiagnosticDataProp = serializedObject.FindProperty("storeDiagnosticData");
        useCachingProp = serializedObject.FindProperty("useCaching");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PathfindingContextPreset preset = (PathfindingContextPreset)target;

        DrawTitle();
        EditorGUILayout.Space(10);

        DrawPresetInfo();
        EditorGUILayout.Space(10);

        DrawMovementSettings();
        EditorGUILayout.Space(10);

        DrawTraversalRules();
        EditorGUILayout.Space(10);

        DrawStrategicPreferences();
        EditorGUILayout.Space(10);

        DrawTerrainCosts();
        EditorGUILayout.Space(10);

        DrawPerformanceSettings();
        EditorGUILayout.Space(10);

        DrawQuickActions(preset);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTitle()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Pathfinding Context Preset", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Reusable pathfinding configuration for different unit types", subtitleStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawPresetInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Preset Information", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(presetNameProp, new GUIContent("Preset Name"));
        EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description"));

        EditorGUILayout.EndVertical();
    }

    private void DrawMovementSettings()
    {
        showMovementSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSettings, "Movement Settings");

        if (showMovementSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(maxMovementPointsProp, new GUIContent("Max Movement Points"));
            if (maxMovementPointsProp.intValue < 0)
            {
                EditorGUILayout.HelpBox("Unlimited movement (no cost restriction)", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Unit can move up to {maxMovementPointsProp.intValue} movement points", MessageType.None);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(maxSearchNodesProp, new GUIContent("Max Search Nodes"));
            EditorGUILayout.HelpBox("Prevents infinite loops. Higher = more thorough but slower. Typical: 10,000", MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTraversalRules()
    {
        showTraversalRules = EditorGUILayout.BeginFoldoutHeaderGroup(showTraversalRules, "Traversal Rules");

        if (showTraversalRules)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawToggleWithDescription(
                allowMoveThroughAlliesProp,
                "Allow Move Through Allies",
                "Units can pass through cells occupied by allies",
                "Useful for: Cavalry, Flying units, Support units"
            );

            EditorGUILayout.Space(3);

            DrawToggleWithDescription(
                allowMoveThroughEnemiesProp,
                "Allow Move Through Enemies",
                "Units can pass through cells occupied by enemies",
                "Useful for: Flying units, Ghost units, Special abilities"
            );

            EditorGUILayout.Space(3);

            DrawToggleWithDescription(
                requireExploredProp,
                "Require Explored",
                "Only consider cells not hidden by fog of war",
                "Usually enabled except for flying/scouting units"
            );

            EditorGUILayout.Space(3);

            DrawToggleWithDescription(
                allowDiagonalMovementProp,
                "Allow Diagonal Movement",
                "Allow diagonal movement (future feature)",
                "Standard hex grids have 6 neighbors"
            );

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawStrategicPreferences()
    {
        showStrategicPreferences = EditorGUILayout.BeginFoldoutHeaderGroup(showStrategicPreferences, "Strategic Preferences");

        if (showStrategicPreferences)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawToggleWithDescription(
                preferHighGroundProp,
                "Prefer High Ground",
                "Units prefer paths through higher terrain for tactical advantage",
                "Useful for: Combat units, Archers, Strategic movement"
            );

            EditorGUILayout.Space(3);

            DrawToggleWithDescription(
                avoidEnemyZonesProp,
                "Avoid Enemy Zones",
                "Units avoid cells adjacent to enemies when possible",
                "Useful for: Cautious movement, Retreating, Support units"
            );

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTerrainCosts()
    {
        showTerrainCosts = EditorGUILayout.BeginFoldoutHeaderGroup(showTerrainCosts, "Terrain Cost Multipliers");

        if (showTerrainCosts)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.HelpBox("Customize movement costs for specific terrain types. 1.0 = normal, 2.0 = double cost, 0.5 = half cost", MessageType.Info);

            EditorGUILayout.Space(5);

            // Draw terrain cost multipliers list
            EditorGUILayout.PropertyField(terrainCostMultipliersProp, new GUIContent("Terrain Multipliers"), true);

            EditorGUILayout.Space(5);

            // Quick add buttons for common terrains
            EditorGUILayout.LabelField("Quick Add:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Forest"))
                AddTerrainMultiplier("Forest", 1.5f);
            if (GUILayout.Button("Mountains"))
                AddTerrainMultiplier("Mountains", 2.0f);
            if (GUILayout.Button("Grassland"))
                AddTerrainMultiplier("Grassland", 1.0f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Ocean"))
                AddTerrainMultiplier("Ocean", 10.0f);
            if (GUILayout.Button("Beach"))
                AddTerrainMultiplier("Beach", 1.2f);
            if (GUILayout.Button("Ice"))
                AddTerrainMultiplier("Ice", 1.3f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPerformanceSettings()
    {
        showPerformance = EditorGUILayout.BeginFoldoutHeaderGroup(showPerformance, "Performance Settings");

        if (showPerformance)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawToggleWithDescription(
                storeDiagnosticDataProp,
                "Store Diagnostic Data",
                "Store cost maps and debug information (disable for production)",
                "Useful for debugging and visualization"
            );

            EditorGUILayout.Space(3);

            DrawToggleWithDescription(
                useCachingProp,
                "Use Caching",
                "Use cached pathfinding results when available",
                "Improves performance for repeated queries"
            );

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawQuickActions(PathfindingContextPreset preset)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Load Template:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Infantry"))
            LoadTemplate(PathfindingContextPreset.CreateInfantryPreset());

        if (GUILayout.Button("Cavalry"))
            LoadTemplate(PathfindingContextPreset.CreateCavalryPreset());

        if (GUILayout.Button("Flying"))
            LoadTemplate(PathfindingContextPreset.CreateFlyingPreset());

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Tactical Combat"))
            LoadTemplate(PathfindingContextPreset.CreateTacticalCombatPreset());

        if (GUILayout.Button("Reset to Default"))
            LoadTemplate(CreateInstance<PathfindingContextPreset>());

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Test in Play Mode"))
        {
            if (Application.isPlaying)
            {
                TestPreset(preset);
            }
            else
            {
                EditorUtility.DisplayDialog("Play Mode Required", "Enter Play mode to test this preset with the pathfinding system.", "OK");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawToggleWithDescription(SerializedProperty prop, string label, string description, string useCase)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.PropertyField(prop, new GUIContent(label));

        GUIStyle descStyle = new GUIStyle(EditorStyles.miniLabel);
        descStyle.wordWrap = true;
        EditorGUILayout.LabelField(description, descStyle);

        if (prop.boolValue)
        {
            GUIStyle useCaseStyle = new GUIStyle(EditorStyles.miniLabel);
            useCaseStyle.wordWrap = true;
            useCaseStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            EditorGUILayout.LabelField($"✓ {useCase}", useCaseStyle);
        }

        EditorGUILayout.EndVertical();
    }

    private void AddTerrainMultiplier(string terrainName, float defaultMultiplier)
    {
        int index = terrainCostMultipliersProp.arraySize;
        terrainCostMultipliersProp.InsertArrayElementAtIndex(index);

        SerializedProperty newElement = terrainCostMultipliersProp.GetArrayElementAtIndex(index);
        SerializedProperty terrainNameProp = newElement.FindPropertyRelative("terrainName");
        SerializedProperty costMultiplierProp = newElement.FindPropertyRelative("costMultiplier");

        terrainNameProp.stringValue = terrainName;
        costMultiplierProp.floatValue = defaultMultiplier;

        serializedObject.ApplyModifiedProperties();
    }

    private void LoadTemplate(PathfindingContextPreset template)
    {
        if (EditorUtility.DisplayDialog(
            "Load Template",
            $"Load template '{template.presetName}'? This will overwrite current settings.",
            "Load",
            "Cancel"))
        {
            Undo.RecordObject(target, "Load Template");

            var context = template.CreateContext();
            ((PathfindingContextPreset)target).ApplyFromContext(context);

            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }

        DestroyImmediate(template);
    }

    private void TestPreset(PathfindingContextPreset preset)
    {
        var context = preset.CreateContext();

        Debug.Log($"Testing Preset: {preset.presetName}");
        Debug.Log($"Max Movement: {context.MaxMovementPoints}");
        Debug.Log($"Max Search Nodes: {context.MaxSearchNodes}");
        Debug.Log($"Through Allies: {context.AllowMoveThroughAllies}");
        Debug.Log($"Through Enemies: {context.AllowMoveThroughEnemies}");
        Debug.Log($"Require Explored: {context.RequireExplored}");
        Debug.Log($"Prefer High Ground: {context.PreferHighGround}");
        Debug.Log($"Avoid Enemy Zones: {context.AvoidEnemyZones}");
        Debug.Log($"Terrain Multipliers: {context.TerrainCostMultipliers.Count}");

        EditorUtility.DisplayDialog(
            "Preset Test",
            $"Preset '{preset.presetName}' configuration logged to console. Use this preset with PathfindingManager.FindPath(start, goal, preset.CreateContext())",
            "OK"
        );
    }
}

/// <summary>
/// Preset creation wizard window
/// </summary>
public class PathfindingPresetWizard : EditorWindow
{
    private string presetName = "New Preset";
    private string description = "";
    private int selectedTemplate = 0;
    private string[] templateNames = new string[] { "Empty", "Infantry", "Cavalry", "Flying", "Tactical Combat" };

    [MenuItem("Assets/Create/Pathfinding/Preset Wizard")]
    public static void ShowWindow()
    {
        PathfindingPresetWizard window = GetWindow<PathfindingPresetWizard>("Preset Wizard");
        window.minSize = new Vector2(400, 250);
        window.Show();
    }

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Pathfinding Context Preset Wizard", titleStyle);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("Create a new pathfinding preset from a template", MessageType.Info);

        EditorGUILayout.Space(10);

        presetName = EditorGUILayout.TextField("Preset Name:", presetName);
        description = EditorGUILayout.TextField("Description:", description);

        EditorGUILayout.Space(10);

        selectedTemplate = EditorGUILayout.Popup("Start From Template:", selectedTemplate, templateNames);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Create Preset", GUILayout.Height(30)))
        {
            CreatePreset();
        }
    }

    private void CreatePreset()
    {
        PathfindingContextPreset newPreset;

        switch (selectedTemplate)
        {
            case 1:
                newPreset = PathfindingContextPreset.CreateInfantryPreset();
                break;
            case 2:
                newPreset = PathfindingContextPreset.CreateCavalryPreset();
                break;
            case 3:
                newPreset = PathfindingContextPreset.CreateFlyingPreset();
                break;
            case 4:
                newPreset = PathfindingContextPreset.CreateTacticalCombatPreset();
                break;
            default:
                newPreset = ScriptableObject.CreateInstance<PathfindingContextPreset>();
                break;
        }

        newPreset.presetName = presetName;
        if (!string.IsNullOrEmpty(description))
        {
            newPreset.description = description;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Pathfinding Preset",
            presetName,
            "asset",
            "Choose where to save the preset"
        );

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newPreset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = newPreset;
            EditorGUIUtility.PingObject(newPreset);

            Debug.Log($"Created pathfinding preset: {path}");
            Close();
        }
        else
        {
            DestroyImmediate(newPreset);
        }
    }
}
