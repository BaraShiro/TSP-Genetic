using System;

public static class ArrayExtensions
{
    /// <summary>
    /// Fills an array with new objects of the contained type.
    /// </summary>
    /// <param name="array">The array to be filled.</param>
    /// <typeparam name="T">The type of the objects the array contains.</typeparam>
    public static void FillWithNew<T>(this T[] array) {
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = Activator.CreateInstance<T>();
        }
    }

    /// <summary>
    /// Performs a Fisher–Yates shuffle on an array.
    /// </summary>
    /// <param name="array">The array to be shuffled.</param>
    /// <typeparam name="T">The type of the objects the array contains.</typeparam>
    /// <seealso href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Fisher–Yates shuffle on Wikipedia</seealso>
    public static void Shuffle<T>(this T[] array)
    {
        Random random = new Random();
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int j = random.Next(i, n);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
