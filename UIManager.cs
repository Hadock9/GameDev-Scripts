using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Registration Screen")]
    public GameObject registrationPanel;
    public TMP_InputField usernameInput;
    public Button registerButton;
    public TextMeshProUGUI registrationStatus;

    [Header("Waiting Screen")]
    public GameObject waitingPanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playersCountText;
    public TextMeshProUGUI waitingStatus;

    [Header("Game Screen")]
    public GameObject gamePanel;
    public Button submitButton;
    public TextMeshProUGUI gameStatus;
    public DroneManager droneManager;

    [Header("Results Screen")]
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;

    [Header("Audio")]
    //public AudioSource waitingSound;
    //public AudioSource gameStartSound;
    //public AudioSource submitSound;
    //public AudioSource errorSound;

    private float waitTime = 180f; // 3 minutes
    private bool isWaiting = false;

    private void Start()
    {
        ShowRegistrationScreen();
        registerButton.onClick.AddListener(OnRegisterClick);
        submitButton.onClick.AddListener(OnSubmitClick);
    }

    private void ShowRegistrationScreen()
    {
        registrationPanel.SetActive(true);
        waitingPanel.SetActive(false);
        gamePanel.SetActive(false);
        resultsPanel.SetActive(false);
    }

    private void ShowWaitingScreen()
    {
        registrationPanel.SetActive(false);
        waitingPanel.SetActive(true);
        gamePanel.SetActive(false);
        resultsPanel.SetActive(false);
    }

    private void ShowGameScreen()
    {
        registrationPanel.SetActive(false);
        waitingPanel.SetActive(false);
        gamePanel.SetActive(true);
        resultsPanel.SetActive(false);
    }

    private void ShowResultsScreen()
    {
        registrationPanel.SetActive(false);
        waitingPanel.SetActive(false);
        gamePanel.SetActive(false);
        resultsPanel.SetActive(true);
    }

    private void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            registrationStatus.text = "❌ Будь ласка, введіть ім'я користувача";
            //errorSound.Play();
            return;
        }

        RegisterPlayer(username);
    }

    private void RegisterPlayer(string username)
    {
        registrationStatus.text = "⏳ Реєстрація...";
        registerButton.interactable = false;

        GameAPIClient.Instance.RegisterPlayer(username, (success, message) =>
        {
            if (success)
            {
                registrationStatus.text = "✅ Реєстрація успішна!";
                ShowWaitingScreen();
                StartWaiting();
            }
            else
            {
                registrationStatus.text = "❌ Помилка реєстрації: " + message;
                registerButton.interactable = true;
                //errorSound.Play();
            }
        });
    }

    private void StartWaiting()
    {
        isWaiting = true;
        //waitingSound.Play();
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        float remainingTime = waitTime;
        while (isWaiting && remainingTime > 0 && !GameAPIClient.Instance.IsGameStarted)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (isWaiting && !GameAPIClient.Instance.IsGameStarted)
        {
            waitingStatus.text = "❌ Недостатньо гравців. Спробуйте ще раз.";
            //waitingSound.Stop();
            //errorSound.Play();
            yield return new WaitForSeconds(3f);
            ShowRegistrationScreen();
        }
        else if (GameAPIClient.Instance.IsGameStarted && isWaiting)
        {
             isWaiting = false;
             //waitingSound.Stop();
             //gameStartSound.Play();
             ShowGameScreen();
        }
    }

    private void OnSubmitClick()
    {
        if (!GameAPIClient.Instance.IsGameStarted)
        {
             gameStatus.text = "❌ Гра ще не розпочалася!";
             //errorSound.Play();
             return;
        }

        if (!droneManager.ValidateStats())
        {
            //errorSound.Play();
            return;
        }

        //submitSound.Play();
        SubmitMove();
    }

    private void SubmitMove()
    {
        var (kronus, lyrion, mystara, eclipsia, fiora) = droneManager.GetDroneDistribution();
        
        GameAPIClient.Instance.SubmitMove(kronus, lyrion, mystara, eclipsia, fiora, (success, message) =>
        {
            if (success)
            {
                GetResults();
            }
            else
            {
                gameStatus.text = "❌ Помилка відправки ходу: " + message;
                //errorSound.Play();
            }
        });
    }

    private void GetResults()
    {
        GameAPIClient.Instance.GetResults((success, results) =>
        {
            if (success && results != null)
            {
                ShowResults(results);
            }
            else
            {
                gameStatus.text = "❌ Помилка отримання результатів.";
                //errorSound.Play();
            }
        });
    }

    private void ShowResults(GameResult[] results)
    {
        ShowResultsScreen();
        string resultsString = "Результати гри:\n\n";
        foreach (var result in results)
        {
            resultsString += $"Гравець: {result.username}\n";
            resultsString += $"Очки: {result.score}\n";
            resultsString += $"Дрони: K:{result.kronus} L:{result.lyrion} M:{result.mystara} E:{result.eclipsia} F:{result.fiora}\n\n";
        }
        resultsText.text = resultsString;
    }
}