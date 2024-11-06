using UnityEngine;

/// <summary>
/// Keeps an array of sprites and a function to randomly return one.
/// </summary>
[CreateAssetMenu(fileName = "CitySprites", menuName = "ScriptableObjects/CitySprites", order = 1)]
public class CitySprites : ScriptableObject
{
    [SerializeField] private Sprite[] citySprites;

    /// <summary>
    /// Get a random sprite from <see cref="citySprites"/> based on <paramref name="position"/>.
    /// </summary>
    /// <param name="position">A position that is used to calculate a pseudo random value.</param>
    /// <returns>A random sprite from <see cref="citySprites"/>.</returns>
    /// <remarks>This function uses a simple pseudo random function based on the calling city's position to generate
    /// random values. This is used instead of <see cref="RNG"/> to make it more deterministic, i.e., a city at a
    /// certain position will always get the same sprite regardless of the number of cities.</remarks>
    public Sprite GetRandomSprite(Vector3 position)
    {
        int random = Mathf.Abs(Mathf.RoundToInt((position.x + position.y + position.z) * 1000) % citySprites.Length);
        return citySprites[random];
    }
}
