using System;

/// <summary>
/// A list of the 3000 most used words in english, minus a few boring ones, plus aardvark because it sounds cool.
/// </summary>
public static partial class WordList
{
    /// <summary>
    /// The number of words. If the wordlist changes, this must be updated accordingly.
    /// </summary>
    private const int NumberOfWords = 2957;

    /// <summary>
    /// Gets a random word from the word list.
    /// Uses <see cref="System.Random"/> (System) instead of RNG to separate it from the randomness of the solver.
    /// </summary>
    /// <returns>A random english word.</returns>
    public static string GetRandomWord()
    {
        Random random = new Random();
        int randomNumber = random.Next(NumberOfWords);
        return Words[randomNumber];
    }
}