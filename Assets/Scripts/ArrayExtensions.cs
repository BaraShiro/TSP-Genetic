using System;
using Random = UnityEngine.Random;

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
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int j = Random.Range(i, n);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    /// <summary>
    /// Produces a pretty <see cref="string"/> representation of an array, using brackets and commas.
    /// </summary>
    /// <param name="array">The array to produce a string from.</param>
    /// <typeparam name="T">The type of the objects the array contains.</typeparam>
    /// <returns>A pretty string representation of the array.</returns>
    /// <example><c>new int[] { 1, 2, 3 }.ToPrettyString()</c> => <c>[1, 2, 3]</c></example>
    public static string ToPrettyString<T>(this T[] array)
    {
        switch (array.Length)
        {
            case < 1:
                return "[]";
            case 1:
                return $"[{array[0]}]";
        }

        string acc = "[";

        for (int i = 0; i < array.Length - 1; i++)
        {
            acc += $"{array[i]}, ";
        }
        acc += $"{array[^1]}]";

        return acc;
    }
}
