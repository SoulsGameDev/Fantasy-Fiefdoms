using UnityEngine;

/// <summary>
/// ScriptableObject for saving and reusing camera configurations.
/// Allows designers to create preset configurations for different gameplay situations.
/// </summary>
[CreateAssetMenu(fileName = "New Camera Preset", menuName = "Camera/Camera Preset", order = 1)]
public class CameraPreset : ScriptableObject
{
    [Header("Preset Information")]
    [Tooltip("Display name for this preset")]
    public string presetName = "New Camera Preset";

    [TextArea(2, 4)]
    [Tooltip("Description of when to use this preset")]
    public string description = "Describe the intended use case for this camera configuration.";

    [Header("Default Camera Mode")]
    public CameraMode defaultCameraMode = CameraMode.TopDown;

    [Header("Movement Settings")]
    [Tooltip("Camera panning speed")]
    public float cameraSpeed = 10f;

    [Tooltip("Camera movement damping/smoothing")]
    public float cameraDamping = 5f;

    [Tooltip("Minimum XZ bounds for camera movement")]
    public Vector2 cameraBoundsMin = new Vector2(-100, -100);

    [Tooltip("Maximum XZ bounds for camera movement")]
    public Vector2 cameraBoundsMax = new Vector2(100, 100);

    [Header("Zoom Settings")]
    [Tooltip("Camera zoom speed (field of view change rate)")]
    public float cameraZoomSpeed = 1f;

    [Tooltip("Minimum field of view (most zoomed in)")]
    public float cameraZoomMin = 15f;

    [Tooltip("Maximum field of view (most zoomed out)")]
    public float cameraZoomMax = 100f;

    [Tooltip("Default field of view on start")]
    public float cameraZoomDefault = 50f;

    [Header("Rotation Settings")]
    [Tooltip("Enable camera rotation")]
    public bool enableRotation = false;

    [Tooltip("Camera rotation speed (degrees per second)")]
    public float cameraRotationSpeed = 50f;

    /// <summary>
    /// Applies this preset's settings to a CameraController
    /// </summary>
    public void ApplyToController(CameraController controller)
    {
        if (controller == null)
        {
            Debug.LogError("Cannot apply preset to null CameraController");
            return;
        }

        // Use reflection to set private fields
        var type = typeof(CameraController);

        SetField(type, controller, "cameraSpeed", cameraSpeed);
        SetField(type, controller, "cameraDamping", cameraDamping);
        SetField(type, controller, "cameraBoundsMin", cameraBoundsMin);
        SetField(type, controller, "cameraBoundsMax", cameraBoundsMax);
        SetField(type, controller, "cameraZoomSpeed", cameraZoomSpeed);
        SetField(type, controller, "cameraZoomMin", cameraZoomMin);
        SetField(type, controller, "cameraZoomMax", cameraZoomMax);
        SetField(type, controller, "cameraZoomDefault", cameraZoomDefault);
        SetField(type, controller, "enableRotation", enableRotation);
        SetField(type, controller, "cameraRotationSpeed", cameraRotationSpeed);
        SetField(type, controller, "defaultMode", defaultCameraMode);

        Debug.Log($"Applied camera preset: {presetName}");
    }

    private void SetField(System.Type type, object instance, string fieldName, object value)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(instance, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on CameraController");
        }
    }

    /// <summary>
    /// Captures current settings from a CameraController
    /// </summary>
    public void CaptureFromController(CameraController controller)
    {
        if (controller == null)
        {
            Debug.LogError("Cannot capture from null CameraController");
            return;
        }

        var type = typeof(CameraController);

        cameraSpeed = GetField<float>(type, controller, "cameraSpeed");
        cameraDamping = GetField<float>(type, controller, "cameraDamping");
        cameraBoundsMin = GetField<Vector2>(type, controller, "cameraBoundsMin");
        cameraBoundsMax = GetField<Vector2>(type, controller, "cameraBoundsMax");
        cameraZoomSpeed = GetField<float>(type, controller, "cameraZoomSpeed");
        cameraZoomMin = GetField<float>(type, controller, "cameraZoomMin");
        cameraZoomMax = GetField<float>(type, controller, "cameraZoomMax");
        cameraZoomDefault = GetField<float>(type, controller, "cameraZoomDefault");
        enableRotation = GetField<bool>(type, controller, "enableRotation");
        cameraRotationSpeed = GetField<float>(type, controller, "cameraRotationSpeed");
        defaultCameraMode = GetField<CameraMode>(type, controller, "defaultMode");

        Debug.Log($"Captured settings to preset: {presetName}");
    }

    private T GetField<T>(System.Type type, object instance, string fieldName)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (T)field.GetValue(instance);
        }

        Debug.LogWarning($"Field '{fieldName}' not found on CameraController");
        return default(T);
    }

    /// <summary>
    /// Creates a preset for exploration (wide view, fast movement)
    /// </summary>
    public static CameraPreset CreateExplorationPreset()
    {
        var preset = CreateInstance<CameraPreset>();
        preset.presetName = "Exploration";
        preset.description = "Wide view for exploring the map. Fast panning, zoomed out.";
        preset.cameraSpeed = 15f;
        preset.cameraDamping = 3f;
        preset.cameraZoomDefault = 75f;
        preset.cameraZoomSpeed = 1.5f;
        preset.enableRotation = false;
        return preset;
    }

    /// <summary>
    /// Creates a preset for tactical combat (closer view, slower movement)
    /// </summary>
    public static CameraPreset CreateCombatPreset()
    {
        var preset = CreateInstance<CameraPreset>();
        preset.presetName = "Tactical Combat";
        preset.description = "Close-up view for tactical decisions. Slower, more precise camera control.";
        preset.cameraSpeed = 8f;
        preset.cameraDamping = 7f;
        preset.cameraZoomDefault = 35f;
        preset.cameraZoomMin = 15f;
        preset.cameraZoomMax = 60f;
        preset.cameraZoomSpeed = 0.8f;
        preset.enableRotation = true;
        preset.cameraRotationSpeed = 40f;
        return preset;
    }

    /// <summary>
    /// Creates a preset for city building (medium view, smooth movement)
    /// </summary>
    public static CameraPreset CreateBuildingPreset()
    {
        var preset = CreateInstance<CameraPreset>();
        preset.presetName = "City Building";
        preset.description = "Balanced view for construction and management. Smooth camera movement.";
        preset.cameraSpeed = 12f;
        preset.cameraDamping = 6f;
        preset.cameraZoomDefault = 55f;
        preset.cameraZoomSpeed = 1.2f;
        preset.enableRotation = true;
        preset.cameraRotationSpeed = 60f;
        return preset;
    }

    /// <summary>
    /// Creates a preset for cinematic viewing (slow, smooth)
    /// </summary>
    public static CameraPreset CreateCinematicPreset()
    {
        var preset = CreateInstance<CameraPreset>();
        preset.presetName = "Cinematic";
        preset.description = "Slow, smooth camera for cinematic viewing and screenshots.";
        preset.cameraSpeed = 5f;
        preset.cameraDamping = 10f;
        preset.cameraZoomDefault = 45f;
        preset.cameraZoomSpeed = 0.5f;
        preset.enableRotation = true;
        preset.cameraRotationSpeed = 30f;
        return preset;
    }

    private void OnValidate()
    {
        // Ensure valid zoom range
        cameraZoomMin = Mathf.Max(1f, cameraZoomMin);
        cameraZoomMax = Mathf.Clamp(cameraZoomMax, cameraZoomMin + 1, 179f);
        cameraZoomDefault = Mathf.Clamp(cameraZoomDefault, cameraZoomMin, cameraZoomMax);

        // Ensure valid camera bounds
        cameraBoundsMax.x = Mathf.Max(cameraBoundsMin.x + 1, cameraBoundsMax.x);
        cameraBoundsMax.y = Mathf.Max(cameraBoundsMin.y + 1, cameraBoundsMax.y);

        // Ensure positive values
        cameraSpeed = Mathf.Max(0.1f, cameraSpeed);
        cameraDamping = Mathf.Max(0.1f, cameraDamping);
        cameraZoomSpeed = Mathf.Max(0.1f, cameraZoomSpeed);
        cameraRotationSpeed = Mathf.Max(1f, cameraRotationSpeed);
    }
}
