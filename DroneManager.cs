using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DroneManager : MonoBehaviour
{
    [Header("Drone Inputs")]
    public TMP_InputField kronusInput;
    public TMP_InputField lyrionInput;
    public TMP_InputField mystaraInput;
    public TMP_InputField eclipsiaInput;
    public TMP_InputField fioraInput;

    [Header("Total Drones")]
    public TextMeshProUGUI totalDronesText;
    public TMP_Text errorText;
    private const int MAX_DRONES = 1000;

    [Header("Planets")]
    public RectTransform kronusPlanet;
    public RectTransform lyrionPlanet;
    public RectTransform mystaraPlanet;
    public RectTransform eclipsiaPlanet;
    public RectTransform fioraPlanet;

    [Header("Drone Settings")]
    public GameObject dronePrefab;
    public float spawnDelay = 0.1f;
    public float spawnRadius = 300f; // Радіус спавну дронів навколо планет

    private List<Drone> activeDrones = new List<Drone>();

    private void Start()
    {
        // Initialize input fields
        kronusInput.onValueChanged.AddListener(OnDroneValueChanged);
        lyrionInput.onValueChanged.AddListener(OnDroneValueChanged);
        mystaraInput.onValueChanged.AddListener(OnDroneValueChanged);
        eclipsiaInput.onValueChanged.AddListener(OnDroneValueChanged);
        fioraInput.onValueChanged.AddListener(OnDroneValueChanged);

        // Set initial values
        kronusInput.text = "200";
        lyrionInput.text = "200";
        mystaraInput.text = "200";
        eclipsiaInput.text = "200";
        fioraInput.text = "200";

        UpdateTotalDrones();
    }

    private void OnDroneValueChanged(string value)
    {
        UpdateTotalDrones();
        ValidateStats();
    }

    private void UpdateTotalDrones()
    {
        int total = GetTotalDrones();
        totalDronesText.text = $"Всього дронів: {total}/{MAX_DRONES}";
        
        // Change color based on total
        if (total > MAX_DRONES)
        {
            totalDronesText.color = Color.red;
        }
        else if (total == MAX_DRONES)
        {
            totalDronesText.color = Color.green;
        }
        else
        {
            totalDronesText.color = Color.white;
        }
    }

    public int GetTotalDrones()
    {
        if (!IsValidInput(kronusInput.text, out int kronus) ||
            !IsValidInput(lyrionInput.text, out int lyrion) ||
            !IsValidInput(mystaraInput.text, out int mystara) ||
            !IsValidInput(eclipsiaInput.text, out int eclipsia) ||
            !IsValidInput(fioraInput.text, out int fiora))
        {
            return 0;
        }

        return kronus + lyrion + mystara + eclipsia + fiora;
    }

    public bool ValidateStats()
    {
        // Спроба зчитати всі значення
        if (!IsValidInput(kronusInput.text, out int kronus) ||
            !IsValidInput(lyrionInput.text, out int lyrion) ||
            !IsValidInput(mystaraInput.text, out int mystara) ||
            !IsValidInput(eclipsiaInput.text, out int eclipsia) ||
            !IsValidInput(fioraInput.text, out int fiora))
        {
            errorText.text = "❌ Введіть лише числа від 0 до 1000.";
            return false;
        }

        // Перевірка порядку
        if (!(kronus >= lyrion && lyrion >= mystara && mystara >= eclipsia && eclipsia >= fiora))
        {
            errorText.text = "❌ Порушено порядок: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora.";
            return false;
        }

        // Перевірка суми
        int total = kronus + lyrion + mystara + eclipsia + fiora;
        if (total != MAX_DRONES)
        {
            errorText.text = $"❌ Сума має бути {MAX_DRONES}. Зараз: {total}.";
            return false;
        }

        // Успіх
        errorText.text = "✅ Усі умови виконано!";
        return true;
    }

    private bool IsValidInput(string input, out int value)
    {
        return int.TryParse(input, out value) && value >= 0 && value <= MAX_DRONES;
    }

    public (int kronus, int lyrion, int mystara, int eclipsia, int fiora) GetDroneDistribution()
    {
        if (!IsValidInput(kronusInput.text, out int kronus) ||
            !IsValidInput(lyrionInput.text, out int lyrion) ||
            !IsValidInput(mystaraInput.text, out int mystara) ||
            !IsValidInput(eclipsiaInput.text, out int eclipsia) ||
            !IsValidInput(fioraInput.text, out int fiora))
        {
            return (0, 0, 0, 0, 0);
        }

        return (kronus, lyrion, mystara, eclipsia, fiora);
    }

    public List<RectTransform> GetPlanetRects()
    {
        var planets = new List<RectTransform>();
        if (kronusPlanet != null) planets.Add(kronusPlanet);
        if (lyrionPlanet != null) planets.Add(lyrionPlanet);
        if (mystaraPlanet != null) planets.Add(mystaraPlanet);
        if (eclipsiaPlanet != null) planets.Add(eclipsiaPlanet);
        if (fioraPlanet != null) planets.Add(fioraPlanet);
        return planets;
    }

    public void SpawnDronesForAllPlanets(List<RectTransform> planetRects)
    {
        // Очищаємо попередні дрони
        ClearAllDrones();
        
        // Отримуємо кількість дронів для кожної планети
        int kronusCount = int.Parse(kronusInput.text) / 10;
        int lyrionCount = int.Parse(lyrionInput.text) / 10;
        int mystaraCount = int.Parse(mystaraInput.text) / 10;
        int eclipsiaCount = int.Parse(eclipsiaInput.text) / 10;
        int fioraCount = int.Parse(fioraInput.text) / 10;
        
        // Запускаємо корутини для спавну дронів
        StartCoroutine(SpawnDronesForPlanet(planetRects[0], kronusCount));
        StartCoroutine(SpawnDronesForPlanet(planetRects[1], lyrionCount));
        StartCoroutine(SpawnDronesForPlanet(planetRects[2], mystaraCount));
        StartCoroutine(SpawnDronesForPlanet(planetRects[3], eclipsiaCount));
        StartCoroutine(SpawnDronesForPlanet(planetRects[4], fioraCount));
    }

    private IEnumerator SpawnDronesForPlanet(RectTransform planetRect, int droneCount)
    {
        Vector2 planetPosition = planetRect.anchoredPosition;
        float spawnRadius = 500f; // Збільшений радіус спавну
        
        for (int i = 0; i < droneCount; i++)
        {
            // Генеруємо випадкову позицію навколо планети
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDistance = Random.Range(0f, spawnRadius);
            
            Vector2 spawnPosition = new Vector2(
                planetPosition.x + Mathf.Cos(randomAngle) * randomDistance,
                planetPosition.y + Mathf.Sin(randomAngle) * randomDistance
            );
            
            // Створюємо дрон
            GameObject droneObj = Instantiate(dronePrefab, planetRect.parent);
            RectTransform droneRect = droneObj.GetComponent<RectTransform>();
            Drone drone = droneObj.GetComponent<Drone>();
            
            if (drone != null && droneRect != null)
            {
                // Встановлюємо початкову позицію
                droneRect.anchoredPosition = spawnPosition;
                // Ініціалізуємо дрон з правильними координатами
                drone.Initialize(spawnPosition, planetPosition);
                activeDrones.Add(drone);
            }
            
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void ClearAllDrones()
    {
        foreach (Drone drone in activeDrones)
        {
            if (drone != null)
            {
                Destroy(drone.gameObject);
            }
        }
        activeDrones.Clear();
    }
} 