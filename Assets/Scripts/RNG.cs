using System.Threading.Tasks;
using UnityEngine;

public class RNG : SingletonMonoBehaviour<RNG>
{
    private Unity.Mathematics.Random random = new Unity.Mathematics.Random();

    public float Value => random.NextFloat();

    public void InitState(uint seed)
    {
        random.InitState(seed);
    }

    public int Range(int min, int max)
    {
        return random.NextInt(min, max);
    }

    public float Range(float min, float max)
    {
        return random.NextFloat(min, max);
    }
}
