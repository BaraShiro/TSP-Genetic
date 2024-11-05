using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private Slider numberOfCitiesSlider;
    [SerializeField] private SeedInput seedInput;

    private int numberOfCities = 0;
    // private Vector3[] positions = Array.Empty<Vector3>();
    private readonly List<City> cities = new List<City>();

    private void Start()
    {
        SetupCities();
    }

    public void SetupCities()
    {
        Random.InitState(seedInput.SeedHash);
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
            Vector3 pos = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0);
            pos += transform.position;
            // positions[i] = pos;
            GameObject cityGameObject = Instantiate(cityPrefab, pos, Quaternion.identity, transform);
            cityGameObject.AddComponent<City>();
            City city = cityGameObject.GetComponent<City>();
            city.Coordinates = pos;
            cities.Add(city);
        }
    }

    public void Solve()
    {
        Genetic genetic = new Genetic(200, cities.ToArray());

        genetic.Solve();

        int[] best = genetic.BestChromosome;
        Vector3[] positions = new Vector3[best.Length + 1];
        for (int i = 0; i < best.Length; i++)
        {
            positions[i] = cities[best[i]].Coordinates;
        }
        positions[^1] = cities[^1].Coordinates;


        lineRenderer.positionCount = numberOfCities;
        lineRenderer.SetPositions(positions);
    }


}
