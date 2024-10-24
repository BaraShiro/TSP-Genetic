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

}
