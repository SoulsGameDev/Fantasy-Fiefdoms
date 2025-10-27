using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Debug window for visualizing and managing command history (undo/redo).
/// Provides Photoshop-style history panel with memory tracking and command management.
/// </summary>
public class CommandHistoryDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Vector2 settingsScrollPosition;

    private bool showSettings = false;
    private bool showStats = true;
    private bool autoRefresh = true;

    private int selectedUndoIndex = -1;
    private int selectedRedoIndex = -1;

    private GUIStyle undoCommandStyle;
    private GUIStyle redoCommandStyle;
    private GUIStyle selectedCommandStyle;
    private GUIStyle statsStyle;

    [MenuItem("Window/Command History Debugger")]
    public static void ShowWindow()
    {
        CommandHistoryDebugWindow window = GetWindow<CommandHistoryDebugWindow>("Command History");
        window.minSize = new Vector2(350, 400);
        window.Show();
    }

    private void OnEnable()
    {
        // Subscribe to history changes for auto-refresh
        CommandHistory.Instance.OnHistoryChanged += OnHistoryChanged;

        InitializeStyles();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        CommandHistory.Instance.OnHistoryChanged -= OnHistoryChanged;
    }

    private void OnHistoryChanged()
    {
        if (autoRefresh)
        {
            Repaint();
        }
    }

    private void InitializeStyles()
    {
        undoCommandStyle = new GUIStyle();
        undoCommandStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        undoCommandStyle.padding = new RectOffset(5, 5, 3, 3);
        undoCommandStyle.wordWrap = true;

        redoCommandStyle = new GUIStyle();
        redoCommandStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        redoCommandStyle.padding = new RectOffset(5, 5, 3, 3);
        redoCommandStyle.fontStyle = FontStyle.Italic;
        redoCommandStyle.wordWrap = true;

        selectedCommandStyle = new GUIStyle();
        selectedCommandStyle.normal.textColor = Color.white;
        selectedCommandStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.8f));
        selectedCommandStyle.padding = new RectOffset(5, 5, 3, 3);
        selectedCommandStyle.wordWrap = true;

        statsStyle = new GUIStyle(EditorStyles.helpBox);
        statsStyle.padding = new RectOffset(10, 10, 10, 10);
    }

    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    private void OnGUI()
    {
        if (undoCommandStyle == null)
        {
            InitializeStyles();
        }

        DrawTitle();
        EditorGUILayout.Space(5);

        DrawToolbar();
        EditorGUILayout.Space(5);

        if (showStats)
        {
            DrawStatistics();
            EditorGUILayout.Space(5);
        }

        if (showSettings)
        {
            DrawSettings();
            EditorGUILayout.Space(5);
        }

        DrawCommandHistory();
    }

    private void DrawTitle()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("Command History Debugger", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Undo/Redo System Visualization", subtitleStyle);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Undo button
        EditorGUI.BeginDisabledGroup(!CommandHistory.Instance.CanUndo || !Application.isPlaying);
        if (GUILayout.Button("⟲ Undo", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            CommandHistory.Instance.Undo();
        }
        EditorGUI.EndDisabledGroup();

        // Redo button
        EditorGUI.BeginDisabledGroup(!CommandHistory.Instance.CanRedo || !Application.isPlaying);
        if (GUILayout.Button("⟳ Redo", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            CommandHistory.Instance.Redo();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();

        // Stats toggle
        showStats = GUILayout.Toggle(showStats, "Stats", EditorStyles.toolbarButton, GUILayout.Width(50));

        // Settings toggle
        showSettings = GUILayout.Toggle(showSettings, "Settings", EditorStyles.toolbarButton, GUILayout.Width(60));

        // Auto-refresh toggle
        autoRefresh = GUILayout.Toggle(autoRefresh, "Auto", EditorStyles.toolbarButton, GUILayout.Width(45));

        // Refresh button
        if (GUILayout.Button("↻", EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            Repaint();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawStatistics()
    {
        EditorGUILayout.BeginVertical(statsStyle);

        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Statistics available during Play mode", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Undo Stack:", GUILayout.Width(120));
        EditorGUILayout.LabelField(CommandHistory.Instance.UndoCount.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Redo Stack:", GUILayout.Width(120));
        EditorGUILayout.LabelField(CommandHistory.Instance.RedoCount.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Total Commands:", GUILayout.Width(120));
        int total = CommandHistory.Instance.UndoCount + CommandHistory.Instance.RedoCount;
        EditorGUILayout.LabelField(total.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Memory Usage:", GUILayout.Width(120));
        EditorGUILayout.LabelField(CommandHistory.Instance.GetMemoryUsageString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        // Memory bar
        DrawMemoryUsageBar();

        EditorGUILayout.EndVertical();
    }

    private void DrawMemoryUsageBar()
    {
        long current = CommandHistory.Instance.GetCurrentMemoryUsage();
        long max = 10 * 1024 * 1024; // 10MB default

        float percentage = Mathf.Clamp01((float)current / max);

        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18));
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * percentage, barRect.height);
        Color fillColor = Color.Lerp(Color.green, Color.red, percentage);
        EditorGUI.DrawRect(fillRect, fillColor);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(barRect, $"{percentage * 100:F1}% of limit", labelStyle);
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Settings can only be modified during Play mode", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.Space(3);

        // Clear all button
        if (GUILayout.Button("Clear All History"))
        {
            if (EditorUtility.DisplayDialog(
                "Clear History",
                "Are you sure you want to clear all undo/redo history? This cannot be undone.",
                "Clear",
                "Cancel"))
            {
                CommandHistory.Instance.Clear();
            }
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Quick Undo/Redo:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Undo 5"))
        {
            CommandHistory.Instance.UndoMultiple(5);
        }

        if (GUILayout.Button("Undo 10"))
        {
            CommandHistory.Instance.UndoMultiple(10);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Redo 5"))
        {
            CommandHistory.Instance.RedoMultiple(5);
        }

        if (GUILayout.Button("Redo 10"))
        {
            CommandHistory.Instance.RedoMultiple(10);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawCommandHistory()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to view command history", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Command History", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Click a command to jump to that state (future feature)", MessageType.Info);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Draw undo stack (in reverse order, most recent at top)
        var undoHistory = CommandHistory.Instance.GetUndoHistory(100);

        if (undoHistory.Count == 0 && CommandHistory.Instance.GetRedoHistory(1).Count == 0)
        {
            EditorGUILayout.HelpBox("No commands in history. Execute actions to see them here.", MessageType.Info);
        }

        for (int i = 0; i < undoHistory.Count; i++)
        {
            DrawCommandEntry(undoHistory[i], i, true);
        }

        // Current state marker
        if (undoHistory.Count > 0 || CommandHistory.Instance.GetRedoHistory(1).Count > 0)
        {
            DrawCurrentStateMarker();
        }

        // Draw redo stack
        var redoHistory = CommandHistory.Instance.GetRedoHistory(100);
        for (int i = 0; i < redoHistory.Count; i++)
        {
            DrawCommandEntry(redoHistory[i], i, false);
        }

        EditorGUILayout.EndScrollView();

        DrawLegend();
    }

    private void DrawCommandEntry(string description, int index, bool isUndo)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Icon
        GUIStyle iconStyle = new GUIStyle(EditorStyles.label);
        iconStyle.fontSize = 16;

        if (isUndo)
        {
            iconStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            EditorGUILayout.LabelField("✓", iconStyle, GUILayout.Width(20));
        }
        else
        {
            iconStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            EditorGUILayout.LabelField("○", iconStyle, GUILayout.Width(20));
        }

        // Description
        GUIStyle style = isUndo ? undoCommandStyle : redoCommandStyle;
        EditorGUILayout.LabelField(description, style);

        // Index
        GUIStyle indexStyle = new GUIStyle(EditorStyles.miniLabel);
        indexStyle.alignment = TextAnchor.MiddleRight;
        EditorGUILayout.LabelField($"#{index}", indexStyle, GUILayout.Width(30));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCurrentStateMarker()
    {
        EditorGUILayout.Space(2);

        Rect markerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(3));
        EditorGUI.DrawRect(markerRect, new Color(1f, 0.5f, 0f));

        GUIStyle markerStyle = new GUIStyle(EditorStyles.boldLabel);
        markerStyle.alignment = TextAnchor.MiddleCenter;
        markerStyle.fontSize = 10;
        markerStyle.normal.textColor = new Color(1f, 0.5f, 0f);

        Rect labelRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18));
        GUI.Label(labelRect, "▼ CURRENT STATE ▼", markerStyle);

        EditorGUILayout.Space(2);
    }

    private void DrawLegend()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Legend", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUIStyle checkStyle = new GUIStyle(EditorStyles.label);
        checkStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
        EditorGUILayout.LabelField("✓", checkStyle, GUILayout.Width(20));
        EditorGUILayout.LabelField("Executed (can be undone)", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUIStyle circleStyle = new GUIStyle(EditorStyles.label);
        circleStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        EditorGUILayout.LabelField("○", circleStyle, GUILayout.Width(20));
        EditorGUILayout.LabelField("Undone (can be redone)", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        Rect orangeRect = GUILayoutUtility.GetRect(15, 15, GUILayout.Width(20));
        EditorGUI.DrawRect(orangeRect, new Color(1f, 0.5f, 0f));
        EditorGUILayout.LabelField("Current state marker", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}

/// <summary>
/// Custom inspector for CommandHistory (for debugging)
/// </summary>
[CustomEditor(typeof(CommandHistory))]
public class CommandHistoryInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Open Command History Debugger", GUILayout.Height(30)))
        {
            CommandHistoryDebugWindow.ShowWindow();
        }
    }
}
