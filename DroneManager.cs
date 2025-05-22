using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
} 