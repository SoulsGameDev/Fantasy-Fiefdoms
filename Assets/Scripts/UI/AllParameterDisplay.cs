using UnityEngine;
using TMPro;

public class AllParameterDisplay : MonoBehaviour
{
    public TextMeshProUGUI widthText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI noiseScaleText;
    public TextMeshProUGUI octavesText;
    public TextMeshProUGUI persistanceText;
    public TextMeshProUGUI lacunarityText;
    public TextMeshProUGUI seedText;
    public TextMeshProUGUI offsetXText;
    public TextMeshProUGUI offsetYText;



    public void UpdateParameterText(MapAnimator.ParameterToAnimate parameter, float value)
    {
        switch (parameter)
        {
            case MapAnimator.ParameterToAnimate.Width:
                widthText.text = $"Width: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.Height:
                heightText.text = $"Height: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.NoiseScale:
                noiseScaleText.text = $"NoiseScale: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.Octaves:
                octavesText.text = $"Octaves: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.Persistance:
                persistanceText.text = $"Persistance: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.Lacunarity:
                lacunarityText.text = $"Lacunarity: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.Seed:
                seedText.text = $"Seed: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.OffsetX:
                offsetXText.text = $"Offset X: {value:F2}";
                break;
            case MapAnimator.ParameterToAnimate.OffsetY:
                offsetYText.text = $"Offset Y: {value:F2}";
                break;
        }
    }

    public void DisableParameterDisplay(MapAnimator.ParameterToAnimate parameter)
    {
        switch (parameter)
        {
            case MapAnimator.ParameterToAnimate.Width:
                widthText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.Height:
                heightText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.NoiseScale:
                noiseScaleText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.Octaves:
                octavesText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.Persistance:
                persistanceText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.Lacunarity:
                lacunarityText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.Seed:
                seedText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.OffsetX:
                offsetXText.gameObject.SetActive(false);
                break;
            case MapAnimator.ParameterToAnimate.OffsetY:
                offsetYText.gameObject.SetActive(false);
                break;
        }
    }

    public void EnableParameterDisplays()
    {
        widthText.gameObject.SetActive(true);
        heightText.gameObject.SetActive(true);
        noiseScaleText.gameObject.SetActive(true);
        octavesText.gameObject.SetActive(true);
        persistanceText.gameObject.SetActive(true);
        lacunarityText.gameObject.SetActive(true);
        seedText.gameObject.SetActive(true);
        offsetXText.gameObject.SetActive(true);
        offsetYText.gameObject.SetActive(true);

    }
}