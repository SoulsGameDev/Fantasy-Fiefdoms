using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapDisplay))]
public class MapDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (DrawDefaultInspector())
        {
            MapDisplay mapDisplay = (MapDisplay)target;

            mapDisplay.SubscribeToEvents();
        }
    }
}
