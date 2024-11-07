using System;
using UnityEngine;

/// <summary>
/// A sprite representing a city in a TSP tour.
/// </summary>
public class CitySprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CitySprites citySprites;

    /// <summary>
    /// Set sprite to a random sprite at start.
    /// </summary>
    private void Start()
    {
        spriteRenderer.sprite = citySprites.GetRandomSprite(transform.position);
    }
}
