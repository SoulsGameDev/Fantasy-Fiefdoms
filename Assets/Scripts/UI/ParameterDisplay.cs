using UnityEngine;
using TMPro; 

public class ParameterDisplay : MonoBehaviour
{
    public TextMeshProUGUI parameterText; 

    public void UpdateParameterText(string parameterName, float value)
    {
        parameterText.text = $"{parameterName}: {value:F2}";
    }
}