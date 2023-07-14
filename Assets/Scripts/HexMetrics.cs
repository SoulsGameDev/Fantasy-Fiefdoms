using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics
{
    public static float OuterRadius (float hexSize)
    {
        return hexSize;
    }

    public static float InnerRadius (float hexSize)
    {
        return hexSize * 0.866025404f;
    }

    public static Vector3[] Corners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, orientation, i);
        }
        return corners;
    }

    public static Vector3 Corner(float hexSize, HexOrientation orientation, int index)
    {
        float angle = 60f * index;
        if (orientation == HexOrientation.PointyTop)
        {
            angle += 30f;
        }
        Vector3 corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad), 0f, hexSize * Mathf.Sin(angle * Mathf.Deg2Rad));
        return corner;
    }

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

    public static Vector3 OffsetToCube(int x, int z, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return OffsetToCubePointy(x, z);
        }
        else
        {
            return OffsetToCubeFlat(x, z);
        }
    }

    private static Vector3 OffsetToCubePointy(int x, int z)
    {
        Vector3 cubeCoordinates = new Vector3(x - (z - (z & 1)) / 2, -x - (z - (z & 1)) / 2, z);
        return cubeCoordinates;
    }

    private static Vector3 OffsetToCubeFlat(int x, int z)
    {
        Vector3 cubeCoordinates = new Vector3(x, -x - z, z);
        return cubeCoordinates;
    }

    public static Vector2 CubeToOffset(int x, int y, int z, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return CubeToOffsetPointy(x, y, z);
        }
        else
        {
            return CubeToOffsetFlat(x, y, z);
        }
    }

    private static Vector2Int CubeToOffsetPointy(int x, int y, int z)
    {
        Vector2Int offsetCoordinates = new Vector2Int(x + (z - (z & 1)) / 2, z);
        return offsetCoordinates;
    }

    private static Vector2 CubeToOffsetFlat(int x, int y, int z)
    {
        Vector2 offsetCoordinates = new Vector2(x, z + (x - (x & 1)) / 2);
        return offsetCoordinates;
    }

    public static Vector2 CubeToAxial(int x, int y, int z)
    {
        Vector2 axialCoordinates = new Vector2(x, z);
        return axialCoordinates;
    }

    public static Vector2 CubeToAxial(Vector3 cube)
    {
        Vector2 axialCoordinates = new Vector2(cube.x, cube.z);
        return axialCoordinates;
    }

    public static Vector3 AxialToCube(int x, int z)
    {
        Vector3 cubeCoordinates = new Vector3(x, -x - z, z);
        return cubeCoordinates;
    }

    public static Vector3 AxialToCube(float x, float z)
    {
        Vector3 cubeCoordinates = new Vector3(x, -x - z, z);
        return cubeCoordinates;
    }

    public static Vector3 AxialToCube(Vector2 axial)
    {
        Vector3 cubeCoordinates = new Vector3(axial.x, -axial.x - axial.y, axial.y);
        return cubeCoordinates;
    }

    public static Vector3 CubeRound(Vector3 frac)
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
        roundedCoordinates.y = rz;
        roundedCoordinates.z = ry;
        return roundedCoordinates;
    }

    public static Vector2 AxialRound(Vector2 coordinates)
    {
        return CubeToAxial(CubeRound(AxialToCube(coordinates.x, coordinates.y)));
    }

    public static Vector2 CoordinateToHex(float x, float z, float hexSize, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return CoordinateToPointyHex(x, z, hexSize);
        }
        else
        {
            return CoordinateToFlatHex(x, z, hexSize);
        }
    }

    private static Vector2 CoordinateToPointyHex(float x, float z, float hexSize)
    {
        Vector2 pointyHexCoordinates = new Vector2();
        /*
        pointyHexCoordinates.x = hexSize * Mathf.Sqrt(3) * (x + z / 2f);
        pointyHexCoordinates.y = hexSize * 3f / 2f * z;
        */
        pointyHexCoordinates.x = (Mathf.Sqrt(3) / 3 * x + -1f / 3 * z) / hexSize;
        pointyHexCoordinates.y = (0f * x + 2f / 3 * z) / hexSize;

        return AxialRound(pointyHexCoordinates);
    }

    private static Vector2 CoordinateToFlatHex(float x, float z, float hexSize)
    {
        Vector2 flatHexCoordinates = new Vector2();
        /*
        flatHexCoordinates.x = hexSize * 3f / 2f * x;
        flatHexCoordinates.y = hexSize * Mathf.Sqrt(3) * (z + x / 2f);
        */
        flatHexCoordinates.x = (2f / 3 * x) / hexSize;
        flatHexCoordinates.y = (-1f / 3 * x + Mathf.Sqrt(3) / 3 * z) / hexSize;
        return AxialRound(flatHexCoordinates);
    }
}
