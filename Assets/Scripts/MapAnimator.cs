using UnityEngine;
using System.Collections;
using System;

public class MapAnimator : MonoBehaviour
{
    public MapGenerator mapGenerator;  // Reference to the MapGenerator
    public ParameterDisplay parameterDisplay;
    public AllParameterDisplay allParameterDisplay;

    public enum ParameterToAnimate
    {
        Width,
        Height,
        NoiseScale,
        Octaves,
        Persistance,
        Lacunarity,
        Seed,
        OffsetX,
        OffsetY
    }
    public ParameterToAnimate parameter;

    public float startValue;
    public float endValue;
    public float rateOfChange;

    public enum AnimationMode
    {
        Bounce,
        MinToMax,
        MaxToMin
    }
    public AnimationMode animationMode;

    private bool isIncreasing = true;

    private void Start()
    {
        allParameterDisplay.EnableParameterDisplays();
        StartCoroutine(AnimateParameter());
    }

    IEnumerator AnimateParameter()
    {
        allParameterDisplay.DisableParameterDisplay(parameter);
        float currentValue = startValue;

        while (true)
        {
            SetParameterValue(currentValue);

            if (isIncreasing)
                currentValue += rateOfChange * Time.deltaTime;
            else
                currentValue -= rateOfChange * Time.deltaTime;

            if (animationMode == AnimationMode.Bounce)
            {
                if (currentValue > endValue || currentValue < startValue)
                {
                    isIncreasing = !isIncreasing;
                }
            }
            else if (animationMode == AnimationMode.MinToMax && currentValue > endValue)
            {
                break;
            }
            else if (animationMode == AnimationMode.MaxToMin && currentValue < startValue)
            {
                break;
            }
            mapGenerator.GenerateMap();
            yield return null;
        }
        allParameterDisplay.EnableParameterDisplays();
    }

    void SetParameterValue(float value)
    {
        switch (parameter)
        {
            case ParameterToAnimate.Width:
                mapGenerator.Width = (int)value;
                parameterDisplay.UpdateParameterText("Width", value);
                break;
            case ParameterToAnimate.Height:
                mapGenerator.Height = (int)value;
                parameterDisplay.UpdateParameterText("Height", value);
                break;
            case ParameterToAnimate.NoiseScale:
                mapGenerator.NoiseScale = value;
                parameterDisplay.UpdateParameterText("Noise Scale", value);
                break;
            case ParameterToAnimate.Octaves:
                mapGenerator.Octaves = (int)value;
                parameterDisplay.UpdateParameterText("Octaves", value);
                break;
            case ParameterToAnimate.Persistance:
                mapGenerator.Persistance = value;
                parameterDisplay.UpdateParameterText("Persistance", value);
                break;
            case ParameterToAnimate.Lacunarity:
                mapGenerator.Lacunarity = value;
                parameterDisplay.UpdateParameterText("Lacunarity", value);
                break;
            case ParameterToAnimate.Seed:
                mapGenerator.Seed = (int)value;
                parameterDisplay.UpdateParameterText("Seed", value);
                break;
            case ParameterToAnimate.OffsetX:
                mapGenerator.Offset = new Vector2(value, mapGenerator.Offset.y);
                parameterDisplay.UpdateParameterText("Offset X", value);
                break;
            case ParameterToAnimate.OffsetY:
                mapGenerator.Offset = new Vector2(mapGenerator.Offset.x, value);
                parameterDisplay.UpdateParameterText("Offset Y", value);
                break;
        }

        // Update the other parameters display:
        foreach (MapAnimator.ParameterToAnimate param in Enum.GetValues(typeof(MapAnimator.ParameterToAnimate)))
        {
            if (param != parameter)
            {
                allParameterDisplay.UpdateParameterText(param, GetParameterValue(param));
            }
        }
    }

    float GetParameterValue(ParameterToAnimate parameter)
    {
        switch (parameter)
        {
            case ParameterToAnimate.Width:
                return mapGenerator.Width;
            case ParameterToAnimate.Height:
                return mapGenerator.Height;
            case ParameterToAnimate.NoiseScale:
                return mapGenerator.NoiseScale;
            case ParameterToAnimate.Octaves:
                return mapGenerator.Octaves;
            case ParameterToAnimate.Persistance:
                return mapGenerator.Persistance;
            case ParameterToAnimate.Lacunarity:
                return mapGenerator.Lacunarity;
            case ParameterToAnimate.Seed:
                return mapGenerator.Seed;
            case ParameterToAnimate.OffsetX:
                return mapGenerator.Offset.x;
            case ParameterToAnimate.OffsetY:
                return mapGenerator.Offset.y;
        }
        return 0;
    }
}