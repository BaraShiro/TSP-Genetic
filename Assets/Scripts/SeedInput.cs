using System;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// The input field for the RNG seed.
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class SeedInput : MonoBehaviour
{
    private SHA1 hashAlgorithm;
    private TMP_InputField input;

    public uint SeedHash { get; private set; }

    private void Awake()
    {
        hashAlgorithm = SHA1.Create();
        input = GetComponent<TMP_InputField>();
        input.text = WordList.GetRandomWord();
        SeedHash = ComputeHash(input.text);
    }

    /// <summary>
    /// Sets the value of the input field to a random word from <see cref="WordList"/> and computes the hash.
    /// </summary>
    public void SetInputToRandomWord()
    {
        input.text = WordList.GetRandomWord();
        SeedHash = ComputeHash(input.text);
    }

    /// <summary>
    /// Compute the hash of the input field's value.
    /// </summary>
    public void SetInput()
    {
        SeedHash = ComputeHash(input.text);
    }

    /// <summary>
    /// Computes a deterministic hash, as opposed to <see cref="object.GetHashCode()"/>.
    /// </summary>
    /// <param name="seed">The string to compute a hash from.</param>
    /// <returns>A unique and deterministic hash of <paramref name="seed"/>.</returns>
    private uint ComputeHash(string seed)
    {
        return BitConverter.ToUInt32(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(seed)));
    }
}
