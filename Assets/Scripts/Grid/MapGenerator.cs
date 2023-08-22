using Codice.Client.BaseCommands.Changelist;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator: MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh, Models };

    public HexGrid hexGrid;
    public DrawMode drawMode;
    
    public int Width = 256;
    public int Height = 256;
    [Tooltip("The scale of the noise map. The higher the scale, the more zoomed in the noise map will be. " +
               "The lower the scale, the more zoomed out the noise map will be. " +
               "The scale should be greater than 0.")]
    public float NoiseScale = 0.5f;
    [Tooltip("The number of layers of noise to generate. More layers means more detail in the noise map.")]
    public int Octaves = 6;
    [Range(0, 1)]
    [Tooltip("The change in amplitude between octaves. The amplitude of each octave is multiplied by this value.")]
    public float Persistance = 0.5f;
    [Tooltip("The change in frequency between octaves. The frequency of each octave is multiplied by this value.")]
    public float Lacunarity = 2f;
    [Tooltip("The seed used to generate the noise map.")]
    public int Seed = 0;
    [Tooltip("The offset of the noise map.")]
    public Vector2 Offset = Vector2.zero;
    [Tooltip("Whether or not to automatically update the noise map when a value is changed.")]
    public bool AutoUpdate = true;
    [Tooltip("Whether or not to use the hex grid width and height information to generate the noise map.")]
    public bool UseHexGrid = true;

    public List<TerrainHeight> Biomes = new List<TerrainHeight>();

    private void Awake()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    public void GenerateMap()
    {
        if(UseHexGrid && (hexGrid != null))
        {
            Width = hexGrid.Width;
            Height = hexGrid.Height;
        }

        ValidateSettings();

        float[,] noiseMap = Noise.GenerateNoiseMap(Width, Height, NoiseScale, Seed,  Octaves, Persistance, Lacunarity, Offset);
        Color[] colorMap = new Color[Width * Height];
        for(int y = 0; y < Height; y++)
        {
            for(int x = 0; x < Width; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < Biomes.Count; i++)
                {
                    if(currentHeight <= Biomes[i].Height)
                    {
                        colorMap[y * Width + x] = Biomes[i].TerrainType.Colour;
                        break;
                    }
                }
            }
        }


        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
 
        if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if(drawMode == DrawMode.ColourMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colorMap, Width, Height));
        else if(drawMode == DrawMode.Mesh)
            //mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colorMap, Width, Height));
            Debug.Log("Draw Mesh");
        else if(drawMode == DrawMode.Models)
            //mapDisplay.DrawModels(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colorMap, Width, Height));
            Debug.Log("Draw Models");
    }

    private void ValidateSettings()
    {
        // We make sure the octaves is not less than 0
        Octaves = Mathf.Max(Octaves, 0);
        // We make sure the lacunarity is not less than 1
        Lacunarity = Mathf.Max(Lacunarity, 1);
        // We make sure the persistance is between 0 and 1
        Persistance = Mathf.Clamp01(Persistance);
        // We make sure the scale is not 0 because we will be dividing by it
        NoiseScale = Mathf.Max(NoiseScale, 0.0001f);
        // Make sure the width and height are not less than 1

        Width = Mathf.Max(Width, 1);
        Height = Mathf.Max(Height, 1);
    }

    private void OnValidate()
    {
        ValidateSettings();
    }

}

[System.Serializable]
public struct TerrainHeight
{
    public float Height;
    public TerrainType TerrainType;
}