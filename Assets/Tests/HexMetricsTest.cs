/*using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HexMetricsTest
{

    //----------Axial Cube Conversion----------
    [Test]
    public void AxialToCubeConversionTest_Origin()
    {
        int q = 0;
        int r = 0;
        int s = 0;
        Vector3 expected = new Vector3(0, 0, 0);
        Vector3 actual = HexMetrics.AxialToCube(q, r);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AxialToCubeConversionTest_PP()
    {
        int q = 2;
        int r = 1;

        Vector3 expected = new Vector3(2, 1, -3);
        Vector3 actual = HexMetrics.AxialToCube(q, r);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AxialToCubeConversionTest_PM()
    {
        int q = 2;
        int r = -1;

        Vector3 expected = new Vector3(2, -1, -1);
        Vector3 actual = HexMetrics.AxialToCube(q, r);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AxialToCubeConversionTest_MM()
    {
        int q = -2;
        int r = -1;

        Vector3 expected = new Vector3(-2, -1, 3);
        Vector3 actual = HexMetrics.AxialToCube(q, r);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AxialToCubeConversionTest_MP()
    {
        int q = -2;
        int r = 1;

        Vector3 expected = new Vector3(-2, 1, 1);
        Vector3 actual = HexMetrics.AxialToCube(q, r);

        Assert.AreEqual(expected, actual);
    }

    //----------Cube Axial Conversion----------

    [Test]
    public void CubeToAxialConversionTest_Origin()
    {
        Vector3 cube = new Vector3(0, 0, 0);

        Vector2 expected = new Vector2(0, 0);
        Vector2 actual = HexMetrics.CubeToAxial(cube);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToAxialConversionTest_PP()
    {
        Vector3 cube = new Vector3(2, 1, -3);

        Vector2 expected = new Vector2(2, 1);
        Vector2 actual = HexMetrics.CubeToAxial(cube);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToAxialConversionTest_PM()
    {
        Vector3 cube = new Vector3(2, -1, -1);

        Vector2 expected = new Vector2(2, -1);
        Vector2 actual = HexMetrics.CubeToAxial(cube);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToAxialConversionTest_MM()
    {
        Vector3 cube = new Vector3(-2, -1, 3);

        Vector2 expected = new Vector2(-2, -1);
        Vector2 actual = HexMetrics.CubeToAxial(cube);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToAxialConversionTest_MP()
    {
        Vector3 cube = new Vector3(-2, 1, 1);

        Vector2 expected = new Vector2(-2, 1);
        Vector2 actual = HexMetrics.CubeToAxial(cube);

        Assert.AreEqual(expected, actual);
    }

    //----------Offset Cube Conversion----------
    [Test]
    public void OffsetToCubeConversionTest_FT_Origin()
    {
        int offsetRow = 0;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(0, 0, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_PT_Origin()
    {
        int offsetRow = 0;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(0, 0, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    //----------Pointy Top----------

    [Test]
    public void OffsetToCubeConversionTest_PT_RowPlus()
    {
        int offsetRow = 1;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(0, 1, -1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_PT_RowMinus()
    {
        int offsetRow = -1;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(1, -1, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void OffsetToCubeConversionTest_PT_ColMinus()
    {
        int offsetRow = 0;
        int offsetCol = -1;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(-1, 0, 1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_PT_ColPlus()
    {
        int offsetRow = -0;
        int offsetCol = 1;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(1, 0, -1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_PT_ColMinusRowUp()
    {
        int offsetRow = 1;
        int offsetCol = -1;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(-1, 1, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_PT_ColMinusRowMinus()
    {
        int offsetRow = -1;
        int offsetCol = -1;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(0, -1, 1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }
    //--------------Flat Top---------------
    [Test]
    public void OffsetToCubeConversionTest_FT_RowPlus()
    {
        int offsetRow = 1;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(0, 1, -1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_FT_RowMinus()
    {
        int offsetRow = -1;
        int offsetCol = 0;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(0, -1, 1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void OffsetToCubeConversionTest_FT_ColMinus()
    {
        int offsetRow = 0;
        int offsetCol = -1;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(-1, 1, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_FT_ColPlus()
    {
        int offsetRow = -0;
        int offsetCol = 1;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector3 expected = new Vector3(1, 0, -1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_FT_ColMinusRowMinus()
    {
        int offsetRow = -1;
        int offsetCol = -1;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(-1, 0, 1);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OffsetToCubeConversionTest_FT_ColPlusRowMinus()
    {
        int offsetRow = -1;
        int offsetCol = 1;
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector3 expected = new Vector3(1, -1, 0);
        Vector3 actual = HexMetrics.OffsetToCube(offsetCol, offsetRow, orientation);

        Assert.AreEqual(expected, actual);
    }

    //--------------Cube to Offset---------------

    [Test]
    public void CubeToOffsetConversionTest_FT_Origin()
    {
        Vector3 cube = new Vector3(0, 0, 0);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(0, 0);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_PT_Origin()
    {
        Vector3 cube = new Vector3(0, 0, 0);
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(0, 0);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_RowPlus()
    {
        Vector3 cube = new Vector3(0, 1, -1);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(0, 1);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_RowMinus()
    {
        Vector3 cube = new Vector3(0, -1, 1);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(0, -1);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_ColMinus()
    {
        Vector3 cube = new Vector3(-1, 1, 0);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(-1, 0);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_ColPlus()
    {
        Vector3 cube = new Vector3(1, 0, -1);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(1, 0);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_ColMinusRowMinus()
    {
        Vector3 cube = new Vector3(-1, 0, 1);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(-1, -1);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CubeToOffsetConversionTest_FT_ColPlusRowMinus()
    {
        Vector3 cube = new Vector3(1, -1, 0);
        HexOrientation orientation = HexOrientation.FlatTop;

        Vector2 expected = new Vector2(1, -1);
        Vector2 actual = HexMetrics.CubeToOffset(cube, orientation);

        Assert.AreEqual(expected, actual);
    }

    //--------------World To Offset---------------
    [Test]
    public void WorldToOffsetConversionTest_PT_Origin()
    {
        float worldX = 0f;
        float worldZ = 0f;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(0, 0);
        Vector2 actual = HexMetrics.CoordinateToOffset(worldX, worldZ, 5f, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WorldToOffsetConversionTest_PT_RowPlus()
    {
        float worldX = 4f;
        float worldZ = 7f;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(0, 1);
        Vector2 actual = HexMetrics.CoordinateToOffset(worldX, worldZ, 5f, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WorldToOffsetConversionTest_PT_RowMinus()
    {
        float worldX = 4f;
        float worldZ = -7f;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(0, -1);
        Vector2 actual = HexMetrics.CoordinateToOffset(worldX, worldZ, 5f, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WorldToOffsetConversionTest_PT_ColMinus()
    {
        float worldX = -9f;
        float worldZ = 0f;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(-1, 0);
        Vector2 actual = HexMetrics.CoordinateToOffset(worldX, worldZ, 5f, orientation);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WorldToOffsetConversionTest_PT_ColPlus()
    {
        float worldX = 9f;
        float worldZ = 0f;
        HexOrientation orientation = HexOrientation.PointyTop;

        Vector2 expected = new Vector2(1, 0);
        Vector2 actual = HexMetrics.CubeToOffset(HexMetrics.AxialToCube(HexMetrics.CoordinateToAxial(worldX, worldZ, 5f, orientation)), orientation);

        Assert.AreEqual(expected, actual);
    }

}
*/