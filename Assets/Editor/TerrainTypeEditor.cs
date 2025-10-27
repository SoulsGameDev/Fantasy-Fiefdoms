using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(TerrainType))]
public class TerrainTypeEditor : Editor
{
    private SerializedProperty terrainNameProp;
    private SerializedProperty nameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty colourProp;
    private SerializedProperty prefabProp;
    private SerializedProperty iconProp;
    private SerializedProperty movementCostProp;
    private SerializedProperty isWalkableProp;

    private bool showPreview = true;
    private bool showUsageStats = true;
    private PreviewRenderUtility previewUtility;
    private GameObject previewInstance;

    private void OnEnable()
    {
        terrainNameProp = serializedObject.FindProperty("<terrainName>k__BackingField");
        nameProp = serializedObject.FindProperty("<Name>k__BackingField");
        descriptionProp = serializedObject.FindProperty("<Description>k__BackingField");
        colourProp = serializedObject.FindProperty("<Colour>k__BackingField");
        prefabProp = serializedObject.FindProperty("<Prefab>k__BackingField");
        iconProp = serializedObject.FindProperty("<Icon>k__BackingField");
        movementCostProp = serializedObject.FindProperty("<movementCost>k__BackingField");
        isWalkableProp = serializedObject.FindProperty("<isWalkable>k__BackingField");
    }

    private void OnDisable()
    {
        if (previewUtility != null)
        {
            previewUtility.Cleanup();
            previewUtility = null;
        }

        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        TerrainType terrain = (TerrainType)target;

        DrawTitle();
        EditorGUILayout.Space(10);

        DrawBasicProperties();
        EditorGUILayout.Space(10);

        DrawVisualProperties();
        EditorGUILayout.Space(10);

        DrawPathfindingProperties();
        EditorGUILayout.Space(10);

        DrawPreviewSection(terrain);
        EditorGUILayout.Space(10);

        DrawUsageInformation(terrain);
        EditorGUILayout.Space(10);

        DrawQuickActions(terrain);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTitle()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Terrain Type Configuration", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Define terrain properties for hex grid gameplay", subtitleStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawBasicProperties()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Basic Properties", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(terrainNameProp, new GUIContent("Terrain Name"));
        EditorGUILayout.HelpBox("Used for pathfinding cost multiplier lookups. Should match preset terrain names.", MessageType.Info);

        EditorGUILayout.Space(3);

        EditorGUILayout.PropertyField(nameProp, new GUIContent("Display Name"));
        EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description"));

        EditorGUILayout.EndVertical();
    }

    private void DrawVisualProperties()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Visual Properties", EditorStyles.boldLabel);

        // Color field with preview
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(colourProp, new GUIContent("Color"));

        // Color preview swatch
        Rect colorRect = GUILayoutUtility.GetRect(50, 18);
        EditorGUI.DrawRect(colorRect, colourProp.colorValue);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        EditorGUILayout.PropertyField(prefabProp, new GUIContent("Prefab"));
        if (prefabProp.objectReferenceValue != null)
        {
            EditorGUILayout.HelpBox("✓ Prefab assigned - will be instantiated for each hex cell of this terrain type", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ No prefab assigned - cells will use default visualization", MessageType.Warning);
        }

        EditorGUILayout.Space(3);

        EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon"));
        if (iconProp.objectReferenceValue != null)
        {
            Sprite icon = iconProp.objectReferenceValue as Sprite;
            if (icon != null)
            {
                Rect iconRect = GUILayoutUtility.GetRect(64, 64);
                EditorGUI.DrawTextureTransparent(iconRect, icon.texture);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Optional: Add an icon for UI display", MessageType.None);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPathfindingProperties()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Pathfinding Properties", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(isWalkableProp, new GUIContent("Is Walkable"));

        if (!isWalkableProp.boolValue)
        {
            EditorGUILayout.HelpBox("This terrain is IMPASSABLE - units cannot pathfind through it", MessageType.Warning);
        }

        EditorGUILayout.Space(3);

        EditorGUILayout.PropertyField(movementCostProp, new GUIContent("Movement Cost"));

        // Movement cost visualization
        int cost = movementCostProp.intValue;
        string costDescription = GetMovementCostDescription(cost);
        MessageType messageType = GetMovementCostMessageType(cost);

        EditorGUILayout.HelpBox(costDescription, messageType);

        // Visual movement cost bar
        DrawMovementCostBar(cost);

        EditorGUILayout.Space(5);

        // Quick preset buttons
        EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Fast (1)"))
            movementCostProp.intValue = 1;

        if (GUILayout.Button("Normal (2)"))
            movementCostProp.intValue = 2;

        if (GUILayout.Button("Slow (3)"))
            movementCostProp.intValue = 3;

        if (GUILayout.Button("Very Slow (5)"))
            movementCostProp.intValue = 5;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Impassable (10)"))
        {
            movementCostProp.intValue = 10;
            isWalkableProp.boolValue = false;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawPreviewSection(TerrainType terrain)
    {
        showPreview = EditorGUILayout.BeginFoldoutHeaderGroup(showPreview, "Preview");

        if (showPreview)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (terrain.Prefab != null)
            {
                EditorGUILayout.LabelField("3D Prefab Preview", EditorStyles.boldLabel);

                Rect previewRect = GUILayoutUtility.GetRect(256, 256);
                DrawPrefabPreview(terrain.Prefab, previewRect);
            }
            else
            {
                EditorGUILayout.HelpBox("No prefab assigned for preview", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            // Color + Name preview
            EditorGUILayout.LabelField("Color Preview", EditorStyles.boldLabel);
            Rect colorPreviewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(40));
            EditorGUI.DrawRect(colorPreviewRect, terrain.Colour);

            GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.fontSize = 14;
            nameStyle.normal.textColor = Color.white;
            GUI.Label(colorPreviewRect, terrain.Name, nameStyle);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawUsageInformation(TerrainType terrain)
    {
        showUsageStats = EditorGUILayout.BeginFoldoutHeaderGroup(showUsageStats, "Usage Information");

        if (showUsageStats)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Scene Usage", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                // Find all hex cells using this terrain type
                HexCell[] allCells = FindObjectsOfType<HexCell>();
                int usageCount = allCells.Count(cell => cell.TerrainType == terrain);

                EditorGUILayout.LabelField("Cells using this terrain:", usageCount.ToString());

                if (usageCount > 0)
                {
                    float percentage = (float)usageCount / allCells.Length * 100f;
                    EditorGUILayout.LabelField("Percentage of map:", $"{percentage:F1}%");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Usage statistics available during Play mode", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Pathfinding Impact", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Movement Cost:", terrain.movementCost.ToString());
            EditorGUILayout.LabelField("Walkable:", terrain.isWalkable ? "Yes" : "No");

            if (terrain.isWalkable)
            {
                int movesPerTurn = Mathf.FloorToInt(5f / terrain.movementCost); // Assuming 5 movement points
                EditorGUILayout.LabelField("~Cells per turn (5 MP):", movesPerTurn.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawQuickActions(TerrainType terrain)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Duplicate"))
        {
            DuplicateTerrainType(terrain);
        }

        if (GUILayout.Button("Find in Scene"))
        {
            FindTerrainTypeInScene(terrain);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Show All Terrain Types"))
        {
            ShowAllTerrainTypesWindow();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private string GetMovementCostDescription(int cost)
    {
        if (cost <= 1)
            return "Very Fast - Units move through easily (roads, plains)";
        else if (cost == 2)
            return "Normal - Standard movement speed (grassland, light forest)";
        else if (cost == 3)
            return "Slow - Reduced movement speed (forests, hills)";
        else if (cost <= 5)
            return "Very Slow - Significantly reduced movement (mountains, swamps)";
        else
            return "Nearly Impassable - Extreme movement penalty (consider making unwalkable)";
    }

    private MessageType GetMovementCostMessageType(int cost)
    {
        if (cost <= 1)
            return MessageType.Info;
        else if (cost <= 3)
            return MessageType.None;
        else if (cost <= 5)
            return MessageType.Warning;
        else
            return MessageType.Error;
    }

    private void DrawMovementCostBar(int cost)
    {
        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));

        // Background
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        // Fill based on cost (inverse - lower cost = more fill)
        float fillAmount = Mathf.Clamp01(1f - (cost - 1) / 9f); // 1-10 scale
        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillAmount, barRect.height);

        Color fillColor = Color.Lerp(Color.red, Color.green, fillAmount);
        EditorGUI.DrawRect(fillRect, fillColor);

        // Label
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(barRect, $"Movement Speed: {cost} cost", labelStyle);
    }

    private void DrawPrefabPreview(Transform prefab, Rect rect)
    {
        if (prefab == null) return;

        // Simple preview using AssetPreview
        GameObject go = prefab.gameObject;
        Texture2D preview = AssetPreview.GetAssetPreview(go);

        if (preview != null)
        {
            GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.HelpBox(rect, "Generating preview...", MessageType.Info);
            AssetPreview.SetPreviewTextureCacheSize(256);
        }
    }

    private void DuplicateTerrainType(TerrainType original)
    {
        string path = AssetDatabase.GetAssetPath(original);
        string directory = System.IO.Path.GetDirectoryName(path);
        string filename = System.IO.Path.GetFileNameWithoutExtension(path);
        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{filename}_Copy.asset");

        AssetDatabase.CopyAsset(path, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        TerrainType newTerrain = AssetDatabase.LoadAssetAtPath<TerrainType>(newPath);
        Selection.activeObject = newTerrain;
        EditorGUIUtility.PingObject(newTerrain);

        Debug.Log($"Duplicated terrain type: {newPath}");
    }

    private void FindTerrainTypeInScene(TerrainType terrain)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Play Mode Required",
                "Enter Play mode to find cells using this terrain type in the active scene.",
                "OK");
            return;
        }

        HexCell[] allCells = FindObjectsOfType<HexCell>();
        var matchingCells = allCells.Where(cell => cell.TerrainType == terrain).ToArray();

        if (matchingCells.Length > 0)
        {
            Selection.objects = matchingCells.Select(c => c.gameObject).ToArray();
            Debug.Log($"Found {matchingCells.Length} cells using terrain type '{terrain.Name}'");
        }
        else
        {
            Debug.Log($"No cells found using terrain type '{terrain.Name}'");
        }
    }

    private void ShowAllTerrainTypesWindow()
    {
        TerrainTypeLibraryWindow.ShowWindow();
    }
}

/// <summary>
/// Window showing all terrain types in the project
/// </summary>
public class TerrainTypeLibraryWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private TerrainType[] allTerrainTypes;

    [MenuItem("Window/Terrain Type Library")]
    public static void ShowWindow()
    {
        TerrainTypeLibraryWindow window = GetWindow<TerrainTypeLibraryWindow>("Terrain Library");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnEnable()
    {
        LoadAllTerrainTypes();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Terrain Type Library", titleStyle);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Refresh"))
        {
            LoadAllTerrainTypes();
        }

        EditorGUILayout.Space(5);

        if (allTerrainTypes == null || allTerrainTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("No terrain types found in project. Create one via:\nAssets > Create > TBS > TerrainType", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Found {allTerrainTypes.Length} terrain types", EditorStyles.miniLabel);

        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var terrain in allTerrainTypes)
        {
            DrawTerrainTypeCard(terrain);
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTerrainTypeCard(TerrainType terrain)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Color indicator
        Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(5));
        EditorGUI.DrawRect(colorRect, terrain.Colour);

        EditorGUILayout.BeginHorizontal();

        // Icon preview
        if (terrain.Icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(48, 48);
            EditorGUI.DrawTextureTransparent(iconRect, terrain.Icon.texture);
        }

        // Info
        EditorGUILayout.BeginVertical();

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 12;
        EditorGUILayout.LabelField(terrain.Name, nameStyle);

        EditorGUILayout.LabelField($"Movement Cost: {terrain.movementCost}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Walkable: {(terrain.isWalkable ? "Yes" : "No")}", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();

        // Select button
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            Selection.activeObject = terrain;
            EditorGUIUtility.PingObject(terrain);
        }

        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(terrain.Description))
        {
            EditorGUILayout.LabelField(terrain.Description, EditorStyles.wordWrappedMiniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    private void LoadAllTerrainTypes()
    {
        string[] guids = AssetDatabase.FindAssets("t:TerrainType");
        allTerrainTypes = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<TerrainType>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(terrain => terrain != null)
            .OrderBy(terrain => terrain.movementCost)
            .ToArray();
    }
}
