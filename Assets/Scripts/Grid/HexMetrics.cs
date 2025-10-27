using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics
{
    // Coordinate conversion caches
    private static Dictionary<(int, int, HexOrientation), Vector3> offsetToCubeCache =
        new Dictionary<(int, int, HexOrientation), Vector3>();
    private static Dictionary<(Vector3, HexOrientation), Vector2> cubeToOffsetCache =
        new Dictionary<(Vector3, HexOrientation), Vector2>();
    private static Dictionary<(float, float, float, HexOrientation), Vector2> coordinateToAxialCache =
        new Dictionary<(float, float, float, HexOrientation), Vector2>();

    /// <summary>
    /// Clear all coordinate conversion caches
    /// </summary>
    public static void ClearConversionCaches()
    {
        offsetToCubeCache.Clear();
        cubeToOffsetCache.Clear();
        coordinateToAxialCache.Clear();
    }

    /// <summary>
    /// Gets the outer radius of the hexagon.
    /// </summary>
    /// <param name="hexSize"></param>
    /// <returns></returns>
    public static float OuterRadius (float hexSize)
    {
        return hexSize;
    }

    /// <summary>
    /// Gets the inner radius of the hexagon.
    /// </summary>
    /// <param name="hexSize"></param>
    /// <returns></returns>
    public static float InnerRadius (float hexSize)
    {
        return hexSize * 0.866025404f;
    }

    /// <summary>
    /// Get all the corners of the hexagon.
    /// </summary>
    /// <param name="hexSize"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public static Vector3[] Corners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, orientation, i);
        }
        return corners;
    }

    /// <summary>
    /// Get a specific corner of the hexagon.
    /// </summary>
    /// <param name="hexSize"></param>
    /// <param name="orientation"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Vector3 Corner(float hexSize, HexOrientation orientation, int index)
    {
        float angle = 60f * -index;
        if (orientation == HexOrientation.PointyTop)
        {
            angle -= 30f;
        }
        Vector3 corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad), 
            0f, 
            hexSize * Mathf.Sin(angle * Mathf.Deg2Rad)
            );
        return corner;
    }

    /// <summary>
    /// Get the center of the hexagon.
    /// </summary>
    /// <param name="hexSize">Size of individual tile</param>
    /// <param name="x">column position</param>
    /// <param name="z">row position</param>
    /// <param name="orientation">Hex orientation</param>
    /// <returns></returns>
    public static Vector3 Center(float hexSize, int x, int z, HexOrientation orientation)
    {
        Vector3 centrePosition;
        if (orientation == HexOrientation.PointyTop)
        {
            centrePosition.x = (x + z * 0.5f - z / 2) * (InnerRadius(hexSize) * 2f);
            centrePosition.y = 0f;
            centrePosition.z = z * (OuterRadius(hexSize) * 1.5f);
        }
        else
        {
            centrePosition.x = (x) * (OuterRadius(hexSize) * 1.5f);
            centrePosition.y = 0f;
            centrePosition.z = (z + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f);
        }
        return centrePosition;
    }

    /*----------Coordinate Conversions-----------*/
    /// <summary>
    /// Converts Offset coordinates to Cube coordinates.
    /// Cube coordinates are used for simplified calculations.
    /// The q, r, s values of the cube coordinates always add up to 0.
    /// Q is the x-axis, R is the y-axis, S is the z-axis.
    /// </summary>
    /// <param name="offsetCoord">Column and Row Offset Coordinate</param>
    /// <param name="orientation">Hex Orientation</param>
    /// <returns></returns>
    public static Vector3 OffsetToCube(Vector2 offsetCoord, HexOrientation orientation)
    {
        return OffsetToCube((int)offsetCoord.x, (int)offsetCoord.y, orientation);
    }

    /// <summary>
    /// Converts Offset coordinates to Cube coordinates.
    /// Cube coordinates are used for simplified calculations.
    /// The q, r, s values of the cube coordinates always add up to 0.
    /// Q is the x-axis, R is the y-axis, S is the z-axis.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public static Vector3 OffsetToCube(int col, int row, HexOrientation orientation)
    {
        var key = (col, row, orientation);

        // Check cache
        if (offsetToCubeCache.TryGetValue(key, out Vector3 cachedResult))
        {
            return cachedResult;
        }

        // Calculate
        Vector3 result;
        if (orientation == HexOrientation.PointyTop)
        {
            result = AxialToCube(OffsetToAxialPointy(col, row));
        }
        else
        {
            result = AxialToCube(OffsetToAxialFlat(col, row));
        }

        // Cache and return
        offsetToCubeCache[key] = result;
        return result;
    }

    /// <summary>
    /// Converts Offset coordinates to Axial coordinates.
    /// Axial coordinates are used for simplified calculations.
    /// Axial coordinates loose the S value of the cube coordinates.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Vector3 AxialToCube(float q, float r)
    {
        return new Vector3(q, r, -q - r);
    }

    /// <summary>
    /// Converts Offset coordinates to Axial coordinates.
    /// Axial coordinates are used for simplified calculations.
    /// Axial coordinates loose the S value of the cube coordinates.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Vector3 AxialToCube(int q, int r)
    {
        return new Vector3(q, r, -q - r);
    }

    /// <summary>
    /// Converts Offset coordinates to Axial coordinates.
    /// Axial coordinates are used for simplified calculations.
    /// Axial coordinates loose the S value of the cube coordinates.
    /// </summary>
    /// <param name="axialCoord"></param>
    /// <returns></returns>
    public static Vector3 AxialToCube(Vector2 axialCoord)
    {
        return AxialToCube(axialCoord.x, axialCoord.y);
    }

    /// <summary>
    /// Converts Cube coordinates to Axial coordinates.
    /// Cube coordianates calculate the S value from the Q and R values.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="r"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Vector2 CubeToAxial(int q, int r, int s)
    {
        return new Vector2(q, r);
    }

    /// <summary>
    /// Converts Cube coordinates to Axial coordinates.
    /// Cube coordianates calculate the S value from the Q and R values.
    /// </summary> 
    /// <param name="q"></param>
    /// <param name="r"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Vector2 CubeToAxial(float q, float r, float s)
    {
        return new Vector2(q, r);
    }
    /// <summary>
    /// Converts Cube coordinates to Axial coordinates.
    /// Cube coordianates calculate the S value from the Q and R values.
    /// </summary>
    /// <param name="cube"></param>
    public static Vector2 CubeToAxial(Vector3 cube)
    {
        return new Vector2(cube.x, cube.y);
    }

    /// <summary>
    /// Converts Offset coordinates to Axial coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public static Vector2 OffsetToAxail(int x, int z, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return OffsetToAxialPointy(x, z);
        }
        else
        {
            return OffsetToAxialFlat(x, z);
        }
    }

    /// <summary>
    /// Converts Offset coordinates to Axial coordinates for a flat orientation.
    /// Following the odd-q layout.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private static Vector2 OffsetToAxialFlat(int col, int row)
    {
        var q = col;
        var r = row - (col - (col & 1)) / 2;
        return new Vector2(q, r);
    }


    /// <summary>
    /// Converts Offset coordinates to Axial coordinates for a pointy orientation.
    /// Following the odd-r layout.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private static Vector2 OffsetToAxialPointy(int col, int row)
    {
        var q = col - (row - (row & 1)) / 2;
        var r = row;
        return new Vector2(q, r);
    }

    /// <summary>
    /// Converts Cube coordinates to Offset coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public static Vector2 CubeToOffset(int x, int y, int z, HexOrientation orientation)
    {
        Vector3 cubeCoord = new Vector3(x, y, z);
        var key = (cubeCoord, orientation);

        // Check cache
        if (cubeToOffsetCache.TryGetValue(key, out Vector2 cachedResult))
        {
            return cachedResult;
        }

        // Calculate
        Vector2 result;
        if (orientation == HexOrientation.PointyTop)
        {
            result = CubeToOffsetPointy(x, y, z);
        }
        else
        {
            result = CubeToOffsetFlat(x, y, z);
        }

        // Cache and return
        cubeToOffsetCache[key] = result;
        return result;
    }

    /// <summary>
    /// Converts Cube coordinates to Offset coordinates.
    /// </summary>
    public static Vector2 CubeToOffset(Vector3 offsetCoord, HexOrientation orientation)
    {
        return CubeToOffset((int)offsetCoord.x, (int)offsetCoord.y, (int)offsetCoord.z, orientation);
    }

    /// <summary>
    /// Converts Cube coordinates to Offset coordinates for a pointy orientation.
    /// Following the odd-r layout.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Vector2 CubeToOffsetPointy(int x, int y, int z)
    {
        Vector2 offsetCoordinates = new Vector2(x + (y - (y & 1)) / 2, y);
        return offsetCoordinates;
    }
    /// <summary>
    /// Converts Cube coordinates to Offset coordinates for a flat orientation.
    /// Following the odd-q layout.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Vector2 CubeToOffsetFlat(int x, int y, int z)
    {
        Vector2 offsetCoordinates = new Vector2(x, y + (x - (x & 1)) / 2);
        return offsetCoordinates;
    }


    /// <summary>
    /// Rounds the cube coordinates to the nearest hexagon center.
    /// Used for getting the nearest hexagon to a point in space.
    /// </summary>
    /// <param name="frac"> 
    /// Frac is the fractional cube coordinates.
    /// </param>
    /// <returns></returns>
    private static Vector3 CubeRound(Vector3 frac)
    {
        Vector3 roundedCoordinates = new Vector3();
        int rx = Mathf.RoundToInt(frac.x);
        int ry = Mathf.RoundToInt(frac.y);
        int rz = Mathf.RoundToInt(frac.z);
        float xDiff = Mathf.Abs(rx - frac.x);
        float yDiff = Mathf.Abs(ry - frac.y);
        float zDiff = Mathf.Abs(rz - frac.z);
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }
        roundedCoordinates.x = rx;
        roundedCoordinates.y = ry;
        roundedCoordinates.z = rz;
        return roundedCoordinates;
    }

    /// <summary>
    /// Rounds the axial coordinates to the nearest hexagon center.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public static Vector2 AxialRound(Vector2 coordinates)
    {
        return CubeToAxial(CubeRound(AxialToCube(coordinates.x, coordinates.y)));
    }

    /// <summary>
    /// Converts a point in space to the nearest hexagon center
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="hexSize"></param>
    /// <param name="orientation"></param>
    /// <returns>Axial Coordiante</returns>
    public static Vector2 CoordinateToAxial(float x, float z, float hexSize, HexOrientation orientation)
    {
        var key = (x, z, hexSize, orientation);

        // Check cache
        if (coordinateToAxialCache.TryGetValue(key, out Vector2 cachedResult))
        {
            return cachedResult;
        }

        // Calculate
        Vector2 result;
        if (orientation == HexOrientation.PointyTop)
        {
            result = CoordinateToPointyAxial(x, z, hexSize);
        }
        else
        {
            result = CoordinateToFlatAxial(x, z, hexSize);
        }

        // Cache and return
        coordinateToAxialCache[key] = result;
        return result;
    }

    /// <summary>
    /// Helper function for CoordinateToAxial.
    /// It gets a fractional axial coordinate from a point in space for a pointy top orientation.
    /// </summary>
    private static Vector2 CoordinateToPointyAxial(float x, float z, float hexSize)
    {
        Vector2 pointyHexCoordinates = new Vector2();
        pointyHexCoordinates.x = (Mathf.Sqrt(3) / 3 * x - 1f / 3 * z) / hexSize;
        pointyHexCoordinates.y = (2f / 3 * z) / hexSize;

        return AxialRound(pointyHexCoordinates);
    }

    /// <summary>
    /// Helper function for CoordinateToAxial.
    /// It gets a fractional axial coordinate from a point in space for a flat top orientation.
    /// </summary>
    private static Vector2 CoordinateToFlatAxial(float x, float z, float hexSize)
    {
        Vector2 flatHexCoordinates = new Vector2();
        flatHexCoordinates.x = (2f / 3 * x) / hexSize;
        flatHexCoordinates.y = (-1f / 3 * x + Mathf.Sqrt(3) / 3 * z) / hexSize;
        return AxialRound(flatHexCoordinates);
    }

    /// <summary>
    /// Converts a point in space to the nearest hexagon center
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="hexSize"></param>
    /// <param name="orientation"></param>
    /// <returns>Offset coordinate</returns>
    public static Vector2 CoordinateToOffset(float x, float z, float hexSize, HexOrientation orientation)
    {
        return CubeToOffset(AxialToCube(CoordinateToAxial(x, z, hexSize, orientation)), orientation);
    }

    /// <summary>
    /// Get the axial coordinates of all neighbors for a given cell
    /// </summary>
    /// <param name="axialCoordinates">The axial coordinates of the cell</param>
    /// <param name="orientation">The orientation of the hex grid</param>
    /// <returns>List of neighbor axial coordinates</returns>
    public static List<Vector2> GetNeighbourCoordinatesList(Vector2 axialCoordinates, HexOrientation orientation = HexOrientation.PointyTop)
    {
        // Axial coordinate neighbor offsets
        // For pointy-top hexagons
        Vector2[] pointyOffsets = new Vector2[]
        {
            new Vector2(+1, 0),  // East
            new Vector2(+1, -1), // Northeast
            new Vector2(0, -1),  // Northwest
            new Vector2(-1, 0),  // West
            new Vector2(-1, +1), // Southwest
            new Vector2(0, +1)   // Southeast
        };

        // For flat-top hexagons
        Vector2[] flatOffsets = new Vector2[]
        {
            new Vector2(+1, 0),  // Southeast
            new Vector2(+1, -1), // Northeast
            new Vector2(0, -1),  // North
            new Vector2(-1, 0),  // Northwest
            new Vector2(-1, +1), // Southwest
            new Vector2(0, +1)   // South
        };

        Vector2[] offsets = (orientation == HexOrientation.PointyTop) ? pointyOffsets : flatOffsets;
        List<Vector2> neighbors = new List<Vector2>(6);

        foreach (Vector2 offset in offsets)
        {
            neighbors.Add(axialCoordinates + offset);
        }

        return neighbors;
    }
}
