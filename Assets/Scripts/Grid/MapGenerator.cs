using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public HexGrid hexGrid;
    public float NoiseScale = 0.5f;
    public bool AutoUpdate = true;
    private void Awake()
    {
        hexGrid = GetComponent<HexGrid>();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(hexGrid.Width, hexGrid.Height, NoiseScale);
        
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }

}
