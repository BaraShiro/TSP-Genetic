using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private Slider numberOfCitiesSlider;
    [SerializeField] private SeedInput seedInput;
    [SerializeField] private FancyUIButton solveButton;

    private int numberOfCities = 0;
    // private Vector3[] positions = Array.Empty<Vector3>();
    private readonly List<City> cities = new List<City>();

    private void Start()
    {
        SetupCities();
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
        lineRenderer.positionCount = 0;

        // positions = new Vector3[numberOfCities];
        for (int i = 0; i < numberOfCities; i++)
        {
            Vector3 pos = new Vector3(RNG.Instance.Range(-4f, 4f), RNG.Instance.Range(-4f, 4f), 0);
            pos += transform.position;
            // positions[i] = pos;
            GameObject cityGameObject = Instantiate(cityPrefab, pos, Quaternion.identity, transform);
            cityGameObject.AddComponent<City>();
            City city = cityGameObject.GetComponent<City>();
            city.Coordinates = pos;
            cities.Add(city);
        }
    }

    public async void Solve()
    {
        RNG.Instance.InitState(seedInput.SeedHash);

        solveButton.interactable = false;

        Genetic genetic = new Genetic(200, cities.ToArray());

        await Task.Run(() =>
        {
            genetic.Solve();
        });

        int[] best = genetic.BestChromosome;
        Vector3[] positions = new Vector3[best.Length + 1];
        for (int i = 0; i < best.Length; i++)
        {
            positions[i] = cities[best[i]].Coordinates;
        }
        positions[^1] = cities[^1].Coordinates;

        lineRenderer.positionCount = numberOfCities;
        lineRenderer.SetPositions(positions);

        solveButton.interactable = true;
    }
}
