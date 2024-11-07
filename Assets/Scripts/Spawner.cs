using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Instantiates the city sprites and lines, takes in the settings, starts the solver, and outputs the result.
/// </summary>
public class Spawner : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private SeedInput seedInput;
    [SerializeField] private Slider numberOfCitiesSlider;
    [SerializeField] private Slider numberOfChromosomesSlider;
    [SerializeField] private Slider percentageParentsSlider;
    [SerializeField] private Slider mpcProbabilitySlider;
    [SerializeField] private Slider multiMutationProbabilitySlider;
    [SerializeField] private Slider multiMutationMutationProbabilitySlider;
    [SerializeField] private Slider percentageOfInitialToStopAtSlider;
    [SerializeField] private Slider generationsWithoutProgressToStopAtSlider;
    [SerializeField] private FancyUIButton solveButton;
    [SerializeField] private TMP_Text solveText;

    /// <summary>
    /// Cities can't be closer that this.
    /// Sprites are 64 px, at 100 px per unit and some margin in the graphics, sprites will never touch.
    /// </summary>
    private const float SmallestTownProximity = 0.6f;

    private int numberOfCities;
    private readonly List<City> cities = new List<City>();
    private readonly List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private void Start()
    {
        SetupCities();
        UndrawLines();
        ResetSolveText();
    }

    /// <summary>
    /// Instantiates <see cref="numberOfCities"/> number of city sprites,
    /// placing them randomly and making sure they are not to close together,
    /// and saves them in <see cref="cities"/>.
    /// </summary>
    public void SetupCities()
    {
        RNG.Instance.InitState(seedInput.SeedHash);

        numberOfCities = Mathf.RoundToInt(numberOfCitiesSlider.value);
        foreach (City city in cities)
        {
            Destroy(city.gameObject);
        }
        cities.Clear();
        UndrawLines();
        ResetSolveText();

        for (int i = 0; i < numberOfCities; i++)
        {
            Vector3 pos = default;

            bool cityHasGoodPosition = false;
            while (!cityHasGoodPosition)
            {
                cityHasGoodPosition = true;

                pos = new Vector3(RNG.Instance.Range(-4f, 4f), RNG.Instance.Range(-4f, 4f), 0);
                pos += transform.position;

                foreach (City city in cities)
                {
                    if (Vector2.Distance(pos, city.Coordinates) < SmallestTownProximity)
                    {
                        cityHasGoodPosition = false;
                        break;
                    }
                }
            }

            GameObject cityGameObject = Instantiate(cityPrefab, pos, Quaternion.identity, transform);
            cityGameObject.AddComponent<City>();
            City newCity = cityGameObject.GetComponent<City>();
            newCity.Coordinates = pos;
            cities.Add(newCity);
        }
    }

    /// <summary>
    /// Sets up and starts the solver, outputting the result.
    /// </summary>
    public async void Solve()
    {
        // Inactivate button until done so one doesn't "accidentally" start a bunch of solvers on the background thread.
        solveButton.interactable = false;

        UndrawLines();
        ResetSolveText();

        // Re-init RNG to get more reliable results
        RNG.Instance.InitState(seedInput.SeedHash);

        Genetic.GeneticSettings settings = new Genetic.GeneticSettings(
            numberOfCities: numberOfCities,
            numberOfChromosomes: Mathf.RoundToInt(numberOfChromosomesSlider.value),
            percentageParents: percentageParentsSlider.value,
            mpcProbability: mpcProbabilitySlider.value,
            multiMutationProbability: multiMutationProbabilitySlider.value,
            multiMutationMutationProbability: multiMutationMutationProbabilitySlider.value,
            percentageOfInitialToStopAt: percentageOfInitialToStopAtSlider.value,
            generationsWithoutProgressToStopAt: Mathf.RoundToInt(generationsWithoutProgressToStopAtSlider.value)
            );

        Genetic genetic = new Genetic(settings, cities.ToArray());

        await genetic.Solve();

        Vector3[] positions = CalculatePositions(genetic.BestChromosome);

        DrawLines(positions);

        solveText.text = $"Initial score: {genetic.BestScoreAtStart:0.00}\n" +
                         $"Best score: {genetic.BestScore:0.00} ({genetic.PercentageOfInitialScore:0.00}% of initial)\n" +
                         $"Evolved for {genetic.NumberOfGenerations} generations \n" +
                         $"Solution generated in {genetic.ElapsedTime:0.00} ms";

        solveButton.interactable = true;
    }

    /// <summary>
    /// Calculates the best path based on the order of positions in bestChromosome.
    /// </summary>
    /// <param name="bestChromosome">A chromosome that contains the order of cities to visit.</param>
    /// <returns>A list of positions that when visited in order makes up the calculated path.</returns>
    private Vector3[] CalculatePositions(int[] bestChromosome)
    {
        Vector3[] positions = new Vector3[bestChromosome.Length + 1];
        for (int i = 0; i < bestChromosome.Length; i++)
        {
            positions[i] = cities[bestChromosome[i]].Coordinates;
        }
        positions[^1] = cities[^1].Coordinates;
        return positions;
    }

    /// <summary>
    /// Removes all lines.
    /// </summary>
    private void UndrawLines()
    {
        foreach (LineRenderer line in lineRenderers)
        {
            Destroy(line.gameObject);
        }
        lineRenderers.Clear();
    }

    /// <summary>
    /// Draws lines between all neighboring positions in <paramref name="positions"/>,
    /// creating a loop by connecting the end to the beginning.
    /// </summary>
    /// <param name="positions">The positions to draw lines between.</param>
    private void DrawLines(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            LineRenderer line = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity, transform);
            line.positionCount = 2;
            line.SetPosition(0, positions[i]);
            line.SetPosition(1, positions[i + 1 < positions.Length ? i + 1 : 0]);
            lineRenderers.Add(line);
        }
    }

    /// <summary>
    /// Resets the output text to the default.
    /// </summary>
    private void ResetSolveText()
    {
        solveText.text = $"Initial score: \n" +
                         $"Best score: \n" +
                         $"Evolved for \n" +
                         $"Solution generated in ";
    }
}
