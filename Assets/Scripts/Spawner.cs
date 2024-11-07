using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
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

    private int numberOfCities;
    private readonly List<City> cities = new List<City>();

    private void Start()
    {
        SetupCities();
        UndrawLine();
        ResetSolveText();
    }

    public void SetupCities()
    {
        RNG.Instance.InitState(seedInput.SeedHash);

        numberOfCities = Mathf.RoundToInt(numberOfCitiesSlider.value);
        foreach (City city in cities)
        {
            Destroy(city.gameObject);
        }
        cities.Clear();
        UndrawLine();
        ResetSolveText();

        for (int i = 0; i < numberOfCities; i++)
        {
            Vector3 pos = new Vector3(RNG.Instance.Range(-4f, 4f), RNG.Instance.Range(-4f, 4f), 0);
            pos += transform.position;
            GameObject cityGameObject = Instantiate(cityPrefab, pos, Quaternion.identity, transform);
            cityGameObject.AddComponent<City>();
            City city = cityGameObject.GetComponent<City>();
            city.Coordinates = pos;
            cities.Add(city);
        }
    }

    public async void Solve()
    {
        solveButton.interactable = false;
        UndrawLine();
        ResetSolveText();

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

        int[] best = genetic.BestChromosome;
        Vector3[] positions = new Vector3[best.Length + 1];
        for (int i = 0; i < best.Length; i++)
        {
            positions[i] = cities[best[i]].Coordinates;
        }
        positions[^1] = cities[^1].Coordinates;

        DrawLine(positions);

        solveText.text = $"Initial score: {genetic.BestScoreAtStart:0.00}\n" +
                         $"Best score: {genetic.BestScore:0.00} ({genetic.PercentageOfInitialScore:0.00}% of initial)\n" +
                         $"Evolved for {genetic.NumberOfGenerations} generations \n" +
                         $"Solution generated in {genetic.ElapsedTime:0.00} ms";

        solveButton.interactable = true;
    }

    private void UndrawLine()
    {
        lineRenderer.positionCount = 0;
    }

    private void DrawLine(Vector3[] positions)
    {
        lineRenderer.positionCount = numberOfCities;
        lineRenderer.SetPositions(positions);
    }

    private void ResetSolveText()
    {
        solveText.text = $"Initial score: \n" +
                         $"Best score: \n" +
                         $"Evolved for \n" +
                         $"Solution generated in ";
    }
}
