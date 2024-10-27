using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine;

public class Genetic
{
    private float bestScoreAtStart = float.PositiveInfinity;
    private float bestScoreThisGeneration = float.PositiveInfinity;
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

    private void CalculateScores()
    {
        for (int i = 0; i < NumberOfChromosomes; i++)
        {
            float score = GoalFunction(GenePool[i]);
            scores[i] = score;
            if (score < bestScoreThisGeneration) bestScoreThisGeneration = score;
        }
        Debug.Log($"Best score this generation: {bestScoreThisGeneration}");
    }

    private float CalculateGenerationProportionalScore()
    {
        return 100 * (bestScoreThisGeneration / bestScoreAtStart);
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
                if (Random.value < mpcProbability)
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
        (int start, int interval) = GetMpcInterval();

        for (int i = start; i < start + interval; i++)
        {
            (GenePool[first][i], GenePool[second][i]) = (GenePool[second][i], GenePool[first][i]);
        }

        RepairChromosome(first, start, interval);
        RepairChromosome(second, start, interval);
    }

    private (int start, int interval) GetMpcInterval()
    {
        int minInterval = 2;
        int maxInterval = ChromosomeLength / 2;
        int interval = Random.Range(minInterval, maxInterval + 1);
        int start = Random.Range(0, (ChromosomeLength - interval) + 1);

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

        int first = Random.Range(0, ChromosomeLength);
        int second = first;
        if (assuredMutation)
        {
            while (first == second) second = Random.Range(0, ChromosomeLength);
        }
        else
        {
            second = Random.Range(0, ChromosomeLength);
        }

        (GenePool[chromosome][first], GenePool[chromosome][second]) = (GenePool[chromosome][second], GenePool[chromosome][first]);
    }

    private void MutateChromosomeMultiple(int index)
    {
        float mutationProbability = 0.2f;
        for (int i = 0; i < ChromosomeLength; i++)
        {
            if (Random.value < mutationProbability)
            {
                int j = Random.Range(0, ChromosomeLength);
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

        for (int i = 0; i < NumberOfParents; i++)
        {
            if (i >= numberOfsortedUnmarkedChromosomes) break; // We have a lot of marked children, not good
            SwapChromosomes(i, sortedUnmarkedChromosomes[i]);
        }

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



    // (5) Move the P best chromosomes (also called parents), to the top of mGP, e.g., m = 0 ... P-1,
    // but make sure that no two parents have the same GF value (and thus, identical chromosomes),
    // although it is mathematically highly improbable that two parents are identical at this stage,
    // unless M >> N (M is a number much larger than N). Nevertheless, if two parents are identical,
    // the simplest solution (disregarding optimization) at this stage is to repeat steps (3)-(5),
    // using new random values until all parents are found to be unique.


    //(6) Fill, using alternation, the space allocated for the children (i.e., m = P ... M-1)
    //in mGP with copies of the parents. As an example, mGP, in a case with M = 7, N = 6, and P = 2:

    // m = 0: 0 4 2 3 1 --- parent 1
    // m = 1: 1 2 3 4 0 --- parent 2
    // m = 2: 0 4 2 3 1 --- child 1 (copy of parent 1)
    // m = 3: 1 2 3 4 0 --- child 2 (copy of parent 2)
    // m = 4: 0 4 2 3 1 --- child 3 (copy of parent 1)
    // m = 5: 1 2 3 4 0 --- child 4 (copy of parent 2)
    // m = 6: 0 4 2 3 1 --- child 5 (copy of parent 1)


    // (7) Apply MPC or mutation to all children, until all chromosomes m = P ... M-1,
    // have been altered. As an example with M > 9 and P = 2:

    // No change: m = 0-1 (parents are left intact)
    // MPC: m = 2-3
    // MPC: m = 4-5
    // Mutation: m = 6
    // MPC: m = 7-8
    // etc.


    // (8) Evaluate the GFs for all chromosomes. Compare each parent with each child by their GFs to check if
    // any parent could be identical to any child. If so, mark this child. In addition, compare each child to
    // the other children by their GFs and mark potentially redundant children,
    // so that only unique children are left unmarked.


    // (9) Sort all chromosomes that are not marked, so that the best chromosomes (new parents) end up at the
    // top of the matrix, i.e., m = 0 ... P-1. Marking is important to maintain the diversity of the parents,
    // so that no two parents turn out to be identical (i.e., to have identical chromosomes).


    // (10) Repeat steps (6)-(9) until the chromosome with the best GF value (i.e., the lowest) for each
    // generation shows in average to be lower than 70% of the best chromosome at start.
}
