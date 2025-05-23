using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Registration Screen")]
    public GameObject registrationPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField gameIdInput;
    public Button registerButton;
    public TextMeshProUGUI registrationStatus;

    [Header("Waiting Screen")]
    public GameObject waitingPanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playersCountText;
    public TextMeshProUGUI waitingStatus;
    public TextMeshProUGUI gameIdText;

    [Header("Game Screen")]
    public GameObject gamePanel;
    public Button submitButton;
    public TextMeshProUGUI gameStatus;
    public DroneManager droneManager;

    [Header("Results Screen")]
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;

    [Header("Audio")]
    public AudioSource waitingSound;
    public AudioSource gameStartSound;
    public AudioSource submitSound;
    public AudioSource errorSound;

    private float waitTime = 180f; // 3 minutes
    private bool isWaiting = false;
    private float updateInterval = 5f; // Перевіряти кожні 5 секунд
    private Coroutine updateCoroutine;

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
        string gameId = gameIdInput.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            registrationStatus.text = "Будь ласка, введіть ім'я користувача";
            if (errorSound != null) errorSound.Play();
            return;
        }

        registerButton.interactable = false;
        registrationStatus.text = "Обробка...";

        if (!string.IsNullOrEmpty(gameId))
        {
            // Attempt to join existing game
            GameAPIClient.Instance.JoinGame(username, gameId, (success, message) =>
            {
                if (success)
                {
                    registrationStatus.text = "Приєднання успішне!";
                    ShowWaitingScreen();
                    gameIdText.text = $"ID гри: {GameAPIClient.Instance.CurrentGameId}"; // Assuming JoinGame updates CurrentGameId
                    StartWaiting();
                    StartCoroutine(UpdatePlayerCount());
                }
                else
                {
                    registrationStatus.text = "Помилка приєднання: " + message;
                    registerButton.interactable = true;
                    if (errorSound != null) errorSound.Play();
                }
            });
        }
        else
        {
            // Create a new game
            RegisterPlayer(username);
        }
    }

    private void RegisterPlayer(string username)
    {
        registrationStatus.text = "Реєстрація...";
        registerButton.interactable = false;

        GameAPIClient.Instance.RegisterPlayer(username, (success, message) =>
        {
            if (success)
            {
                registrationStatus.text = "Реєстрація успішна!";
                ShowWaitingScreen();
                gameIdText.text = $"ID гри: {GameAPIClient.Instance.CurrentGameId}";
                StartWaiting();
                StartCoroutine(UpdatePlayerCount());
            }
            else
            {
                registrationStatus.text = "Помилка реєстрації: " + message;
                registerButton.interactable = true;
                if (errorSound != null) errorSound.Play();
            }
        });
    }

    private void StartWaiting()
    {
        isWaiting = true;
        if (waitingSound != null) waitingSound.Play();
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        float remainingTime = waitTime;
        bool hasEnoughPlayers = false;
        bool timerReset = false;

        while (isWaiting && remainingTime > 0)
        {
            // Перевіряємо кількість гравців
            GameAPIClient.Instance.GetPlayerCount((success, count, status) =>
            {
                if (success)
                {
                    hasEnoughPlayers = count >= 2;
                    if (hasEnoughPlayers && !timerReset)
                    {
                        waitingStatus.text = "Гра готова до початку!";
                        remainingTime = 30f; // Скидаємо таймер до 30 секунд
                        timerReset = true;
                    }
                    else if (!hasEnoughPlayers)
                    {
                        waitingStatus.text = "Очікування гравців...";
                    }
                }
            });

            // Оновлюємо таймер
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        // Перевіряємо фінальний стан
        if (isWaiting)
        {
            if (hasEnoughPlayers)
            {
                waitingStatus.text = "Гра починається!";
                isWaiting = false;
                if (waitingSound != null && waitingSound.isPlaying) waitingSound.Stop();
                if (gameStartSound != null) gameStartSound.Play();
                ShowGameScreen();
            }
            else
            {
                waitingStatus.text = "Недостатньо гравців. Спробуйте ще раз.";
                yield return new WaitForSeconds(3f);
                ShowRegistrationScreen();
            }
        }
    }

    private void OnSubmitClick()
    {
        if (!GameAPIClient.Instance.IsGameStarted)
        {
            gameStatus.text = "Гра ще не розпочалася!";
            Debug.LogWarning("Attempted to submit move but game is not started");
            return;
        }

        if (!droneManager.ValidateStats())
        {
            Debug.LogWarning("Attempted to submit move but drone stats are invalid");
            return;
        }

        gameStatus.text = "Відправка ходу...";
        if (submitSound != null) submitSound.Play();
        SubmitMove();
    }

    private void SubmitMove()
    {
        var (kronus, lyrion, mystara, eclipsia, fiora) = droneManager.GetDroneDistribution();
        Debug.Log($"Submitting move with distribution: K:{kronus} L:{lyrion} M:{mystara} E:{eclipsia} F:{fiora}");
        
        GameAPIClient.Instance.SubmitMove(kronus, lyrion, mystara, eclipsia, fiora, (success, message) =>
        {
            if (success)
            {
                gameStatus.text = "Хід успішно відправлено! Очікування інших гравців...";
                Debug.Log("Move submitted successfully");
                
                // Отримуємо посилання на планети
                var planetRects = droneManager.GetPlanetRects();
                if (planetRects != null && planetRects.Count > 0)
                {
                    // Запускаємо спавн дронів
                    droneManager.SpawnDronesForAllPlanets(planetRects);
                }
                else
                {
                    Debug.LogError("Не вдалося отримати посилання на планети");
                }
                
                StartCoroutine(PollForResults());
            }
            else
            {
                gameStatus.text = "Помилка відправки ходу: " + message;
                Debug.LogError("Failed to submit move: " + message);
            }
        });
    }

    private IEnumerator PollForResults()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // Перевіряємо кожні 2 секунди
            
            GameAPIClient.Instance.GetResults((success, results) =>
            {
                if (success && results != null)
                {
                    Debug.Log($"Received {results.Length} results");
                    ShowResults(results);
                    StopCoroutine(PollForResults());
                }
                else
                {
                    Debug.Log("Waiting for other players to complete their moves...");
                }
            });
        }
    }

    private void GetResults()
    {
        gameStatus.text = "Отримання результатів...";
        Debug.Log("Requesting game results");
        
        GameAPIClient.Instance.GetResults((success, results) =>
        {
            if (success && results != null)
            {
                Debug.Log($"Received {results.Length} results");
                ShowResults(results);
            }
            else
            {
                gameStatus.text = "Помилка отримання результатів";
                Debug.LogError("Failed to get results");
            }
        });
    }

    private void ShowResults(GameResult[] results)
    {
        try
        {
            ShowResultsScreen();
            string resultsString = "Результати гри:\n\n";
            
            // Розраховуємо бали для кожної команди
            int[] teamScores = CalculateTeamScores(results);
            
            // Знаходимо максимальний бал
            int maxScore = 0;
            foreach (int score in teamScores)
            {
                maxScore = Math.Max(maxScore, score);
            }
            
            // Перевірка на нічию
            int winnersCount = 0;
            foreach (int score in teamScores)
            {
                if (score == maxScore) winnersCount++;
            }

            bool isDraw = winnersCount > 1;

            if (isDraw)
            {
                resultsString += "🤝 Нічия!\n\n";
            }
            else if (winnersCount == 1)
            {
                 resultsString += " Переможець раунду!\n\n";
            }
            
            // Виводимо результати
            for (int i = 0; i < results.Length; i++)
            {
                resultsString += $"Команда {i + 1}: {results[i].username}\n";
                resultsString += $"Дрони: K:{results[i].kronus} L:{results[i].lyrion} M:{results[i].mystara} E:{results[i].eclipsia} F:{results[i].fiora}\n";
                resultsString += $"Бали: {teamScores[i]}\n";
                // Додаємо позначку переможця лише якщо це не нічия і цей гравець має максимальний бал
                if (!isDraw && teamScores[i] == maxScore)
                {
                    resultsString += "\n";
                }
                resultsString += "\n";
            }
            
            resultsString += "\n";
            resultsText.text = resultsString;
            Debug.Log("Results displayed successfully");

            // Запускаємо корутину для перезапуску гри через 10 секунд
            // StartCoroutine(RestartGameAfterDelay());
        }
        catch (Exception e)
        {
            Debug.LogError("Error displaying results: " + e.Message);
            gameStatus.text = "Помилка відображення результатів";
            if (errorSound != null) errorSound.Play();
        }
    }

    private int[] CalculateTeamScores(GameResult[] results)
    {
        int[] scores = new int[results.Length];
        
        // Порівнюємо кожну команду з кожною іншою
        for (int i = 0; i < results.Length; i++)
        {
            for (int j = i + 1; j < results.Length; j++)
            {
                int team1Score = 0;
                int team2Score = 0;
                
                // Порівнюємо дрони по кожній планеті
                if (results[i].kronus > results[j].kronus) team1Score++;
                else if (results[i].kronus < results[j].kronus) team2Score++;
                
                if (results[i].lyrion > results[j].lyrion) team1Score++;
                else if (results[i].lyrion < results[j].lyrion) team2Score++;
                
                if (results[i].mystara > results[j].mystara) team1Score++;
                else if (results[i].mystara < results[j].mystara) team2Score++;
                
                if (results[i].eclipsia > results[j].eclipsia) team1Score++;
                else if (results[i].eclipsia < results[j].eclipsia) team2Score++;
                
                if (results[i].fiora > results[j].fiora) team1Score++;
                else if (results[i].fiora < results[j].fiora) team2Score++;
                
                // Нараховуємо бали
                if (team1Score > team2Score)
                {
                    scores[i] += 2; // Перемога
                }
                else if (team1Score < team2Score)
                {
                    scores[j] += 2; // Перемога
                }
                else
                {
                    scores[i] += 1; // Нічия
                    scores[j] += 1; // Нічия
                }
            }
        }
        
        return scores;
    }

    private IEnumerator UpdatePlayerCount()
    {
        while (isWaiting)
        {
            GameAPIClient.Instance.GetPlayerCount((success, count, status) =>
            {
                if (success)
                {
                    playersCountText.text = $"Гравців: {count}";
                    
                    if (count >= 2)
                    {
                        waitingStatus.text = "Гра готова до початку!";
                    }
                    else
                    {
                        waitingStatus.text = "Очікування гравців...";
                    }
                }
            });
            
            yield return new WaitForSeconds(updateInterval);
        }
    }

    [System.Serializable]
    private class PlayerCountResponse
    {
        public bool success;
        public string error;
        public GameData game;
    }

    [System.Serializable]
    private class GameData
    {
        public int id;
        public string status;
        public int player_count;
    }
}