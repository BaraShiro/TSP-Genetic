public class Genetic
{
    private int NumberOfChromosomes { get; set; }
    private int NumberOfCities { get; set; }
    private int ChromosomeLength => NumberOfCities - 1;

    private int[,] GenePool { get; set; }
    private City[] Cities { get; set; }

    public Genetic(int numberOfChromosomes, int numberOfCities)
    {
        NumberOfChromosomes = numberOfChromosomes;
        NumberOfCities = numberOfCities;

        GenePool = new int[NumberOfChromosomes, ChromosomeLength];
        Cities = new City[NumberOfCities];
        Cities.FillWithNew();
    }

    public Genetic(int numberOfChromosomes, City[] cities)
    {
        NumberOfChromosomes = numberOfChromosomes;
        NumberOfCities = cities.Length;

        GenePool = new int[NumberOfChromosomes, ChromosomeLength];
        Cities = cities;
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
