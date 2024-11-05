using System;

public static partial class WordList
{
    private const int NumberOfWords = 2957;
    public static string GetRandomWord()
    {
        Random random = new Random();
        int randomNumber = random.Next(NumberOfWords);
        return Words[randomNumber];
    }
}