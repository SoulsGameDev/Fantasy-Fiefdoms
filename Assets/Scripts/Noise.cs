using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        // We create a random number generator with the seed
        System.Random prng = new System.Random(seed);

        // We create an array of random offsets for each octave
        // Without this, the noise will look the same for each octave (because the noise is based on the x and y coordinates, which are the same for each octave)
        // We use a Vector2 because we need an x and y offset
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            // We generate a random number between -100000 and 100000 for each octave
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            // We set the octave offset to the random number
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // We make sure the scale is not 0 because we will be dividing by it
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // We keep track of the max and min noise heights so we can normalize the noise map later
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Move the center of the map to the center of the game object (where noise starts from)
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                // We set the amplitude and frequency to 1 for the first octave at each point on the map
                float amplitude = 1;
                // We set the frequency to 1 for the first octave at each point on the map
                float frequency = 1;
                // We set the noise height to 0 for the first octave at each point on the map
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    // We multiply the x and y coordinates by the scale to make the noise more pronounced (the higher the scale, the more zoomed in the noise will be)
                    // We also multiply by the frequency to make the noise scale with each octave (the higher the frequency, the more zoomed out the noise will be)
                    // We add the octave offset to make the noise different for each octave
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    // PerlinNoise returns a value between 0 and 1, so we multiply by 2 and subtract 1 to get a value between -1 and 1
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //Add the perlin value to the noise height for this octave (the amplitude is multiplied by the perlin value to control how pronounced each octave is)
                    noiseHeight += perlinValue * amplitude;

                    // We multiply the amplitude by the persistance to make each octave less pronounced than the last
                    amplitude *= persistance;
                    // We multiply the frequency by the lacunarity to make each octave more zoomed out than the last
                    frequency *= lacunarity;
                }

                // We update the max and min noise heights if necessary
                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                // We set the noise map at this point to the noise height
                noiseMap[x, y] = noiseHeight;

            }
        }

        // We normalize the noise map
        NormalizeMap(noiseMap, maxNoiseHeight, minNoiseHeight);

        return noiseMap;
    }

    public static void NormalizeMap(float[,] map, float maxHeight, float minHeight)
    {
        for(int y = 0; y < map.GetLength(1); y++)
        {
            for(int x = 0; x < map.GetLength(0); x++)
            {
                // We normalize the noise map by subtracting the min height from each point and dividing by the difference between the max and min heights
                map[x, y] = Mathf.InverseLerp(minHeight, maxHeight, map[x, y]);
            }
        }
    }

}
