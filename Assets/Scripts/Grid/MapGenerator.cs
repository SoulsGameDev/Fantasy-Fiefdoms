using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public HexGrid hexGrid;
    public float NoiseScale = 0.5f;
    public int Octaves = 6;
    public float Persistance = 0.5f;
    public float Lacunarity = 2f;
    public int Seed = 0;
    public Vector2 Offset = Vector2.zero;
    public bool AutoUpdate = true;
    private void Awake()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(hexGrid.Width, hexGrid.Height, NoiseScale, Seed,  Octaves, Persistance, Lacunarity, Offset);
        
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }

}
