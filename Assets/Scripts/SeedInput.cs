using System;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;

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

    public void SetInputToRandomWord()
    {
        input.text = WordList.GetRandomWord();
        SeedHash = ComputeHash(input.text);
    }

    public void SetInput()
    {
        SeedHash = ComputeHash(input.text);
    }

    private uint ComputeHash(string seed)
    {
        return BitConverter.ToUInt32(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(seed)));
    }
}
