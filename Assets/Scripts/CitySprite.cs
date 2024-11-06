using System;
using UnityEngine;

public class CitySprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CitySprites citySprites;

    private void Start()
    {
        spriteRenderer.sprite = citySprites.GetRandomSprite(transform.position);
    }
}
