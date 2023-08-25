using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LoadingUI : MonoBehaviour
{
    public HexGrid grid;
    public TextMeshProUGUI text;

    

    private void OnEnable()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = "Loading...";
        grid = FindObjectOfType<HexGrid>();
        grid.OnMapInfoGenerated += OnMapCalculated;
        grid.OnCellInstancesGenerated += OnCellInstancesGenerated;
        grid.OnCellBatchGenerated += OnCellBatchGenerated;
    }


    private void OnDisable()
    {
        grid.OnMapInfoGenerated -= OnMapCalculated;
        grid.OnCellInstancesGenerated -= OnCellInstancesGenerated;
        grid.OnCellBatchGenerated -= OnCellBatchGenerated;
    }

    private void OnMapCalculated()
    {
        text.text = "Generated Map...";
    }


    private void OnCellBatchGenerated(float obj)
    {
        text.text = $"Loading... {Mathf.Round(obj * 10000)/100}%";
    }

    private void OnCellInstancesGenerated()
    {
        text.text = "Loading Complete";
    }
}
