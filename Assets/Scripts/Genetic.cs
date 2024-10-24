using System;
using System.Linq;
using UnityEngine;

public class Genetic
{
    private float bestScoreAtStart = float.PositiveInfinity;
    private float[] scores;
    private int NumberOfChromosomes { get; set; }
    private int NumberOfCities { get; set; }
    private int ChromosomeLength => NumberOfCities - 1;
    private int NumberOfParents => Mathf.RoundToInt(NumberOfChromosomes * 0.25f);

    private int[][] GenePool { get; set; }
    private City[] Cities { get; set; }

    public Genetic(int numberOfChromosomes, int numberOfCities)
    {
        NumberOfChromosomes = numberOfChromosomes;
        NumberOfCities = numberOfCities;

        GenePool = new int[NumberOfChromosomes][];
        Cities = new City[NumberOfCities];
        Cities.FillWithNew();
    }

    public Genetic(int numberOfChromosomes, City[] cities)
    {
        NumberOfChromosomes = numberOfChromosomes;
        NumberOfCities = cities.Length;

        GenePool = new int[NumberOfChromosomes][];
        Cities = cities;
    }
    private void InitializeGenePool()
    {
        scores = new float[NumberOfChromosomes];
        for (int i = 0; i < NumberOfChromosomes; i++)
        {
            GenePool[i] = Enumerable.Range(0, ChromosomeLength).ToArray();
            GenePool[i].Shuffle();

            float score = GoalFunction(GenePool[i]);
            scores[i] = score;
            if (score < bestScoreAtStart) bestScoreAtStart = score;
        }
    }

    private void SelectParents()
    {
        int[] sortedChromosomes = Enumerable.Range(0, NumberOfChromosomes).ToArray();
        Array.Sort(sortedChromosomes, (x, y) => scores[x].CompareTo(scores[y]));

        for (int i = 0; i < NumberOfParents; i++)
        {
            SwapChromosomes(i, sortedChromosomes[i]);
        }
    }

    private void SwapChromosomes(int first, int second)
    {
        if (first == second) return; // Same index, no need to swap

        (GenePool[first], GenePool[second]) = (GenePool[second], GenePool[first]);
    }

    public string CitiesToString()
    {
        string result = "Cities: [";

        for (int i = 0; i < Cities.Length; i++)
        {
            result += Cities[i].ToString();
            if (i != Cities.Length - 1)
            {
                result += ", ";
            }
            else
            {
                result += "]";
            }
        }

        return result;
    }

    public string GenePoolToString()
    {
        return GenePool.Aggregate("", (current, chromosome) => current + (chromosome.ToPrettyString() + " "));
    }

    public string GoalTest()
    {
        InitializeGenePool();
        return $"{bestScoreAtStart}";
    }

    /// <summary>
    /// Sums the distance between all <see cref="City">cities</see> in <see cref="Cities"/> in the order specified in
    /// <paramref name="chromosome"/>, with the last element in <see cref="Cities"/> as the start and end of the tour.
    /// </summary>
    /// <param name="chromosome">An array containing the order in which to visit cities,
    /// with each element representing an index in <see cref="Cities"/>.</param>
    /// <returns>The sum distance of the tour.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>Preconditions:</listheader>
    /// <item>The length of <paramref name="chromosome"/> must be equal to <see cref="ChromosomeLength"/>.</item>
    /// <item>Elements of <paramref name="chromosome"/> must be unique.</item>
    /// <item>Elements of <paramref name="chromosome"/> must be an index of an element in <see cref="Cities"/>.</item>
    /// <item>Elements of <paramref name="chromosome"/> must not be the index of the last element in <see cref="Cities"/>.</item>
    /// </list>
    /// </remarks>
    public float GoalFunction(int[] chromosome)
    {
        float sum = 0;

        // Add distance between start of tour (last city in Cities) and first city in chromosome
        sum += Cities[^1].Distance(Cities[chromosome[0]]);

        // Sum distance between cities in chromosome
        for (int i = 0; i < ChromosomeLength - 1; i++)
        {
            sum += Cities[chromosome[i]].Distance(Cities[chromosome[i+1]]);
        }

        // Add distance between last in chromosome and end of tour (last city in Cities)
        sum += Cities[chromosome[^1]].Distance(Cities[^1]);

        return sum;
    }
}
