using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for visualizing and testing hex cell state machine transitions.
/// Provides a visual state diagram and interactive testing tools.
/// </summary>
public class HexCellStateManagerEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private CellState selectedState = CellState.Visible;
    private InputEvent selectedInput = InputEvent.MouseEnter;

    // State machine visualization data
    private static readonly Dictionary<CellState, Vector2> statePositions = new Dictionary<CellState, Vector2>
    {
        { CellState.Invisible, new Vector2(400, 50) },
        { CellState.Visible, new Vector2(400, 150) },
        { CellState.Highlighted, new Vector2(200, 250) },
        { CellState.Selected, new Vector2(400, 350) },
        { CellState.Focused, new Vector2(600, 250) }
    };

    private static readonly Dictionary<CellState, Color> stateColors = new Dictionary<CellState, Color>
    {
        { CellState.Invisible, new Color(0.3f, 0.3f, 0.3f) },
        { CellState.Visible, new Color(0.5f, 0.5f, 0.5f) },
        { CellState.Highlighted, new Color(1f, 1f, 0f) },
        { CellState.Selected, new Color(0f, 1f, 0f) },
        { CellState.Focused, new Color(0f, 0.5f, 1f) }
    };

    private static readonly Dictionary<CellState, string> stateDescriptions = new Dictionary<CellState, string>
    {
        { CellState.Invisible, "Cell hidden by fog of war. Only responds to RevealFog event." },
        { CellState.Visible, "Cell is visible but not interacted with. Default state after fog reveal." },
        { CellState.Highlighted, "Mouse is hovering over cell. Visual feedback for potential interaction." },
        { CellState.Selected, "Cell is actively selected (mouse down). Primary interaction state." },
        { CellState.Focused, "Cell has special focus (F key). Camera or UI may focus on this cell." }
    };

    // Transition data for visualization
    private struct Transition
    {
        public CellState from;
        public CellState to;
        public InputEvent trigger;
        public string description;

        public Transition(CellState from, CellState to, InputEvent trigger, string description)
        {
            this.from = from;
            this.to = to;
            this.trigger = trigger;
            this.description = description;
        }
    }

    private static readonly List<Transition> allTransitions = new List<Transition>
    {
        // From Invisible
        new Transition(CellState.Invisible, CellState.Visible, InputEvent.RevealFog, "Fog of war revealed"),

        // From Visible
        new Transition(CellState.Visible, CellState.Highlighted, InputEvent.MouseEnter, "Mouse enters cell"),
        new Transition(CellState.Visible, CellState.Selected, InputEvent.MouseDown, "Click cell directly"),
        new Transition(CellState.Visible, CellState.Focused, InputEvent.FKeyDown, "Press F to focus"),

        // From Highlighted
        new Transition(CellState.Highlighted, CellState.Visible, InputEvent.MouseExit, "Mouse leaves cell"),
        new Transition(CellState.Highlighted, CellState.Selected, InputEvent.MouseDown, "Click while hovering"),
        new Transition(CellState.Highlighted, CellState.Focused, InputEvent.FKeyDown, "Press F while hovering"),

        // From Selected
        new Transition(CellState.Selected, CellState.Visible, InputEvent.MouseExit, "Mouse leaves selected cell"),
        new Transition(CellState.Selected, CellState.Highlighted, InputEvent.MouseUp, "Release mouse button"),
        new Transition(CellState.Selected, CellState.Focused, InputEvent.FKeyDown, "Press F while selected"),
        new Transition(CellState.Selected, CellState.Visible, InputEvent.Deselect, "Programmatic deselect"),

        // From Focused
        new Transition(CellState.Focused, CellState.Selected, InputEvent.MouseExit, "Mouse leaves focused cell"),
        new Transition(CellState.Focused, CellState.Selected, InputEvent.MouseDown, "Click focused cell"),
        new Transition(CellState.Focused, CellState.Visible, InputEvent.FKeyDown, "Press F to unfocus"),
        new Transition(CellState.Focused, CellState.Visible, InputEvent.Deselect, "Programmatic deselect")
    };

    [MenuItem("Window/Hex Cell State Machine Visualizer")]
    public static void ShowWindow()
    {
        HexCellStateManagerEditorWindow window = GetWindow<HexCellStateManagerEditorWindow>("State Machine");
        window.minSize = new Vector2(800, 600);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawTitle();
        EditorGUILayout.Space(10);

        DrawStateMachineVisualization();
        EditorGUILayout.Space(20);

        DrawStateInformation();
        EditorGUILayout.Space(20);

        DrawTransitionTable();
        EditorGUILayout.Space(20);

        DrawInteractiveTester();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("Hex Cell State Machine Visualizer", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("5 States | 7 Input Events | 14 Transitions", subtitleStyle);
    }

    private void DrawStateMachineVisualization()
    {
        EditorGUILayout.LabelField("State Machine Diagram", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Visual representation of all cell states and their transitions", MessageType.Info);

        Rect diagramRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
            GUILayout.Height(450), GUILayout.ExpandWidth(true));

        // Draw background
        EditorGUI.DrawRect(diagramRect, new Color(0.2f, 0.2f, 0.2f));

        // Draw transitions (arrows) first, so they're behind nodes
        DrawTransitions(diagramRect);

        // Draw state nodes
        DrawStates(diagramRect);
    }

    private void DrawStates(Rect container)
    {
        foreach (var kvp in statePositions)
        {
            CellState state = kvp.Key;
            Vector2 position = kvp.Value;

            // Calculate absolute position within container
            Vector2 absPos = new Vector2(
                container.x + position.x,
                container.y + position.y
            );

            // Draw state node
            Rect nodeRect = new Rect(absPos.x - 60, absPos.y - 30, 120, 60);

            // Node background
            EditorGUI.DrawRect(nodeRect, stateColors[state]);

            // Node border
            Rect borderRect = new Rect(nodeRect.x - 2, nodeRect.y - 2, nodeRect.width + 4, nodeRect.height + 4);
            EditorGUI.DrawRect(borderRect, Color.white);
            EditorGUI.DrawRect(nodeRect, stateColors[state]);

            // State name
            GUIStyle stateStyle = new GUIStyle(EditorStyles.boldLabel);
            stateStyle.alignment = TextAnchor.MiddleCenter;
            stateStyle.normal.textColor = Color.black;
            GUI.Label(nodeRect, state.ToString(), stateStyle);
        }
    }

    private void DrawTransitions(Rect container)
    {
        Handles.BeginGUI();

        foreach (var transition in allTransitions)
        {
            if (!statePositions.ContainsKey(transition.from) || !statePositions.ContainsKey(transition.to))
                continue;

            Vector2 fromPos = statePositions[transition.from];
            Vector2 toPos = statePositions[transition.to];

            Vector2 absFrom = new Vector2(container.x + fromPos.x, container.y + fromPos.y);
            Vector2 absto = new Vector2(container.x + toPos.x, container.y + toPos.y);

            // Calculate arrow direction
            Vector2 direction = (absto - absFrom).normalized;
            Vector2 arrowStart = absFrom + direction * 40; // Offset from node center
            Vector2 arrowEnd = absto - direction * 40;

            // Draw arrow
            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            Handles.DrawLine(arrowStart, arrowEnd);

            // Draw arrowhead
            DrawArrowHead(arrowEnd, direction);
        }

        Handles.EndGUI();
    }

    private void DrawArrowHead(Vector2 position, Vector2 direction)
    {
        float arrowSize = 10f;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        Vector2 point1 = position - direction * arrowSize + perpendicular * (arrowSize * 0.5f);
        Vector2 point2 = position - direction * arrowSize - perpendicular * (arrowSize * 0.5f);

        Handles.DrawLine(position, point1);
        Handles.DrawLine(position, point2);
    }

    private void DrawStateInformation()
    {
        EditorGUILayout.LabelField("State Descriptions", EditorStyles.boldLabel);

        foreach (var kvp in stateDescriptions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Color indicator
            Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(5));
            EditorGUI.DrawRect(colorRect, stateColors[kvp.Key]);

            EditorGUILayout.LabelField(kvp.Key.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(kvp.Value, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
    }

    private void DrawTransitionTable()
    {
        EditorGUILayout.LabelField("Transition Table", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Complete list of all state transitions and their triggers", MessageType.Info);

        // Group transitions by source state
        var groupedTransitions = new Dictionary<CellState, List<Transition>>();
        foreach (var transition in allTransitions)
        {
            if (!groupedTransitions.ContainsKey(transition.from))
            {
                groupedTransitions[transition.from] = new List<Transition>();
            }
            groupedTransitions[transition.from].Add(transition);
        }

        foreach (var kvp in groupedTransitions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Source state header
            Rect colorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(3));
            EditorGUI.DrawRect(colorRect, stateColors[kvp.Key]);

            EditorGUILayout.LabelField($"From: {kvp.Key}", EditorStyles.boldLabel);

            foreach (var transition in kvp.Value)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"→ {transition.to}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"[{transition.trigger}]", GUILayout.Width(120));
                EditorGUILayout.LabelField(transition.description, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
    }

    private void DrawInteractiveTester()
    {
        EditorGUILayout.LabelField("Interactive Testing (Play Mode Only)", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Select a cell in the scene hierarchy to test state transitions", MessageType.Info);

            EditorGUILayout.Space(5);
            selectedState = (CellState)EditorGUILayout.EnumPopup("Target State:", selectedState);
            selectedInput = (InputEvent)EditorGUILayout.EnumPopup("Input Event:", selectedInput);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Test Transition"))
            {
                TestTransition();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("This will log the transition result to the console", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Interactive testing only available during Play mode. Enter Play mode to test state transitions on actual cells.", MessageType.Warning);
        }

        EditorGUILayout.EndVertical();
    }

    private void TestTransition()
    {
        // Find selected cell in scene
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected. Please select a cell in the scene.");
            return;
        }

        // Try to find HexCell component or reference
        Debug.Log($"Testing transition: {selectedInput} → {selectedState}");
        Debug.Log("Note: Actual transition testing requires HexCell reference. This is a placeholder for UI demonstration.");
    }
}

/// <summary>
/// Inspector button to open the state machine visualizer
/// </summary>
[CustomEditor(typeof(HexCellStateManager))]
public class HexCellStateManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("HexCellStateManager controls cell interaction states with guards and transition validation.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Open State Machine Visualizer", GUILayout.Height(30)))
        {
            HexCellStateManagerEditorWindow.ShowWindow();
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Quick Reference", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("• 5 States: Invisible, Visible, Highlighted, Selected, Focused");
        EditorGUILayout.LabelField("• 7 Input Events: MouseEnter, MouseExit, MouseDown, MouseUp, FKeyDown, Deselect, RevealFog");
        EditorGUILayout.LabelField("• Transition lookup is O(1) via dictionary");
        EditorGUILayout.LabelField("• Guards can prevent transitions based on game state");
    }
}
