using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Debug.Log("Generating texture from colour map");
        Texture2D texture = new Texture2D(width, height);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        Debug.Log("Generating texture from height map");
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // We create a new colour map
        Color[] colourMap = new Color[width * height];

        // We loop through the height map
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
                // We set the colour map to the colour of the height map
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
        }

        // We return the texture from the colour map
        return TextureFromColourMap(colourMap, width, height);
    }
}
