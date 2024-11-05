using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FancyUIButton : Button
{
    private const int Offset = 8; // The image move 4 pixels, so the offset is double that. I don't know why.
    private RectTransform textTransform;

    private RectTransform TextTransform
    {
        get
        {
            if (!textTransform)
            {
                textTransform = transform.GetChild(0).GetComponent<RectTransform>();
            }

            return textTransform;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!interactable) return;

        Vector2 min = TextTransform.offsetMin;
        min.y += Offset;
        TextTransform.offsetMin = min;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!interactable) return;

        Vector2 min = TextTransform.offsetMin;
        min.y -= Offset;
        TextTransform.offsetMin = min;
    }
}
