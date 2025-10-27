using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom inspector for HexCell showing all coordinate systems, states, and pathfinding data.
/// Provides visual debugging tools and quick actions for cell manipulation.
/// </summary>
[CustomPropertyDrawer(typeof(HexCell))]
public class HexCellPropertyDrawer : PropertyDrawer
{
    private bool showCoordinates = true;
    private bool showInteractionState = true;
    private bool showPathfindingState = true;
    private bool showNeighbors = false;
    private bool showQuickActions = false;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Calculate height based on expanded sections
        float height = EditorGUIUtility.singleLineHeight + 5; // Title

        if (property.isExpanded)
        {
            height += EditorGUIUtility.singleLineHeight * 20; // Approximate for all sections
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Title
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + 5;

            // Draw all properties
            y = DrawCoordinates(position.x, y, position.width, property);
            y = DrawTerrainType(position.x, y, position.width, property);
            y = DrawStates(position.x, y, position.width, property);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private float DrawCoordinates(float x, float y, float width, SerializedProperty property)
    {
        Rect labelRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "Coordinates", EditorStyles.boldLabel);
        y += EditorGUIUtility.singleLineHeight;

        // Offset
        SerializedProperty offsetProp = property.FindPropertyRelative("<OffsetCoordinates>k__BackingField");
        if (offsetProp != null)
        {
            Rect offsetRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(offsetRect, offsetProp, new GUIContent("Offset"));
            y += EditorGUIUtility.singleLineHeight;
        }

        // Cube
        SerializedProperty cubeProp = property.FindPropertyRelative("<CubeCoordinates>k__BackingField");
        if (cubeProp != null)
        {
            Rect cubeRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(cubeRect, cubeProp, new GUIContent("Cube"));
            y += EditorGUIUtility.singleLineHeight;
        }

        // Axial
        SerializedProperty axialProp = property.FindPropertyRelative("<AxialCoordinates>k__BackingField");
        if (axialProp != null)
        {
            Rect axialRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(axialRect, axialProp, new GUIContent("Axial"));
            y += EditorGUIUtility.singleLineHeight;
        }

        return y + 5;
    }

    private float DrawTerrainType(float x, float y, float width, SerializedProperty property)
    {
        SerializedProperty terrainProp = property.FindPropertyRelative("<TerrainType>k__BackingField");
        if (terrainProp != null)
        {
            Rect terrainRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(terrainRect, terrainProp, new GUIContent("Terrain"));
            y += EditorGUIUtility.singleLineHeight;
        }

        return y + 5;
    }

    private float DrawStates(float x, float y, float width, SerializedProperty property)
    {
        Rect labelRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "States (Play Mode Only)", EditorStyles.boldLabel);
        y += EditorGUIUtility.singleLineHeight;

        if (!Application.isPlaying)
        {
            Rect helpRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight * 2);
            EditorGUI.HelpBox(helpRect, "State information available during Play mode", MessageType.Info);
            y += EditorGUIUtility.singleLineHeight * 2;
        }

        return y;
    }
}

/// <summary>
/// Editor window for detailed HexCell inspection and debugging
/// </summary>
public class HexCellInspectorWindow : EditorWindow
{
    private HexCell selectedCell;
    private GameObject selectedCellObject;
    private Vector2 scrollPosition;

    private bool showCoordinates = true;
    private bool showInteractionState = true;
    private bool showPathfindingState = true;
    private bool showNeighbors = false;

    [MenuItem("Window/Hex Cell Inspector")]
    public static void ShowWindow()
    {
        HexCellInspectorWindow window = GetWindow<HexCellInspectorWindow>("Hex Cell Inspector");
        window.minSize = new Vector2(350, 500);
        window.Show();
    }

    private void OnGUI()
    {
        DrawTitle();
        EditorGUILayout.Space(5);

        DrawSelection();
        EditorGUILayout.Space(10);

        if (selectedCell != null && Application.isPlaying)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawCoordinatesSection();
            EditorGUILayout.Space(10);

            DrawTerrainSection();
            EditorGUILayout.Space(10);

            DrawInteractionStateSection();
            EditorGUILayout.Space(10);

            DrawPathfindingStateSection();
            EditorGUILayout.Space(10);

            DrawNeighborsSection();
            EditorGUILayout.Space(10);

            DrawQuickActions();

            EditorGUILayout.EndScrollView();
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to inspect hex cells", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Select a GameObject with a HexCell to inspect it", MessageType.Info);
        }
    }

    private void DrawTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("Hex Cell Inspector", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Detailed cell data and debugging", subtitleStyle);
    }

    private void DrawSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);

        GameObject newSelection = EditorGUILayout.ObjectField("Cell Object:", selectedCellObject, typeof(GameObject), true) as GameObject;

        if (newSelection != selectedCellObject)
        {
            selectedCellObject = newSelection;
            selectedCell = null; // Will be found via grid lookup in Play mode
        }

        if (GUILayout.Button("Use Current Selection"))
        {
            if (Selection.activeGameObject != null)
            {
                selectedCellObject = Selection.activeGameObject;
                selectedCell = null;
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCoordinatesSection()
    {
        showCoordinates = EditorGUILayout.BeginFoldoutHeaderGroup(showCoordinates, "Coordinate Systems");

        if (showCoordinates && selectedCell != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Offset Coordinates:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  X: {selectedCell.OffsetCoordinates.x}");
            EditorGUILayout.LabelField($"  Y: {selectedCell.OffsetCoordinates.y}");

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Cube Coordinates:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  X: {selectedCell.CubeCoordinates.x}");
            EditorGUILayout.LabelField($"  Y: {selectedCell.CubeCoordinates.y}");
            EditorGUILayout.LabelField($"  Z: {selectedCell.CubeCoordinates.z}");

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Axial Coordinates:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  Q: {selectedCell.AxialCoordinates.x}");
            EditorGUILayout.LabelField($"  R: {selectedCell.AxialCoordinates.y}");

            EditorGUILayout.Space(5);

            // Visual coordinate diagram
            DrawCoordinateDiagram();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawCoordinateDiagram()
    {
        EditorGUILayout.LabelField("Coordinate Reference:", EditorStyles.miniLabel);

        Rect diagramRect = GUILayoutUtility.GetRect(250, 80);
        EditorGUI.DrawRect(diagramRect, new Color(0.2f, 0.2f, 0.2f));

        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
        textStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(diagramRect.x + 10, diagramRect.y + 10, 230, 20),
            $"Offset: Row-Column grid system", textStyle);
        GUI.Label(new Rect(diagramRect.x + 10, diagramRect.y + 30, 230, 20),
            $"Cube: X + Y + Z = 0 (constraint)", textStyle);
        GUI.Label(new Rect(diagramRect.x + 10, diagramRect.y + 50, 230, 20),
            $"Axial: Q-R diagonal coordinate system", textStyle);
    }

    private void DrawTerrainSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Terrain Information", EditorStyles.boldLabel);

        if (selectedCell.TerrainType != null)
        {
            // Color preview
            Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(30));
            EditorGUI.DrawRect(colorRect, selectedCell.TerrainType.Colour);

            GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = Color.white;
            GUI.Label(colorRect, selectedCell.TerrainType.Name, nameStyle);

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Name:", selectedCell.TerrainType.Name);
            EditorGUILayout.LabelField("Movement Cost:", selectedCell.TerrainType.movementCost.ToString());
            EditorGUILayout.LabelField("Walkable:", selectedCell.TerrainType.isWalkable ? "Yes" : "No");

            if (!string.IsNullOrEmpty(selectedCell.TerrainType.Description))
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(selectedCell.TerrainType.Description, EditorStyles.wordWrappedMiniLabel);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No terrain type assigned", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawInteractionStateSection()
    {
        showInteractionState = EditorGUILayout.BeginFoldoutHeaderGroup(showInteractionState, "Interaction State");

        if (showInteractionState && selectedCell != null && selectedCell.InteractionState != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            CellState currentState = selectedCell.InteractionState.State;

            // State display with color
            EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);

            Rect stateRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(25));
            Color stateColor = GetStateColor(currentState);
            EditorGUI.DrawRect(stateRect, stateColor);

            GUIStyle stateStyle = new GUIStyle(EditorStyles.boldLabel);
            stateStyle.alignment = TextAnchor.MiddleCenter;
            stateStyle.fontSize = 12;
            stateStyle.normal.textColor = Color.white;
            GUI.Label(stateRect, currentState.ToString().ToUpper(), stateStyle);

            EditorGUILayout.Space(5);

            // State description
            EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(GetStateDescription(currentState), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(5);

            // Possible transitions
            EditorGUILayout.LabelField("Possible Transitions:", EditorStyles.boldLabel);
            DrawPossibleTransitions(currentState);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPathfindingStateSection()
    {
        showPathfindingState = EditorGUILayout.BeginFoldoutHeaderGroup(showPathfindingState, "Pathfinding State");

        if (showPathfindingState && selectedCell != null && selectedCell.PathfindingState != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var state = selectedCell.PathfindingState;

            // Persistent flags
            EditorGUILayout.LabelField("Persistent State:", EditorStyles.boldLabel);
            DrawStateFlag("Walkable", state.IsWalkable, "Cell can be pathfinded through");
            DrawStateFlag("Occupied", state.IsOccupied, "Cell is occupied by a unit");
            DrawStateFlag("Explored", state.IsExplored, "Cell visible (not in fog of war)");
            DrawStateFlag("Reachable", state.IsReachable, "Cell reachable with current movement");
            DrawStateFlag("Reserved", state.IsReserved, "Cell reserved for movement");
            DrawStateFlag("Path", state.IsPath, "Cell is part of active path");

            EditorGUILayout.Space(5);

            // Pathfinding costs
            EditorGUILayout.LabelField("Pathfinding Costs:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Movement Cost: {state.MovementCost}");
            EditorGUILayout.LabelField($"G Cost: {state.GCost} (cost from start)");
            EditorGUILayout.LabelField($"H Cost: {state.HCost} (heuristic to goal)");
            EditorGUILayout.LabelField($"F Cost: {state.FCost} (G + H total)");

            EditorGUILayout.Space(5);

            // Search state
            EditorGUILayout.LabelField("Search State:", EditorStyles.boldLabel);
            DrawStateFlag("In Open Set", state.IsInOpenSet, "Cell in frontier to explore");
            DrawStateFlag("In Closed Set", state.IsInClosedSet, "Cell already explored");

            if (state.CameFrom != null)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Came From: {state.CameFrom.OffsetCoordinates}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawNeighborsSection()
    {
        showNeighbors = EditorGUILayout.BeginFoldoutHeaderGroup(showNeighbors, "Neighbors");

        if (showNeighbors && selectedCell != null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var neighbors = selectedCell.GetNeighbors();

            if (neighbors != null && neighbors.Count > 0)
            {
                EditorGUILayout.LabelField($"Neighbor Count: {neighbors.Count}/6", EditorStyles.boldLabel);

                EditorGUILayout.Space(3);

                // Draw neighbor diagram
                DrawNeighborDiagram(neighbors);

                EditorGUILayout.Space(5);

                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField($"{neighbor.OffsetCoordinates}", GUILayout.Width(80));

                        if (neighbor.TerrainType != null)
                        {
                            Rect colorRect = GUILayoutUtility.GetRect(15, 15, GUILayout.Width(20));
                            EditorGUI.DrawRect(colorRect, neighbor.TerrainType.Colour);

                            EditorGUILayout.LabelField(neighbor.TerrainType.Name);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No neighbors found", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawNeighborDiagram(System.Collections.Generic.List<HexCell> neighbors)
    {
        Rect diagramRect = GUILayoutUtility.GetRect(200, 200);
        EditorGUI.DrawRect(diagramRect, new Color(0.15f, 0.15f, 0.15f));

        Vector2 center = new Vector2(diagramRect.x + 100, diagramRect.y + 100);
        float radius = 60f;

        // Draw center hex
        Rect centerRect = new Rect(center.x - 15, center.y - 15, 30, 30);
        EditorGUI.DrawRect(centerRect, new Color(0.3f, 0.6f, 0.3f));

        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        GUI.Label(centerRect, "CENTER", textStyle);

        // Draw neighbors in hex pattern
        for (int i = 0; i < neighbors.Count && i < 6; i++)
        {
            float angle = (60f * i - 30f) * Mathf.Deg2Rad;
            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            Rect neighborRect = new Rect(pos.x - 12, pos.y - 12, 24, 24);

            Color neighborColor = neighbors[i] != null && neighbors[i].TerrainType != null
                ? neighbors[i].TerrainType.Colour
                : new Color(0.3f, 0.3f, 0.3f);

            EditorGUI.DrawRect(neighborRect, neighborColor);
        }
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Actions only available during Play mode", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        if (selectedCell == null)
        {
            EditorGUILayout.HelpBox("Select a cell first", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField("State Actions:", EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Reveal"))
        {
            selectedCell.HandleInput(InputEvent.RevealFog);
        }

        if (GUILayout.Button("Select"))
        {
            selectedCell.HandleInput(InputEvent.MouseDown);
        }

        if (GUILayout.Button("Focus"))
        {
            selectedCell.HandleInput(InputEvent.FKeyDown);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Deselect"))
        {
            selectedCell.HandleInput(InputEvent.Deselect);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Pathfinding Actions:", EditorStyles.miniLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Toggle Walkable"))
        {
            selectedCell.PathfindingState.IsWalkable = !selectedCell.PathfindingState.IsWalkable;
        }

        if (GUILayout.Button("Toggle Explored"))
        {
            selectedCell.PathfindingState.IsExplored = !selectedCell.PathfindingState.IsExplored;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawStateFlag(string label, bool value, string tooltip)
    {
        EditorGUILayout.BeginHorizontal();

        GUIStyle iconStyle = new GUIStyle(EditorStyles.label);
        iconStyle.normal.textColor = value ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
        EditorGUILayout.LabelField(value ? "✓" : "✗", iconStyle, GUILayout.Width(20));

        EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(120));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawPossibleTransitions(CellState currentState)
    {
        string[] transitions = GetPossibleTransitions(currentState);

        foreach (string transition in transitions)
        {
            EditorGUILayout.LabelField($"  → {transition}", EditorStyles.miniLabel);
        }
    }

    private string[] GetPossibleTransitions(CellState state)
    {
        switch (state)
        {
            case CellState.Invisible:
                return new string[] { "RevealFog → Visible" };
            case CellState.Visible:
                return new string[] { "MouseEnter → Highlighted", "MouseDown → Selected", "FKeyDown → Focused" };
            case CellState.Highlighted:
                return new string[] { "MouseExit → Visible", "MouseDown → Selected", "FKeyDown → Focused" };
            case CellState.Selected:
                return new string[] { "MouseExit → Visible", "MouseUp → Highlighted", "FKeyDown → Focused", "Deselect → Visible" };
            case CellState.Focused:
                return new string[] { "MouseExit → Selected", "MouseDown → Selected", "FKeyDown → Visible", "Deselect → Visible" };
            default:
                return new string[0];
        }
    }

    private Color GetStateColor(CellState state)
    {
        switch (state)
        {
            case CellState.Invisible: return new Color(0.3f, 0.3f, 0.3f);
            case CellState.Visible: return new Color(0.5f, 0.5f, 0.5f);
            case CellState.Highlighted: return Color.yellow;
            case CellState.Selected: return Color.green;
            case CellState.Focused: return new Color(0f, 0.5f, 1f);
            default: return Color.gray;
        }
    }

    private string GetStateDescription(CellState state)
    {
        switch (state)
        {
            case CellState.Invisible: return "Cell hidden by fog of war. Only responds to RevealFog event.";
            case CellState.Visible: return "Cell is visible but not interacted with. Default state after fog reveal.";
            case CellState.Highlighted: return "Mouse is hovering over cell. Visual feedback for potential interaction.";
            case CellState.Selected: return "Cell is actively selected (mouse down). Primary interaction state.";
            case CellState.Focused: return "Cell has special focus (F key). Camera or UI may focus on this cell.";
            default: return "Unknown state";
        }
    }
}
