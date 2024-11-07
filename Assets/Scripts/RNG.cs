/// <summary>
/// A wrapper for <see cref="Unity.Mathematics.Random"/> (Unity.Mathematics)
/// to make it behave more like <see cref="UnityEngine.Random"/> (UnityEngine).
/// Callable outside the main thread.
/// </summary>
public class RNG : SingletonMonoBehaviour<RNG>
{
    private Unity.Mathematics.Random random = new Unity.Mathematics.Random();

    /// <summary>
    /// Gets a uniformly random float value in the interval [0,1)
    /// </summary>
    public float Value => random.NextFloat();

    /// <summary>
    /// Initialized the state of the Random instance with a given seed value. The seed must be non-zero.
    /// </summary>
    /// <param name="seed">The seed to initialize with.</param>
    public void InitState(uint seed)
    {
        random.InitState(seed);
    }

    /// <summary>
    /// Returns a uniformly random int value in the interval [min, max).
    /// </summary>
    /// <param name="min">The minimum value to generate, inclusive.</param>
    /// <param name="max">The maximum value to generate, exclusive.</param>
    /// <returns>A uniformly random integer between [min, max).</returns>
    public int Range(int min, int max)
    {
        return random.NextInt(min, max);
    }
    /// <summary>
    /// Returns a uniformly random float value in the interval [min, max).
    /// </summary>
    /// <param name="min">The minimum value to generate, inclusive.</param>
    /// <param name="max">The maximum value to generate, exclusive.</param>
    /// <returns>A uniformly random float value between [min, max).</returns>
    public float Range(float min, float max)
    {
        return random.NextFloat(min, max);
    }
}
