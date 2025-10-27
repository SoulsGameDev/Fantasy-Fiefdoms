using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator mapGen;

    // Foldout states
    private bool showBasicSettings = true;
    private bool showNoiseSettings = true;
    private bool showBiomeSettings = true;
    private bool showPreview = true;
    private bool showStatistics = false;
    private bool showPresets = false;

    // Preview settings
    private int previewOctave = -1; // -1 means show all octaves
    private Texture2D previewTexture;
    private Texture2D noisePreviewTexture;
    private bool showNoisePreview = true;
    private bool showColorPreview = true;

    // Comparison
    private int comparisonSeed = 0;
    private Texture2D comparisonTexture;

    // Statistics
    private Dictionary<TerrainType, int> terrainCounts;
    private Dictionary<TerrainType, float> terrainPercentages;

    // Serialized properties
    private SerializedProperty widthProp;
    private SerializedProperty heightProp;
    private SerializedProperty noiseScaleProp;
    private SerializedProperty octavesProp;
    private SerializedProperty persistanceProp;
    private SerializedProperty lacunarityProp;
    private SerializedProperty seedProp;
    private SerializedProperty offsetProp;
    private SerializedProperty autoUpdateProp;
    private SerializedProperty useHexGridProp;
    private SerializedProperty generateMapOnStartProp;
    private SerializedProperty useThreadedGenerationProp;
    private SerializedProperty biomesProp;

    private void OnEnable()
    {
        mapGen = (MapGenerator)target;

        widthProp = serializedObject.FindProperty("Width");
        heightProp = serializedObject.FindProperty("Height");
        noiseScaleProp = serializedObject.FindProperty("NoiseScale");
        octavesProp = serializedObject.FindProperty("Octaves");
        persistanceProp = serializedObject.FindProperty("Persistance");
        lacunarityProp = serializedObject.FindProperty("Lacunarity");
        seedProp = serializedObject.FindProperty("Seed");
        offsetProp = serializedObject.FindProperty("Offset");
        autoUpdateProp = serializedObject.FindProperty("AutoUpdate");
        useHexGridProp = serializedObject.FindProperty("UseHexGrid");
        generateMapOnStartProp = serializedObject.FindProperty("GenerateMapOnStart");
        useThreadedGenerationProp = serializedObject.FindProperty("UseThreadedGeneration");
        biomesProp = serializedObject.FindProperty("Biomes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawTitle();
        EditorGUILayout.Space(10);

        DrawQuickActions();
        EditorGUILayout.Space(10);

        DrawBasicSettings();
        EditorGUILayout.Space(10);

        DrawNoiseSettings();
        EditorGUILayout.Space(10);

        DrawBiomeSettings();
        EditorGUILayout.Space(10);

        DrawPreview();
        EditorGUILayout.Space(10);

        DrawStatistics();
        EditorGUILayout.Space(10);

        DrawPresets();

        // Auto-update functionality
        if (serializedObject.ApplyModifiedProperties())
        {
            if (mapGen.AutoUpdate)
            {
                mapGen.GenerateMap();
            }
        }
    }

    private void DrawTitle()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Procedural Map Generator", titleStyle);

        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Perlin noise-based terrain generation", subtitleStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Generate button (large, prominent)
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Generate Map", GUILayout.Height(35)))
        {
            mapGen.GenerateMap();
            UpdateStatistics();
            RegeneratePreview();
        }
        GUI.backgroundColor = Color.white;

        // Random seed button
        if (GUILayout.Button("Random Seed", GUILayout.Height(35), GUILayout.Width(120)))
        {
            seedProp.intValue = Random.Range(0, 999999);
            serializedObject.ApplyModifiedProperties();
            if (mapGen.AutoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Regenerate Similar"))
        {
            RegenerateSimilar();
        }

        if (GUILayout.Button("Export as PNG"))
        {
            ExportMapAsPNG();
        }

        if (GUILayout.Button("Copy Settings"))
        {
            CopySettingsToClipboard();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawBasicSettings()
    {
        showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "Basic Settings");

        if (showBasicSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(useHexGridProp, new GUIContent("Use Hex Grid Size"));

            if (!useHexGridProp.boolValue)
            {
                EditorGUILayout.PropertyField(widthProp);
                EditorGUILayout.PropertyField(heightProp);

                int totalCells = widthProp.intValue * heightProp.intValue;
                EditorGUILayout.LabelField($"Total Cells: {totalCells:N0}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                if (mapGen.hexGrid != null)
                {
                    EditorGUILayout.LabelField($"Width: {mapGen.hexGrid.Width} (from HexGrid)");
                    EditorGUILayout.LabelField($"Height: {mapGen.hexGrid.Height} (from HexGrid)");
                }
                else
                {
                    EditorGUILayout.HelpBox("No HexGrid component found", MessageType.Warning);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(autoUpdateProp, new GUIContent("Auto Update"));
            EditorGUILayout.PropertyField(generateMapOnStartProp, new GUIContent("Generate On Start"));
            EditorGUILayout.PropertyField(useThreadedGenerationProp, new GUIContent("Use Threading"));

            if (!useThreadedGenerationProp.boolValue)
            {
                EditorGUILayout.HelpBox("Threading disabled. Generation will be slower but more stable in editor.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawNoiseSettings()
    {
        showNoiseSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showNoiseSettings, "Noise Settings");

        if (showNoiseSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Seed
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(seedProp);
            if (GUILayout.Button("🎲", GUILayout.Width(30)))
            {
                seedProp.intValue = Random.Range(0, 999999);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Scale with slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(noiseScaleProp);
            noiseScaleProp.floatValue = EditorGUILayout.Slider(noiseScaleProp.floatValue, 0.01f, 10f);
            EditorGUILayout.EndHorizontal();
            DrawParameterHint("Lower = zoomed out, Higher = zoomed in");

            // Octaves
            EditorGUILayout.PropertyField(octavesProp);
            DrawParameterHint($"More layers = more detail (current: {octavesProp.intValue})");

            // Persistance
            EditorGUILayout.PropertyField(persistanceProp);
            DrawParameterHint("Controls amplitude change between octaves");

            // Lacunarity
            EditorGUILayout.PropertyField(lacunarityProp);
            DrawParameterHint("Controls frequency change between octaves");

            // Offset
            EditorGUILayout.PropertyField(offsetProp);

            EditorGUILayout.Space(5);

            // Noise parameter presets
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Smooth"))
                ApplyNoisePreset(0.3f, 3, 0.5f, 2f);
            if (GUILayout.Button("Detailed"))
                ApplyNoisePreset(0.5f, 6, 0.5f, 2f);
            if (GUILayout.Button("Rough"))
                ApplyNoisePreset(0.8f, 8, 0.6f, 2.5f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Islands"))
                ApplyNoisePreset(0.4f, 5, 0.4f, 2.2f);
            if (GUILayout.Button("Continents"))
                ApplyNoisePreset(0.2f, 4, 0.5f, 2f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawBiomeSettings()
    {
        showBiomeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBiomeSettings, "Biome Distribution");

        if (showBiomeSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(biomesProp, new GUIContent("Biomes"), true);

            if (mapGen.Biomes.Count == 0)
            {
                EditorGUILayout.HelpBox("No biomes defined. Add at least one biome.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Biome Height Distribution:", EditorStyles.boldLabel);

                DrawBiomeDistributionBar();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Add Default Biomes"))
            {
                AddDefaultBiomes();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPreview()
    {
        showPreview = EditorGUILayout.BeginFoldoutHeaderGroup(showPreview, "Preview");

        if (showPreview)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (mapGen.colorMap != null && mapGen.colorMap.Length > 0)
            {
                // Preview controls
                EditorGUILayout.BeginHorizontal();
                showNoisePreview = EditorGUILayout.Toggle("Show Noise", showNoisePreview, GUILayout.Width(100));
                showColorPreview = EditorGUILayout.Toggle("Show Color", showColorPreview, GUILayout.Width(100));

                if (GUILayout.Button("Refresh Preview"))
                {
                    RegeneratePreview();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Octave preview slider
                if (showNoisePreview)
                {
                    EditorGUILayout.LabelField($"Preview Octave: {(previewOctave == -1 ? "All" : previewOctave.ToString())}");
                    int newOctave = EditorGUILayout.IntSlider(previewOctave, -1, mapGen.Octaves - 1);
                    if (newOctave != previewOctave)
                    {
                        previewOctave = newOctave;
                        RegenerateNoisePreview();
                    }
                }

                EditorGUILayout.Space(5);

                // Draw previews
                if (showNoisePreview && noisePreviewTexture != null)
                {
                    EditorGUILayout.LabelField("Noise Preview:", EditorStyles.boldLabel);
                    Rect noiseRect = GUILayoutUtility.GetRect(256, 256);
                    EditorGUI.DrawPreviewTexture(noiseRect, noisePreviewTexture);
                    EditorGUILayout.Space(5);
                }

                if (showColorPreview && previewTexture != null)
                {
                    EditorGUILayout.LabelField("Color Preview:", EditorStyles.boldLabel);
                    Rect colorRect = GUILayoutUtility.GetRect(256, 256);
                    EditorGUI.DrawPreviewTexture(colorRect, previewTexture);
                }

                // Seed comparison
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Seed Comparison:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                comparisonSeed = EditorGUILayout.IntField("Compare Seed:", comparisonSeed);
                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    GenerateComparison();
                }
                EditorGUILayout.EndHorizontal();

                if (comparisonTexture != null)
                {
                    EditorGUILayout.Space(5);
                    Rect compRect = GUILayoutUtility.GetRect(256, 256);
                    EditorGUI.DrawPreviewTexture(compRect, comparisonTexture);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Generate a map to see preview", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawStatistics()
    {
        showStatistics = EditorGUILayout.BeginFoldoutHeaderGroup(showStatistics, "Map Statistics");

        if (showStatistics)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (mapGen.terrainMap != null)
            {
                if (terrainPercentages == null || terrainPercentages.Count == 0)
                {
                    UpdateStatistics();
                }

                EditorGUILayout.LabelField("Terrain Distribution:", EditorStyles.boldLabel);

                if (terrainPercentages != null)
                {
                    foreach (var kvp in terrainPercentages.OrderByDescending(x => x.Value))
                    {
                        if (kvp.Key != null)
                        {
                            DrawTerrainStatBar(kvp.Key, kvp.Value, terrainCounts[kvp.Key]);
                        }
                    }
                }

                EditorGUILayout.Space(5);

                int totalCells = mapGen.Width * mapGen.Height;
                EditorGUILayout.LabelField($"Total Cells: {totalCells:N0}");
                EditorGUILayout.LabelField($"Seed: {mapGen.Seed}");
            }
            else
            {
                EditorGUILayout.HelpBox("Generate a map to see statistics", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPresets()
    {
        showPresets = EditorGUILayout.BeginFoldoutHeaderGroup(showPresets, "Save/Load Presets");

        if (showPresets)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.HelpBox("Save current settings as a preset ScriptableObject for reuse", MessageType.Info);

            if (GUILayout.Button("Save As Preset..."))
            {
                SaveAsPreset();
            }

            if (GUILayout.Button("Load From Preset..."))
            {
                LoadFromPreset();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // Helper Methods

    private void DrawParameterHint(string hint)
    {
        GUIStyle hintStyle = new GUIStyle(EditorStyles.miniLabel);
        hintStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        EditorGUILayout.LabelField(hint, hintStyle);
    }

    private void DrawBiomeDistributionBar()
    {
        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(30));

        float currentX = barRect.x;
        float previousHeight = 0f;

        foreach (var biome in mapGen.Biomes)
        {
            if (biome.TerrainType == null) continue;

            float width = (biome.Height - previousHeight) * barRect.width;

            Rect segmentRect = new Rect(currentX, barRect.y, width, barRect.height);
            EditorGUI.DrawRect(segmentRect, biome.TerrainType.Colour);

            currentX += width;
            previousHeight = biome.Height;
        }

        // Draw labels
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.normal.textColor = Color.white;

        currentX = barRect.x;
        previousHeight = 0f;

        foreach (var biome in mapGen.Biomes)
        {
            if (biome.TerrainType == null) continue;

            float width = (biome.Height - previousHeight) * barRect.width;
            Rect labelRect = new Rect(currentX + 5, barRect.y, width - 10, barRect.height);

            if (width > 40) // Only show label if segment is wide enough
            {
                GUI.Label(labelRect, $"{(biome.Height * 100):F0}%", labelStyle);
            }

            currentX += width;
            previousHeight = biome.Height;
        }
    }

    private void DrawTerrainStatBar(TerrainType terrain, float percentage, int count)
    {
        EditorGUILayout.BeginHorizontal();

        // Color indicator
        Rect colorRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
        EditorGUI.DrawRect(colorRect, terrain.Colour);

        // Name
        EditorGUILayout.LabelField(terrain.Name, GUILayout.Width(100));

        // Percentage bar
        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * (percentage / 100f), barRect.height);
        EditorGUI.DrawRect(fillRect, terrain.Colour);

        GUIStyle percentStyle = new GUIStyle(EditorStyles.miniLabel);
        percentStyle.alignment = TextAnchor.MiddleCenter;
        percentStyle.normal.textColor = Color.white;
        GUI.Label(barRect, $"{percentage:F1}% ({count:N0})", percentStyle);

        EditorGUILayout.EndHorizontal();
    }

    private void ApplyNoisePreset(float scale, int octaves, float persistance, float lacunarity)
    {
        noiseScaleProp.floatValue = scale;
        octavesProp.intValue = octaves;
        persistanceProp.floatValue = persistance;
        lacunarityProp.floatValue = lacunarity;

        serializedObject.ApplyModifiedProperties();

        if (mapGen.AutoUpdate)
        {
            mapGen.GenerateMap();
        }
    }

    private void AddDefaultBiomes()
    {
        // Clear existing
        biomesProp.ClearArray();

        // Add default biome distribution
        // This would need actual TerrainType references
        EditorGUILayout.HelpBox("Load TerrainTypes from your project and assign them manually", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    private void RegenerateSimilar()
    {
        // Change seed slightly
        seedProp.intValue = Random.Range(seedProp.intValue - 100, seedProp.intValue + 100);
        serializedObject.ApplyModifiedProperties();
        mapGen.GenerateMap();
        UpdateStatistics();
        RegeneratePreview();
    }

    private void UpdateStatistics()
    {
        if (mapGen.terrainMap == null) return;

        terrainCounts = new Dictionary<TerrainType, int>();
        terrainPercentages = new Dictionary<TerrainType, float>();

        int totalCells = mapGen.Width * mapGen.Height;

        // Count each terrain type
        for (int x = 0; x < mapGen.Width; x++)
        {
            for (int y = 0; y < mapGen.Height; y++)
            {
                TerrainType terrain = mapGen.terrainMap[x, y];
                if (terrain == null) continue;

                if (!terrainCounts.ContainsKey(terrain))
                {
                    terrainCounts[terrain] = 0;
                }
                terrainCounts[terrain]++;
            }
        }

        // Calculate percentages
        foreach (var kvp in terrainCounts)
        {
            terrainPercentages[kvp.Key] = (kvp.Value / (float)totalCells) * 100f;
        }
    }

    private void RegeneratePreview()
    {
        if (mapGen.colorMap == null || mapGen.colorMap.Length == 0) return;

        // Create color preview
        int width = Mathf.Min(mapGen.Width, 256);
        int height = Mathf.Min(mapGen.Height, 256);

        if (previewTexture == null || previewTexture.width != width || previewTexture.height != height)
        {
            previewTexture = new Texture2D(width, height);
        }

        // Sample the color map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sourceX = (int)((x / (float)width) * mapGen.Width);
                int sourceY = (int)((y / (float)height) * mapGen.Height);

                Color color = mapGen.colorMap[sourceY * mapGen.Width + sourceX];
                previewTexture.SetPixel(x, y, color);
            }
        }

        previewTexture.Apply();

        RegenerateNoisePreview();
    }

    private void RegenerateNoisePreview()
    {
        if (mapGen.noiseMap == null) return;

        int width = Mathf.Min(mapGen.Width, 256);
        int height = Mathf.Min(mapGen.Height, 256);

        if (noisePreviewTexture == null || noisePreviewTexture.width != width || noisePreviewTexture.height != height)
        {
            noisePreviewTexture = new Texture2D(width, height);
        }

        // Generate preview of specific octave or all
        float[,] previewNoise;

        if (previewOctave == -1)
        {
            previewNoise = mapGen.noiseMap;
        }
        else
        {
            // Generate noise for just one octave
            previewNoise = Noise.GenerateNoiseMap(
                mapGen.Width,
                mapGen.Height,
                mapGen.NoiseScale,
                mapGen.Seed,
                previewOctave + 1,
                mapGen.Persistance,
                mapGen.Lacunarity,
                mapGen.Offset
            );
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sourceX = (int)((x / (float)width) * mapGen.Width);
                int sourceY = (int)((y / (float)height) * mapGen.Height);

                float value = previewNoise[sourceX, sourceY];
                noisePreviewTexture.SetPixel(x, y, new Color(value, value, value));
            }
        }

        noisePreviewTexture.Apply();
    }

    private void GenerateComparison()
    {
        int originalSeed = mapGen.Seed;

        // Generate with comparison seed
        float[,] compNoise = Noise.GenerateNoiseMap(
            mapGen.Width,
            mapGen.Height,
            mapGen.NoiseScale,
            comparisonSeed,
            mapGen.Octaves,
            mapGen.Persistance,
            mapGen.Lacunarity,
            mapGen.Offset
        );

        // Create texture
        int width = Mathf.Min(mapGen.Width, 256);
        int height = Mathf.Min(mapGen.Height, 256);

        if (comparisonTexture == null || comparisonTexture.width != width || comparisonTexture.height != height)
        {
            comparisonTexture = new Texture2D(width, height);
        }

        // Apply same terrain mapping
        TerrainType[,] compTerrain = new TerrainType[mapGen.Width, mapGen.Height];
        for (int y = 0; y < mapGen.Height; y++)
        {
            for (int x = 0; x < mapGen.Width; x++)
            {
                float currentHeight = compNoise[x, y];
                foreach (var biome in mapGen.Biomes)
                {
                    if (currentHeight <= biome.Height)
                    {
                        compTerrain[x, y] = biome.TerrainType;
                        break;
                    }
                }
            }
        }

        // Create color map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sourceX = (int)((x / (float)width) * mapGen.Width);
                int sourceY = (int)((y / (float)height) * mapGen.Height);

                Color color = compTerrain[sourceX, sourceY]?.Colour ?? Color.black;
                comparisonTexture.SetPixel(x, y, color);
            }
        }

        comparisonTexture.Apply();
    }

    private void ExportMapAsPNG()
    {
        if (mapGen.colorMap == null || mapGen.colorMap.Length == 0)
        {
            EditorUtility.DisplayDialog("Export Failed", "Generate a map first", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Save Map as PNG",
            Application.dataPath,
            $"Map_Seed{mapGen.Seed}",
            "png"
        );

        if (string.IsNullOrEmpty(path)) return;

        Texture2D exportTexture = new Texture2D(mapGen.Width, mapGen.Height);

        for (int y = 0; y < mapGen.Height; y++)
        {
            for (int x = 0; x < mapGen.Width; x++)
            {
                Color color = mapGen.colorMap[y * mapGen.Width + x];
                exportTexture.SetPixel(x, y, color);
            }
        }

        byte[] bytes = exportTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        DestroyImmediate(exportTexture);

        Debug.Log($"Map exported to: {path}");
        EditorUtility.DisplayDialog("Export Complete", $"Map saved to:\n{path}", "OK");
    }

    private void CopySettingsToClipboard()
    {
        string settings = $"Seed: {mapGen.Seed}\n" +
                         $"Scale: {mapGen.NoiseScale}\n" +
                         $"Octaves: {mapGen.Octaves}\n" +
                         $"Persistance: {mapGen.Persistance}\n" +
                         $"Lacunarity: {mapGen.Lacunarity}\n" +
                         $"Offset: {mapGen.Offset}\n" +
                         $"Size: {mapGen.Width}x{mapGen.Height}";

        EditorGUIUtility.systemCopyBuffer = settings;
        Debug.Log("Settings copied to clipboard:\n" + settings);
    }

    private void SaveAsPreset()
    {
        // This would create a MapGeneratorPreset ScriptableObject
        EditorUtility.DisplayDialog("Feature Coming Soon",
            "Preset saving will be implemented in MapGeneratorPreset ScriptableObject",
            "OK");
    }

    private void LoadFromPreset()
    {
        EditorUtility.DisplayDialog("Feature Coming Soon",
            "Preset loading will be implemented in MapGeneratorPreset ScriptableObject",
            "OK");
    }
}
