using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A fancy version of <see cref="Button"/> that moves the button text down when pressing the button.
/// <remarks>This class is hardcoded to work in this very specific scenario.</remarks>
/// </summary>
[RequireComponent(typeof(Image))]
public class FancyUIButton : Button
{
    private const int Offset = 8; // The image move 4 pixels, so the offset is double that. I don't know why.
    private RectTransform textTransform;

    /// <summary>
    /// Get the first child's <see cref="RectTransform"/>.
    /// </summary>
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

    /// <summary>
    /// When releasing the button, move the text back up.
    /// </summary>
    /// <param name="eventData">The data for the event.</param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!interactable) return;

        Vector2 min = TextTransform.offsetMin;
        min.y += Offset;
        TextTransform.offsetMin = min;
    }

    /// <summary>
    /// When pressing the button, move the text down.
    /// </summary>
    /// <param name="eventData">The data for the event.</param>
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!interactable) return;

        Vector2 min = TextTransform.offsetMin;
        min.y -= Offset;
        TextTransform.offsetMin = min;
    }
}
