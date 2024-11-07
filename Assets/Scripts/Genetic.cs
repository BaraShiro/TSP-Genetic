using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

#pragma warning disable CS0162 // Unreachable code detected

public class Genetic
{
    public class GeneticSettings
    {
        /// <summary>
        /// Number of cities in tour.
        /// </summary>
        public readonly int NumberOfCities;
        /// <summary>
        /// The number of chromosomes in each generation.
        /// </summary>
        public readonly int NumberOfChromosomes;
        /// <summary>
        /// The number of parents in each generation, as a percentage of NumberOfChromosomes. A value between 0 and 100.
        /// </summary>
        public readonly float PercentageParents;
        /// <summary>
        /// The probability for a multipoint crossover to occur. A value between 0 and 1.
        /// </summary>
        public readonly float MpcProbability;
        /// <summary>
        /// The probability for a multiple mutation to occur. A value between 0 and 1.
        /// </summary>
        public readonly float MultiMutationProbability;
        /// <summary>
        /// The probability for a single mutation within the multiple mutation to occur. A value between 0 and 1.
        /// </summary>
        public readonly float MultiMutationMutationProbability;
        /// <summary>
        /// The point to stop, as a percentage of the initial score. A value between 0 and 100.
        /// </summary>
        public readonly float PercentageOfInitialToStopAt;
        /// <summary>
        /// The point to stop, as a number of generations without any further progress.
        /// </summary>
        public readonly int GenerationsWithoutProgressToStopAt;

        public GeneticSettings(int numberOfCities = 10,
            int numberOfChromosomes = 100,
            float percentageParents = 10,
            float mpcProbability = 75,
            float multiMutationProbability = 20,
            float multiMutationMutationProbability = 20,
            float percentageOfInitialToStopAt = 60,
            int generationsWithoutProgressToStopAt = 5)
        {
            NumberOfCities = numberOfCities;
            NumberOfChromosomes = numberOfChromosomes;
            PercentageParents = percentageParents;
            MpcProbability = mpcProbability;
            MultiMutationProbability = multiMutationProbability;
            MultiMutationMutationProbability = multiMutationMutationProbability;
            PercentageOfInitialToStopAt = percentageOfInitialToStopAt;
            GenerationsWithoutProgressToStopAt = generationsWithoutProgressToStopAt;
        }
    }

    private const bool LogDebug = false;

    private float bestScoreAtStart = float.PositiveInfinity;
    private float bestScoreThisGeneration = float.PositiveInfinity;
    private int[] bestChromosomeThisGeneration;
    private float[] scores;
    private int generation = 0;
    private int generationsWithoutProgress = 0;
    private double elapsedTime;

    public float BestScoreAtStart => bestScoreAtStart;
    public float BestScore => bestScoreThisGeneration;
    public float PercentageOfInitialScore => 100 * (bestScoreThisGeneration / bestScoreAtStart);
    public int NumberOfGenerations => generation - generationsWithoutProgress;
    public int[] BestChromosome => bestChromosomeThisGeneration;
    public double ElapsedTime => elapsedTime;

    private GeneticSettings Settings { get; }

    private int ChromosomeLength => Settings.NumberOfCities - 1;
    private int NumberOfParents => Mathf.RoundToInt(Settings.NumberOfChromosomes * (Settings.PercentageParents / 100));

    private int[][] GenePool { get; set; }
    private City[] Cities { get; set; }

    public Genetic(GeneticSettings settings)
    {
        Settings = settings;

        GenePool = new int[Settings.NumberOfChromosomes][];
        Cities = new City[Settings.NumberOfCities];
        Cities.FillWithNew();
    }

    public Genetic(GeneticSettings settings, City[] cities)
    {
        Settings = settings;

        GenePool = new int[Settings.NumberOfChromosomes][];
        Cities = cities;
    }

    public async Awaitable Solve()
    {
        await Awaitable.BackgroundThreadAsync();

        Application.exitCancellationToken.ThrowIfCancellationRequested();

        Stopwatch timer = new Stopwatch();
        timer.Start();

        bool identicalParents = true;
        while (identicalParents)
        {
            Application.exitCancellationToken.ThrowIfCancellationRequested();

            InitializeGenePool();
            SelectParents();
            identicalParents = CheckForIdenticalParents();
        }

        float percentageOfStartScore = float.PositiveInfinity;
        float lastGenerationsScore = float.PositiveInfinity;

        while (percentageOfStartScore >= Settings.PercentageOfInitialToStopAt)
        {
            Application.exitCancellationToken.ThrowIfCancellationRequested();

            generation++;

            if(LogDebug) Debug.Log($"percentage of initial score: {percentageOfStartScore}, percentage of score to stop at {Settings.PercentageOfInitialToStopAt / 100}");
            ProduceChildren();
            MutateChildren();
            SelectNewParents();

            percentageOfStartScore = 100 * (bestScoreThisGeneration / bestScoreAtStart);
            if(LogDebug) Debug.Log($"Generation: {generation}, Best score this generation: {bestScoreThisGeneration} ({percentageOfStartScore:000.00}%)");
            if (lastGenerationsScore <= bestScoreThisGeneration)
            {
                generationsWithoutProgress++;
            }
            else
            {
                generationsWithoutProgress = 0;
            }

            if (generationsWithoutProgress >= Settings.GenerationsWithoutProgressToStopAt)
            {
                if(LogDebug) Debug.Log($"No progress made in {Settings.GenerationsWithoutProgressToStopAt} generations, giving up.");
                break;
            }

            lastGenerationsScore = bestScoreThisGeneration;
        }

        timer.Stop();
        elapsedTime = timer.Elapsed.TotalMilliseconds;

        await Awaitable.MainThreadAsync();
    }

    private void InitializeGenePool()
    {
        bestChromosomeThisGeneration = new int[ChromosomeLength];
        scores = new float[Settings.NumberOfChromosomes];
        for (int i = 0; i < Settings.NumberOfChromosomes; i++)
        {
            GenePool[i] = Enumerable.Range(0, ChromosomeLength).ToArray();
            GenePool[i].Shuffle();

            float score = GoalFunction(GenePool[i]);
            scores[i] = score;
            if (score < bestScoreAtStart) bestScoreAtStart = score;
        }
        if(LogDebug) Debug.Log($"Best score at start: {bestScoreAtStart}");
    }

    private void CalculateScores()
    {
        int bestChromosomeIndex = 0;
        for (int i = 0; i < Settings.NumberOfChromosomes; i++)
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
        int[] sortedChromosomes = Enumerable.Range(0, Settings.NumberOfChromosomes).ToArray();
        Array.Sort(sortedChromosomes, (a, b) => scores[a].CompareTo(scores[b]));

        if(LogDebug) Debug.Log($"Number of parents: {NumberOfParents}");
        if(LogDebug) Debug.Log($"GenePool before: {GenePoolToString()}");

        for (int i = 0; i < NumberOfParents; i++)
        {
            SwapChromosomes(i, sortedChromosomes[i]);
        }

        // TODO: Test identical parents with seed

        if(LogDebug) Debug.Log($"GenePool after: {GenePoolToString()}");
        if(LogDebug) Debug.Log($"Sorted indices: {sortedChromosomes.ToPrettyString()}");
        if(LogDebug) Debug.Log($"Scores: {scores.ToPrettyString()}");
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
        for (int i = NumberOfParents; i < Settings.NumberOfChromosomes; i++)
        {
            CopyChromosome(parentIndex, i);
            parentIndex = (parentIndex + 1) % NumberOfParents;
        }
        if(LogDebug) Debug.Log($"GenePool children: {GenePoolToString()}");
    }

    private void MutateChildren()
    {
        // float mpcProbability = 0.75f;
        for (int i = NumberOfParents; i < Settings.NumberOfChromosomes; i++)
        {
            // If there are at least two children left
            if (Settings.NumberOfChromosomes - i >= 2)
            {
                // If mpcProbability apply MPC, increment i, and continue
                if (RNG.Instance.Value < Settings.MpcProbability)
                {
                    MultiPointCrossover(i, ++i);
                    continue;
                }
            }

            // Apply mutation if not mpcProbability, or only one child left
            if (RNG.Instance.Value < Settings.MultiMutationProbability)
            {
                MutateChromosomeMultiple(i);
            }
            else
            {
                MutateChromosomeSingle(i);
            }
        }
    }

    private void MultiPointCrossover(int first, int second)
    {
        if(LogDebug) Debug.Log($"MPC {first} & {second} before: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");
        (int start, int interval) = GetMpcInterval();
        if(LogDebug) Debug.Log($"Start {start}, Interval {interval}");

        for (int i = start; i < start + interval; i++)
        {
            (GenePool[first][i], GenePool[second][i]) = (GenePool[second][i], GenePool[first][i]);
        }
        if(LogDebug) Debug.Log($"MPC {first} & {second} after: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");

        RepairChromosome(first, start, interval);
        RepairChromosome(second, start, interval);

        if(LogDebug) Debug.Log($"MPC {first} & {second} repaired: {GenePool[first].ToPrettyString()} {GenePool[second].ToPrettyString()}");
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
        // Replace each duplicate number outside crossover interval with -1 and save its position in duplicates
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

        // If no duplicates are found, nothing needs repairing
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
        if(LogDebug) Debug.Log($"Mutate {chromosome} before: {GenePool[chromosome].ToPrettyString()}");

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

        if(LogDebug) Debug.Log($"Mutate {chromosome} after: {GenePool[chromosome].ToPrettyString()}");
    }

    private void MutateChromosomeMultiple(int chromosome)
    {
        if (Settings.MultiMutationMutationProbability <= 0) return;

        if(LogDebug) Debug.Log($"Multi mutate {chromosome} before: {GenePool[chromosome].ToPrettyString()}");
        for (int i = 0; i < ChromosomeLength; i++)
        {
            if (RNG.Instance.Value < Settings.MultiMutationMutationProbability)
            {
                int j = RNG.Instance.Range(0, ChromosomeLength);
                (GenePool[chromosome][i], GenePool[chromosome][j]) = (GenePool[chromosome][j], GenePool[chromosome][i]);
            }
        }
        if(LogDebug) Debug.Log($"Multi mutate {chromosome} after: {GenePool[chromosome].ToPrettyString()}");
    }

    private void SelectNewParents()
    {
        CalculateScores();

        // Mark all redundant children
        bool[] markedChildren = new bool[Settings.NumberOfChromosomes];
        int numberOfMarkedChildren = 0;

        // Compare parents to children, marking identical children
        for (int i = 0; i < NumberOfParents; i++)
        {
            for (int j = NumberOfParents; j < Settings.NumberOfChromosomes; j++)
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
        for (int i = NumberOfParents; i < Settings.NumberOfChromosomes - 1; i++)
        {
            for (int j = i + 1; j < Settings.NumberOfChromosomes; j++)
            {
                if (Mathf.Approximately(scores[i], scores[j])) // Approximate to compensate for float inaccuracy
                {
                    if(markedChildren[j]) continue; // already marked so don't mark again
                    markedChildren[j] = true;
                    numberOfMarkedChildren++;
                }
            }
        }

        if(LogDebug) Debug.Log($"GenePool: {GenePoolToString()}");
        if(LogDebug) Debug.Log($"Scores: {scores.ToPrettyString()}");
        if(LogDebug) Debug.Log($"Marked children: ({numberOfMarkedChildren}) {markedChildren.ToPrettyString()}");

        int[] sortedUnmarkedChromosomes = new int[Settings.NumberOfChromosomes - numberOfMarkedChildren];
        int numberOfSortedUnmarkedChromosomes = 0;
        for (int i = 0; i < Settings.NumberOfChromosomes; i++)
        {
            if (!markedChildren[i])
            {
                sortedUnmarkedChromosomes[numberOfSortedUnmarkedChromosomes] = i;
                numberOfSortedUnmarkedChromosomes++;
            }
        }
        Array.Sort(sortedUnmarkedChromosomes, (a, b) => scores[a].CompareTo(scores[b]));

        if(LogDebug) Debug.Log($"Sorted unmarked: {sortedUnmarkedChromosomes.ToPrettyString()}");

        for (int i = 0; i < NumberOfParents; i++)
        {
            if (i >= numberOfSortedUnmarkedChromosomes) break; // We have a lot of marked children, not good
            SwapChromosomes(i, sortedUnmarkedChromosomes[i]);
        }

        if(LogDebug) Debug.Log($"GenePool with new parents: {GenePoolToString()}");
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

    private string GenePoolToString()
    {
        return GenePool.Aggregate("", (current, chromosome) => current + (chromosome.ToPrettyString() + " "));
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
    private float GoalFunction(int[] chromosome)
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
