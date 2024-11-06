using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderUpdater : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private TMP_Text valueText;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        valueText.text = slider.value.ToString();
    }

    public void SetValueText()
    {
        valueText.text = slider.value.ToString();
    }
}
