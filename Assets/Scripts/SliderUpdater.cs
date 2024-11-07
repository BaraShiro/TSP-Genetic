using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a label based on the value of a slider.
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderUpdater : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private TMP_Text valueText;
    /// <summary>
    /// Whether this a percent value.
    /// </summary>
    [SerializeField] private bool inPercent;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        SetValueText();
    }

    /// <summary>
    /// Sets the text of the label to the value of the slider, truncating to two decimals for floats,
    /// and appends a "%" if <see cref="inPercent"/> is true.
    /// </summary>
    public void SetValueText()
    {
        if (slider.wholeNumbers)
        {
            valueText.text = $"{slider.value}{(inPercent ? "%": "")}";
        }
        else
        {
            valueText.text = $"{slider.value:0.00}{(inPercent ? "%": "")}";
        }

    }
}
