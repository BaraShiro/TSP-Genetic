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

}
