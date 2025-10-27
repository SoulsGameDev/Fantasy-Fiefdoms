using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    private CameraController controller;

    // Foldout states
    private bool showMovementSettings = true;
    private bool showZoomSettings = true;
    private bool showRotationSettings = true;
    private bool showBoundsEditor = true;
    private bool showPresets = false;

    // Bounds editing
    private bool editBoundsInScene = false;
    private Vector3[] boundsHandles = new Vector3[4];

    // Serialized properties
    private SerializedProperty cameraTargetProp;
    private SerializedProperty topDownCameraProp;
    private SerializedProperty focusCameraProp;
    private SerializedProperty defaultModeProp;
    private SerializedProperty currentModeProp;
    private SerializedProperty cameraSpeedProp;
    private SerializedProperty cameraDampingProp;
    private SerializedProperty cameraBoundsMinProp;
    private SerializedProperty cameraBoundsMaxProp;
    private SerializedProperty cameraZoomSpeedProp;
    private SerializedProperty cameraZoomMinProp;
    private SerializedProperty cameraZoomMaxProp;
    private SerializedProperty cameraZoomDefaultProp;
    private SerializedProperty enableRotationProp;
    private SerializedProperty cameraRotationSpeedProp;

    private void OnEnable()
    {
        controller = (CameraController)target;

        cameraTargetProp = serializedObject.FindProperty("cameraTarget");
        topDownCameraProp = serializedObject.FindProperty("topDownCamera");
        focusCameraProp = serializedObject.FindProperty("focusCamera");
        defaultModeProp = serializedObject.FindProperty("defaultMode");
        currentModeProp = serializedObject.FindProperty("currentMode");
        cameraSpeedProp = serializedObject.FindProperty("cameraSpeed");
        cameraDampingProp = serializedObject.FindProperty("cameraDamping");
        cameraBoundsMinProp = serializedObject.FindProperty("cameraBoundsMin");
        cameraBoundsMaxProp = serializedObject.FindProperty("cameraBoundsMax");
        cameraZoomSpeedProp = serializedObject.FindProperty("cameraZoomSpeed");
        cameraZoomMinProp = serializedObject.FindProperty("cameraZoomMin");
        cameraZoomMaxProp = serializedObject.FindProperty("cameraZoomMax");
        cameraZoomDefaultProp = serializedObject.FindProperty("cameraZoomDefault");
        enableRotationProp = serializedObject.FindProperty("enableRotation");
        cameraRotationSpeedProp = serializedObject.FindProperty("cameraRotationSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawTitle();
        EditorGUILayout.Space(10);

        DrawCameraReferences();
        EditorGUILayout.Space(10);

        DrawMovementSettings();
        EditorGUILayout.Space(10);

        DrawZoomSettings();
        EditorGUILayout.Space(10);

        DrawRotationSettings();
        EditorGUILayout.Space(10);

        DrawBoundsEditor();
        EditorGUILayout.Space(10);

        DrawPresets();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTitle()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Camera Controller", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Cinemachine-based camera system with presets", subtitleStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawCameraReferences()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Camera References", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(cameraTargetProp, new GUIContent("Camera Target"));
        EditorGUILayout.PropertyField(topDownCameraProp, new GUIContent("Top Down Camera"));
        EditorGUILayout.PropertyField(focusCameraProp, new GUIContent("Focus Camera"));

        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(defaultModeProp, new GUIContent("Default Mode"));

        if (Application.isPlaying)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(currentModeProp, new GUIContent("Current Mode"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Runtime Controls:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Top Down"))
            {
                controller.ChangeCamera(CameraMode.TopDown);
            }

            if (GUILayout.Button("Focus"))
            {
                controller.ChangeCamera(CameraMode.Focus);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMovementSettings()
    {
        showMovementSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSettings, "Movement Settings");

        if (showMovementSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(cameraSpeedProp, new GUIContent("Pan Speed"));
            DrawSpeedBar(cameraSpeedProp.floatValue, 5f, 20f, "Slow", "Fast");
            EditorGUILayout.HelpBox("Higher = faster panning", MessageType.None);

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(cameraDampingProp, new GUIContent("Damping / Smoothing"));
            DrawSpeedBar(cameraDampingProp.floatValue, 1f, 10f, "Snappy", "Smooth");
            EditorGUILayout.HelpBox("Higher = smoother, more laggy movement", MessageType.None);

            EditorGUILayout.Space(5);

            // Quick presets
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Fast"))
            {
                cameraSpeedProp.floatValue = 15f;
                cameraDampingProp.floatValue = 3f;
            }

            if (GUILayout.Button("Normal"))
            {
                cameraSpeedProp.floatValue = 10f;
                cameraDampingProp.floatValue = 5f;
            }

            if (GUILayout.Button("Slow"))
            {
                cameraSpeedProp.floatValue = 6f;
                cameraDampingProp.floatValue = 8f;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawZoomSettings()
    {
        showZoomSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showZoomSettings, "Zoom Settings");

        if (showZoomSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(cameraZoomSpeedProp, new GUIContent("Zoom Speed"));

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Field of View Range:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(cameraZoomMinProp, new GUIContent("Min FOV (Zoomed In)"));
            EditorGUILayout.PropertyField(cameraZoomMaxProp, new GUIContent("Max FOV (Zoomed Out)"));
            EditorGUILayout.PropertyField(cameraZoomDefaultProp, new GUIContent("Default FOV"));

            // Visual zoom range
            DrawZoomRangeVisual();

            EditorGUILayout.Space(5);

            // Quick presets
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Close-Up"))
            {
                cameraZoomMinProp.floatValue = 15f;
                cameraZoomMaxProp.floatValue = 60f;
                cameraZoomDefaultProp.floatValue = 35f;
            }

            if (GUILayout.Button("Normal"))
            {
                cameraZoomMinProp.floatValue = 15f;
                cameraZoomMaxProp.floatValue = 100f;
                cameraZoomDefaultProp.floatValue = 50f;
            }

            if (GUILayout.Button("Wide"))
            {
                cameraZoomMinProp.floatValue = 30f;
                cameraZoomMaxProp.floatValue = 120f;
                cameraZoomDefaultProp.floatValue = 75f;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRotationSettings()
    {
        showRotationSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationSettings, "Rotation Settings");

        if (showRotationSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(enableRotationProp, new GUIContent("Enable Rotation"));

            if (enableRotationProp.boolValue)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(cameraRotationSpeedProp, new GUIContent("Rotation Speed"));
                DrawSpeedBar(cameraRotationSpeedProp.floatValue, 20f, 100f, "Slow", "Fast");
            }
            else
            {
                EditorGUILayout.HelpBox("Camera rotation is disabled", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawBoundsEditor()
    {
        showBoundsEditor = EditorGUILayout.BeginFoldoutHeaderGroup(showBoundsEditor, "Camera Bounds");

        if (showBoundsEditor)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Movement Boundaries (XZ plane):", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(cameraBoundsMinProp, new GUIContent("Min (Bottom-Left)"));
            EditorGUILayout.PropertyField(cameraBoundsMaxProp, new GUIContent("Max (Top-Right)"));

            Vector2 boundsSize = cameraBoundsMaxProp.vector2Value - cameraBoundsMinProp.vector2Value;
            EditorGUILayout.LabelField($"Bounds Size: {boundsSize.x:F1} x {boundsSize.y:F1}");

            EditorGUILayout.Space(5);

            // Scene editing toggle
            editBoundsInScene = EditorGUILayout.Toggle("Edit in Scene View", editBoundsInScene);

            if (editBoundsInScene)
            {
                EditorGUILayout.HelpBox("Bounds handles visible in scene view. Drag to adjust.", MessageType.Info);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(5);

            // Quick size presets
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Small (50x50)"))
                SetBoundsSize(50);
            if (GUILayout.Button("Medium (100x100)"))
                SetBoundsSize(100);
            if (GUILayout.Button("Large (200x200)"))
                SetBoundsSize(200);

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Center Bounds on Origin"))
            {
                float halfWidth = boundsSize.x * 0.5f;
                float halfHeight = boundsSize.y * 0.5f;
                cameraBoundsMinProp.vector2Value = new Vector2(-halfWidth, -halfHeight);
                cameraBoundsMaxProp.vector2Value = new Vector2(halfWidth, halfHeight);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPresets()
    {
        showPresets = EditorGUILayout.BeginFoldoutHeaderGroup(showPresets, "Camera Presets");

        if (showPresets)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.HelpBox("Save current settings or load from a preset ScriptableObject", MessageType.Info);

            EditorGUILayout.Space(5);

            // Built-in templates
            EditorGUILayout.LabelField("Built-in Templates:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Exploration"))
                ApplyBuiltInPreset(CameraPreset.CreateExplorationPreset());

            if (GUILayout.Button("Combat"))
                ApplyBuiltInPreset(CameraPreset.CreateCombatPreset());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Building"))
                ApplyBuiltInPreset(CameraPreset.CreateBuildingPreset());

            if (GUILayout.Button("Cinematic"))
                ApplyBuiltInPreset(CameraPreset.CreateCinematicPreset());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Custom presets
            EditorGUILayout.LabelField("Custom Presets:", EditorStyles.boldLabel);

            if (GUILayout.Button("Save As New Preset..."))
            {
                SaveAsPreset();
            }

            if (GUILayout.Button("Load From Preset..."))
            {
                LoadFromPreset();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Show All Camera Presets"))
            {
                ShowAllPresets();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // Helper Methods

    private void DrawSpeedBar(float value, float min, float max, string minLabel, string maxLabel)
    {
        float normalized = Mathf.InverseLerp(min, max, value);

        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * normalized, barRect.height);
        Color fillColor = Color.Lerp(new Color(0.5f, 0.7f, 1f), new Color(1f, 0.7f, 0.3f), normalized);
        EditorGUI.DrawRect(fillRect, fillColor);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(barRect.x + 5, barRect.y, 50, barRect.height), minLabel, labelStyle);

        labelStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(barRect.x + barRect.width - 55, barRect.y, 50, barRect.height), maxLabel, labelStyle);
    }

    private void DrawZoomRangeVisual()
    {
        Rect rangeRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(40));
        EditorGUI.DrawRect(rangeRect, new Color(0.2f, 0.2f, 0.2f));

        float min = cameraZoomMinProp.floatValue;
        float max = cameraZoomMaxProp.floatValue;
        float defaultZoom = cameraZoomDefaultProp.floatValue;

        // Draw range bar
        float range = 179f; // Max possible FOV
        float minX = (min / range) * rangeRect.width;
        float maxX = (max / range) * rangeRect.width;
        float defaultX = (defaultZoom / range) * rangeRect.width;

        Rect rangeBar = new Rect(rangeRect.x + minX, rangeRect.y + 10, maxX - minX, 20);
        EditorGUI.DrawRect(rangeBar, new Color(0.4f, 0.6f, 0.8f));

        // Draw default marker
        Rect defaultMarker = new Rect(rangeRect.x + defaultX - 2, rangeRect.y + 5, 4, 30);
        EditorGUI.DrawRect(defaultMarker, Color.yellow);

        // Labels
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(rangeRect.x + minX, rangeRect.y + 25, 50, 15), $"{min:F0}", labelStyle);
        labelStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(rangeRect.x + maxX - 50, rangeRect.y + 25, 50, 15), $"{max:F0}", labelStyle);
    }

    private void SetBoundsSize(float size)
    {
        float halfSize = size * 0.5f;
        cameraBoundsMinProp.vector2Value = new Vector2(-halfSize, -halfSize);
        cameraBoundsMaxProp.vector2Value = new Vector2(halfSize, halfSize);
    }

    private void ApplyBuiltInPreset(CameraPreset preset)
    {
        if (EditorUtility.DisplayDialog(
            "Apply Preset",
            $"Apply '{preset.presetName}' preset?\n\n{preset.description}\n\nThis will overwrite current settings.",
            "Apply",
            "Cancel"))
        {
            Undo.RecordObject(target, "Apply Camera Preset");

            cameraSpeedProp.floatValue = preset.cameraSpeed;
            cameraDampingProp.floatValue = preset.cameraDamping;
            cameraBoundsMinProp.vector2Value = preset.cameraBoundsMin;
            cameraBoundsMaxProp.vector2Value = preset.cameraBoundsMax;
            cameraZoomSpeedProp.floatValue = preset.cameraZoomSpeed;
            cameraZoomMinProp.floatValue = preset.cameraZoomMin;
            cameraZoomMaxProp.floatValue = preset.cameraZoomMax;
            cameraZoomDefaultProp.floatValue = preset.cameraZoomDefault;
            enableRotationProp.boolValue = preset.enableRotation;
            cameraRotationSpeedProp.floatValue = preset.cameraRotationSpeed;
            defaultModeProp.enumValueIndex = (int)preset.defaultCameraMode;

            serializedObject.ApplyModifiedProperties();

            Debug.Log($"Applied preset: {preset.presetName}");
        }

        DestroyImmediate(preset);
    }

    private void SaveAsPreset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Camera Preset",
            "CameraPreset",
            "asset",
            "Choose where to save the camera preset"
        );

        if (string.IsNullOrEmpty(path)) return;

        CameraPreset preset = ScriptableObject.CreateInstance<CameraPreset>();
        preset.presetName = System.IO.Path.GetFileNameWithoutExtension(path);

        // Copy current settings
        preset.cameraSpeed = cameraSpeedProp.floatValue;
        preset.cameraDamping = cameraDampingProp.floatValue;
        preset.cameraBoundsMin = cameraBoundsMinProp.vector2Value;
        preset.cameraBoundsMax = cameraBoundsMaxProp.vector2Value;
        preset.cameraZoomSpeed = cameraZoomSpeedProp.floatValue;
        preset.cameraZoomMin = cameraZoomMinProp.floatValue;
        preset.cameraZoomMax = cameraZoomMaxProp.floatValue;
        preset.cameraZoomDefault = cameraZoomDefaultProp.floatValue;
        preset.enableRotation = enableRotationProp.boolValue;
        preset.cameraRotationSpeed = cameraRotationSpeedProp.floatValue;
        preset.defaultCameraMode = (CameraMode)defaultModeProp.enumValueIndex;

        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = preset;
        EditorGUIUtility.PingObject(preset);

        Debug.Log($"Created camera preset: {path}");
    }

    private void LoadFromPreset()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load Camera Preset",
            "Assets",
            "asset"
        );

        if (string.IsNullOrEmpty(path)) return;

        // Convert absolute to relative path
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        CameraPreset preset = AssetDatabase.LoadAssetAtPath<CameraPreset>(path);

        if (preset == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not load preset. Make sure you selected a CameraPreset asset.", "OK");
            return;
        }

        ApplyBuiltInPreset(preset);
    }

    private void ShowAllPresets()
    {
        CameraPresetLibraryWindow.ShowWindow();
    }

    private void OnSceneGUI()
    {
        if (!editBoundsInScene) return;

        Vector2 boundsMin = cameraBoundsMinProp.vector2Value;
        Vector2 boundsMax = cameraBoundsMaxProp.vector2Value;

        // Draw bounds rectangle
        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(boundsMin.x, 0, boundsMin.y);
        corners[1] = new Vector3(boundsMax.x, 0, boundsMin.y);
        corners[2] = new Vector3(boundsMax.x, 0, boundsMax.y);
        corners[3] = new Vector3(boundsMin.x, 0, boundsMax.y);
        corners[4] = corners[0];

        Handles.color = Color.yellow;
        Handles.DrawPolyLine(corners);

        // Draw handles at corners
        EditorGUI.BeginChangeCheck();

        Vector3 newCorner0 = Handles.PositionHandle(corners[0], Quaternion.identity);
        Vector3 newCorner2 = Handles.PositionHandle(corners[2], Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Adjust Camera Bounds");

            cameraBoundsMinProp.vector2Value = new Vector2(newCorner0.x, newCorner0.z);
            cameraBoundsMaxProp.vector2Value = new Vector2(newCorner2.x, newCorner2.z);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

/// <summary>
/// Window showing all camera presets
/// </summary>
public class CameraPresetLibraryWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private CameraPreset[] allPresets;

    [MenuItem("Window/Camera Preset Library")]
    public static void ShowWindow()
    {
        CameraPresetLibraryWindow window = GetWindow<CameraPresetLibraryWindow>("Camera Presets");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnEnable()
    {
        LoadAllPresets();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Camera Preset Library", titleStyle);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Refresh"))
        {
            LoadAllPresets();
        }

        EditorGUILayout.Space(5);

        if (allPresets == null || allPresets.Length == 0)
        {
            EditorGUILayout.HelpBox("No camera presets found. Create one via:\nAssets > Create > Camera > Camera Preset", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Found {allPresets.Length} presets", EditorStyles.miniLabel);
        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var preset in allPresets)
        {
            DrawPresetCard(preset);
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPresetCard(CameraPreset preset)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        // Info
        EditorGUILayout.BeginVertical();

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 12;
        EditorGUILayout.LabelField(preset.presetName, nameStyle);

        if (!string.IsNullOrEmpty(preset.description))
        {
            EditorGUILayout.LabelField(preset.description, EditorStyles.wordWrappedMiniLabel);
        }

        EditorGUILayout.LabelField($"Speed: {preset.cameraSpeed} | Zoom: {preset.cameraZoomDefault:F0}", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();

        // Select button
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            Selection.activeObject = preset;
            EditorGUIUtility.PingObject(preset);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void LoadAllPresets()
    {
        string[] guids = AssetDatabase.FindAssets("t:CameraPreset");
        allPresets = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CameraPreset>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(preset => preset != null)
            .OrderBy(preset => preset.presetName)
            .ToArray();
    }
}
