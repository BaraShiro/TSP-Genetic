using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderUpdater : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private TMP_Text valueText;
    [SerializeField] private bool inPercent;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        SetValueText();
    }

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
