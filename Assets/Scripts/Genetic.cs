using System;
using System.Linq;
using UnityEngine;

public class Genetic
{
    private const float PercentageToStopAt = 50f;
    private const int GenerationsWithoutProgressToStopAt = 5;

    private float bestScoreAtStart = float.PositiveInfinity;
    private float bestScoreThisGeneration = float.PositiveInfinity;
    private int[] bestChromosomeThisGeneration;
    private float[] scores;

    public float BestScoreAtStart => bestScoreAtStart;
    public float BestScore => bestScoreThisGeneration;
    public int[] BestChromosome => bestChromosomeThisGeneration;

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

    public void Solve()
    {
        bool identicalParents = true;
        while (identicalParents)
        {
            InitializeGenePool();
            SelectParents();
            identicalParents = CheckForIdenticalParents();
        }

        float percentageOfStartScore = float.PositiveInfinity;
        int generation = 1;
        int generationWithoutProgress = 0;
        float lastGenerationsScore = float.PositiveInfinity;
        while (percentageOfStartScore >= PercentageToStopAt)
        {
            ProduceChildren();
            MutateChildren();
            SelectNewParents();

            percentageOfStartScore = 100 * (bestScoreThisGeneration / bestScoreAtStart);
            Debug.Log($"Generation: {generation}, Best score this generation: {bestScoreThisGeneration} ({percentageOfStartScore:000.00}%)");
            if (Mathf.Approximately(lastGenerationsScore, bestScoreThisGeneration)) generationWithoutProgress++;
            if (generationWithoutProgress >= GenerationsWithoutProgressToStopAt)
            {
                Debug.Log($"No progress made in {GenerationsWithoutProgressToStopAt} generations, giving up.");
                break;
            }

            generation++;
            lastGenerationsScore = bestScoreThisGeneration;
        }
    }

    private void InitializeGenePool()
    {
        bestChromosomeThisGeneration = new int[ChromosomeLength];
        scores = new float[NumberOfChromosomes];
        for (int i = 0; i < NumberOfChromosomes; i++)
        {
            GenePool[i] = Enumerable.Range(0, ChromosomeLength).ToArray();
            GenePool[i].Shuffle();

            float score = GoalFunction(GenePool[i]);
            scores[i] = score;
            if (score < bestScoreAtStart) bestScoreAtStart = score;
        }
        Debug.Log($"Best score at start: {bestScoreAtStart}");
    }

    private void CalculateScores()
    {
        int bestChromosomeIndex = 0;
        for (int i = 0; i < NumberOfChromosomes; i++)
        {
            float score = GoalFunction(GenePool[i]);
            scores[i] = score;
            if (score < bestScoreThisGeneration)
            {
                bestScoreThisGeneration = score;
                bestChromosomeIndex = i;
            }
        }

        GenePool[bestChromosomeIndex].CopyTo(bestChromosomeThisGeneration, 0);
    }

    private void SelectParents()
    {
        int[] sortedChromosomes = Enumerable.Range(0, NumberOfChromosomes).ToArray();
        Array.Sort(sortedChromosomes, (a, b) => scores[a].CompareTo(scores[b]));

        Debug.Log($"Number of parents: {NumberOfParents}");
        Debug.Log($"GenePool before: {GenePoolToString()}");

        for (int i = 0; i < NumberOfParents; i++)
        {
            SwapChromosomes(i, sortedChromosomes[i]);
        }

        // TODO: Test identical parents with seed

        Debug.Log($"GenePool after: {GenePoolToString()}");
        Debug.Log($"Sorted indices: {sortedChromosomes.ToPrettyString()}");
        Debug.Log($"Scores: {scores.ToPrettyString()}");
    }

    private bool CheckForIdenticalParents()
    {
        // Look for identical scores to find identical parents
        for (int i = 0; i < NumberOfParents - 1; i++)
        {
            float firstScore = GoalFunction(GenePool[i]);

            for (int j = i + 1; j < NumberOfParents; j++)
            {
                float secondScore = GoalFunction(GenePool[j]);

                // Identical score means identical parents
                if (Mathf.Approximately(firstScore, secondScore)) // Approximate to compensate for float inaccuracy
                {
                    Debug.LogWarning($"Identical parents found. Starting over...");
                    return true;
                }
            }
        }

        return false;
    }

    private void ProduceChildren()
    {
        int parentIndex = 0;
        for (int i = NumberOfParents; i < NumberOfChromosomes; i++)
        {
            CopyChromosome(parentIndex, i);
            parentIndex = (parentIndex + 1) % NumberOfParents;
        }
        Debug.Log($"GenePool children: {GenePoolToString()}");
    }

    private void MutateChildren()
    {
        float mpcProbability = 0.75f;
        for (int i = NumberOfParents; i < NumberOfChromosomes; i++)
        {
            // If there are at least two children left
            if (NumberOfChromosomes - i >= 2)
            {
                // If mpcProbability apply MPC, increment i, and continue
                if (RNG.Instance.Value < mpcProbability)
                {
                    MultiPointCrossover(i, ++i);
                    continue;
                }
            }
            // Apply mutation if not mpcProbability, or only one child left
            MutateChromosomeSingle(i);
        }
    }

    private void MultiPointCrossover(int first, int second)
    {
        Debug.Log($"MPC {first} & {second} before: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");
        (int start, int interval) = GetMpcInterval();
        Debug.Log($"Start {start}, Interval {interval}");

        for (int i = start; i < start + interval; i++)
        {
            (GenePool[first][i], GenePool[second][i]) = (GenePool[second][i], GenePool[first][i]);
        }
        Debug.Log($"MPC {first} & {second} after: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");

        RepairChromosome(first, start, interval);
        RepairChromosome(second, start, interval);

        Debug.Log($"MPC {first} & {second} repaired: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");
    }

    private (int start, int interval) GetMpcInterval()
    {
        int minInterval = 2;
        int maxInterval = ChromosomeLength / 2;
        int interval = RNG.Instance.Range(minInterval, maxInterval + 1);
        int start = RNG.Instance.Range(0, (ChromosomeLength - interval) + 1);

        return (start, interval);
    }

    private void RepairChromosome(int chromosome, int start, int interval)
    {
        // Replace each duplicate number outside of crossover interval with -1 and save its position in duplicates
        int[] duplicates = new int[ChromosomeLength];
        int numberOfDuplicates = 0;

        // Iterate over all numbers outside interval
        for(int i = 0; i < ChromosomeLength; i++) {
            // We are inside the crossover interval, so continue
            if(i >= start && i < start + interval) continue;

            // Compare current number to all numbers inside interval to find duplicates
            for (int j = start; j < start + interval; j++) {
                // Duplicate found. Set to -1, record position, and update number of duplicates found
                if(GenePool[chromosome][i] == GenePool[chromosome][j]) {
                    GenePool[chromosome][i] = -1;
                    duplicates[numberOfDuplicates] = i;
                    numberOfDuplicates++;
                }
            }
        }

        // If no duplicates are found nothing needs repairing
        if (numberOfDuplicates == 0) return;

        // Find included numbers and set them to true
        bool[] numbersIncluded = new bool[ChromosomeLength];
        for(int i = 0; i < ChromosomeLength; i++) {
            // If a number is not -1, set it to true in numbersIncluded[]
            if(GenePool[chromosome][i] != -1) {
                numbersIncluded[GenePool[chromosome][i]] = true;
            }
        }

        // Find missing numbers, record them, and keep track of how many
        int[] missingNumbers = new int[ChromosomeLength];
        int numberOfMissingNumbers = 0;

        // Iterate over numbersIncluded[]
        for (int i = 0; i < ChromosomeLength; i++) {
            // If a number is not present (false) in numbersIncluded[] add it to missingNumbers[]
            if (!numbersIncluded[i]) {
                missingNumbers[numberOfMissingNumbers] = i;
                numberOfMissingNumbers++;
            }
        }

        if (numberOfDuplicates != numberOfMissingNumbers)
        {
            Debug.LogError($"Mismatch! Number of duplicates ({numberOfDuplicates}) is not equal to the number of missing numbers ({numberOfMissingNumbers})");
        }

        // Put the missing numbers from missingNumbers[] into the holes in GenePool[chromosome][] in ascending order
        for (int i = 0; i < numberOfMissingNumbers; i++) {
            GenePool[chromosome][duplicates[i]] = missingNumbers[i];
        }
    }

    private void MutateChromosomeSingle(int chromosome, bool assuredMutation = true)
    {
        Debug.Log($"Mutate {chromosome} before: {GenePool[chromosome].ToPrettyString()}");

        int first = RNG.Instance.Range(0, ChromosomeLength);
        int second = first;
        if (assuredMutation)
        {
            while (first == second) second = RNG.Instance.Range(0, ChromosomeLength);
        }
        else
        {
            second = RNG.Instance.Range(0, ChromosomeLength);
        }

        (GenePool[chromosome][first], GenePool[chromosome][second]) = (GenePool[chromosome][second], GenePool[chromosome][first]);

        Debug.Log($"Mutate {chromosome} after: {GenePool[chromosome].ToPrettyString()}");
    }

    private void MutateChromosomeMultiple(int index)
    {
        float mutationProbability = 0.2f;
        for (int i = 0; i < ChromosomeLength; i++)
        {
            if (RNG.Instance.Value < mutationProbability)
            {
                int j = RNG.Instance.Range(0, ChromosomeLength);
                (GenePool[index][i], GenePool[index][j]) = (GenePool[index][j], GenePool[index][i]);
            }
        }
    }

    private void SelectNewParents()
    {
        CalculateScores();

        // Mark all redundant children
        bool[] markedChildren = new bool[NumberOfChromosomes];
        int numberOfMarkedChildren = 0;

        // Compare parents to children, marking identical children
        for (int i = 0; i < NumberOfParents; i++)
        {
            for (int j = NumberOfParents; j < NumberOfChromosomes; j++)
            {
                if (Mathf.Approximately(scores[i], scores[j])) // Approximate to compensate for float inaccuracy
                {
                    if(markedChildren[j]) continue; // already marked so don't mark again
                    markedChildren[j] = true;
                    numberOfMarkedChildren++;
                }
            }
        }

        // Compare children to children, marking identical children
        for (int i = NumberOfParents; i < NumberOfChromosomes - 1; i++)
        {
            for (int j = i + 1; j < NumberOfChromosomes; j++)
            {
                if (Mathf.Approximately(scores[i], scores[j])) // Approximate to compensate for float inaccuracy
                {
                    if(markedChildren[j]) continue; // already marked so don't mark again
                    markedChildren[j] = true;
                    numberOfMarkedChildren++;
                }
            }
        }

        Debug.Log($"GenePool: {GenePoolToString()}");
        Debug.Log($"Scores: {scores.ToPrettyString()}");
        Debug.Log($"Marked children: ({numberOfMarkedChildren}) {markedChildren.ToPrettyString()}");

        int[] sortedUnmarkedChromosomes = new int[NumberOfChromosomes - numberOfMarkedChildren];
        int numberOfsortedUnmarkedChromosomes = 0;
        for (int i = 0; i < NumberOfChromosomes; i++)
        {
            if (!markedChildren[i])
            {
                sortedUnmarkedChromosomes[numberOfsortedUnmarkedChromosomes] = i;
                numberOfsortedUnmarkedChromosomes++;
            }
        }
        Array.Sort(sortedUnmarkedChromosomes, (a, b) => scores[a].CompareTo(scores[b]));

        Debug.Log($"Sorted unmarked: {sortedUnmarkedChromosomes.ToPrettyString()}");

        for (int i = 0; i < NumberOfParents; i++)
        {
            if (i >= numberOfsortedUnmarkedChromosomes) break; // We have a lot of marked children, not good
            SwapChromosomes(i, sortedUnmarkedChromosomes[i]);
        }

        Debug.Log($"GenePool with new parents: {GenePoolToString()}");
    }


    private void SwapChromosomes(int first, int second)
    {
        if (first == second) return; // Same index, no need to swap

        (GenePool[first], GenePool[second]) = (GenePool[second], GenePool[first]);
    }

    private void CopyChromosome(int source, int destination)
    {
        if (source == destination) return; // Same index, no need to copy

        for (int i = 0; i < ChromosomeLength; i++)
        {
            GenePool[destination][i] = GenePool[source][i];
        }
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
