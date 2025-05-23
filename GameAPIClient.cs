using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameAPIClient : MonoBehaviour
{
    private const string BASE_URL = "https://7985-91-243-2-33.ngrok-free.app/GameDev/";
    private string playerId;
    private string gameId;
    private bool isGameStarted = false;
    private float checkInterval = 2f; // Перевіряти кожні 2 секунди

    public bool IsGameStarted => isGameStarted;
    public string CurrentGameId => gameId;

    public static GameAPIClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(CheckGameStatusRoutine());
    }

    private IEnumerator CheckGameStatusRoutine()
    {
        while (true)
        {
            if (!string.IsNullOrEmpty(playerId))
            {
                yield return StartCoroutine(CheckGameStatus());
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    [Serializable]
    private class RegisterData
    {
        public string username;
    }

    public void RegisterPlayer(string username, Action<bool, string> callback)
    {
        StartCoroutine(RegisterPlayerCoroutine(username, callback));
    }

    public void JoinGame(string username, string gameId, Action<bool, string> callback)
    {
        StartCoroutine(JoinGameCoroutine(username, gameId, callback));
    }

    private IEnumerator JoinGameCoroutine(string username, string gameId, Action<bool, string> callback)
    {
        var request = new UnityWebRequest(BASE_URL + "join_game.php", "POST");
        var joinData = new JoinGameData { username = username, game_id = gameId };
        var jsonData = JsonUtility.ToJson(joinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Debug.Log("Join game response: " + request.downloadHandler.text);
                var response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    playerId = response.player_id;
                    this.gameId = gameId;
                    Debug.Log($"Join game successful. Player ID: {playerId}, Game ID: {gameId}");
                    callback(true, "Приєднання успішне");
                }
                else
                {
                    Debug.LogError("Join game failed: " + response.message);
                    callback(false, response.message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing join game response: " + e.Message);
                callback(false, "Помилка обробки відповіді: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Network error during join game: " + request.error);
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError("Server Response Body: " + request.downloadHandler.text);
            }
            callback(false, "Помилка мережі: " + request.error);
        }
    }

    [Serializable]
    private class JoinGameData
    {
        public string username;
        public string game_id;
    }

    private IEnumerator RegisterPlayerCoroutine(string username, Action<bool, string> callback)
    {
        var request = new UnityWebRequest(BASE_URL + "register.php", "POST");
        var registrationData = new RegisterData { username = username };
        var jsonData = JsonUtility.ToJson(registrationData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Debug.Log("Registration response: " + request.downloadHandler.text);
                var response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    playerId = response.player_id;
                    gameId = response.game_id;
                    Debug.Log($"Registration successful. Player ID: {playerId}, Game ID: {gameId}");
                    
                    // Перевіряємо, чи game_id не пустий
                    if (string.IsNullOrEmpty(gameId))
                    {
                        Debug.LogError("Game ID is empty after registration!");
                        callback(false, "Помилка: Game ID не отримано");
                        yield break;
                    }
                    
                    callback(true, "Реєстрація успішна");
                }
                else
                {
                    Debug.LogError("Registration failed: " + response.message);
                    callback(false, response.message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing registration response: " + e.Message);
                callback(false, "Помилка обробки відповіді: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Network error during registration: " + request.error);
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError("Server Response Body: " + request.downloadHandler.text);
            }
            callback(false, "Помилка мережі: " + request.error);
        }
    }

    public IEnumerator CheckGameStatus()
    {
        var request = UnityWebRequest.Get(BASE_URL + "start_game.php?player_id=" + playerId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<GameStatusResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    if (response.status == "in_progress" && !isGameStarted)
                    {
                        isGameStarted = true;
                        // Тут можна додати подію для початку гри
                        Debug.Log("Гра почалася!");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Помилка обробки статусу: " + e.Message);
            }
        }
    }

    public void SubmitMove(int kronus, int lyrion, int mystara, int eclipsia, int fiora, Action<bool, string> callback)
    {
        StartCoroutine(SubmitMoveCoroutine(kronus, lyrion, mystara, eclipsia, fiora, callback));
    }

    private IEnumerator SubmitMoveCoroutine(int kronus, int lyrion, int mystara, int eclipsia, int fiora, Action<bool, string> callback)
    {
        var moveData = new MoveData
        {
            player_id = playerId,
            game_id = gameId,
            kronus = kronus,
            lyrion = lyrion,
            mystara = mystara,
            eclipsia = eclipsia,
            fiora = fiora
        };

        Debug.Log($"Submitting move for game {gameId}, player {playerId}");
        Debug.Log($"Move data: K:{kronus} L:{lyrion} M:{mystara} E:{eclipsia} F:{fiora}");

        var request = new UnityWebRequest(BASE_URL + "submit_move.php", "POST");
        var jsonData = JsonUtility.ToJson(moveData);
        Debug.Log("Sending JSON data: " + jsonData);
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Server response: " + request.downloadHandler.text);
            try
            {
                var response = JsonUtility.FromJson<MoveResponse>(request.downloadHandler.text);
                callback(response.success, response.message);
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing response: " + e.Message);
                callback(false, "Помилка обробки відповіді: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Network error: " + request.error);
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError("Server Response: " + request.downloadHandler.text);
            }
            callback(false, "Помилка мережі: " + request.error);
        }
    }

    public void GetResults(Action<bool, GameResult[]> callback)
    {
        StartCoroutine(GetResultsCoroutine(callback));
    }

    private IEnumerator GetResultsCoroutine(Action<bool, GameResult[]> callback)
    {
        Debug.Log($"Getting results for game {gameId}");
        var request = UnityWebRequest.Get(BASE_URL + "get_results.php?game_id=" + gameId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Get results response: " + request.downloadHandler.text);
            try
            {
                var response = JsonUtility.FromJson<ResultsResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    Debug.Log($"Successfully got {response.results?.Length ?? 0} results");
                    callback(true, response.results);
                }
                else
                {
                    Debug.LogError("Failed to get results: " + (response.message ?? "Unknown error"));
                    callback(false, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing results: " + e.Message);
                callback(false, null);
            }
        }
        else
        {
            Debug.LogError("Network error getting results: " + request.error);
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError("Server Response: " + request.downloadHandler.text);
            }
            callback(false, null);
        }
    }

    public void Disconnect(Action<bool, string> callback)
    {
        StartCoroutine(DisconnectCoroutine(callback));
    }

    private IEnumerator DisconnectCoroutine(Action<bool, string> callback)
    {
        var request = new UnityWebRequest(BASE_URL + "disconnect.php", "POST");
        var jsonData = JsonUtility.ToJson(new { player_id = playerId, game_id = gameId });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<DisconnectResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    playerId = null;
                    gameId = null;
                    isGameStarted = false;
                }
                callback(response.success, response.message);
            }
            catch (Exception e)
            {
                callback(false, "Помилка обробки відповіді: " + e.Message);
            }
        }
        else
        {
            callback(false, "Помилка мережі: " + request.error);
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(playerId) && !string.IsNullOrEmpty(gameId))
        {
            // Створюємо новий об'єкт для відправки запиту
            var disconnectObject = new GameObject("DisconnectHelper");
            var helper = disconnectObject.AddComponent<DisconnectHelper>();
            helper.Disconnect(playerId, gameId, (success, message) => 
            {
                Debug.Log("Відключення при закритті: " + message);
                Destroy(disconnectObject);
            });
        }
    }

    public void GetPlayerCount(Action<bool, int, string> callback)
    {
        StartCoroutine(GetPlayerCountCoroutine(callback));
    }

    [Serializable]
    public class GetPlayersRequest
    {
        public string game_id;
    }

    private IEnumerator GetPlayerCountCoroutine(Action<bool, int, string> callback)
    {
        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError("Game ID is not set. Current gameId: " + (gameId ?? "null"));
            callback(false, 0, "error");
            yield break;
        }

        Debug.Log("Getting player count for game ID: " + gameId);
        var request = new UnityWebRequest(BASE_URL + "get_players.php", "POST");
        
        // Створюємо JSON об'єкт
        var requestData = new GetPlayersRequest { game_id = gameId };
        var jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("Sending JSON data: " + jsonData);
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                Debug.Log("Get players response: " + request.downloadHandler.text);
                var response = JsonUtility.FromJson<PlayerCountResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    callback(true, response.game.player_count, response.game.status);
                    
                    // Оновлюємо статус гри
                    if (response.game.status == "in_progress")
                    {
                        isGameStarted = true;
                    }
                }
                else
                {
                    Debug.LogError("Server error: " + (string.IsNullOrEmpty(response.error) ? "Unknown error" : response.error));
                    callback(false, 0, "error");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing response: " + e.Message);
                callback(false, 0, "error");
            }
        }
        else
        {
            Debug.LogError("Network error: " + request.error);
            if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogError("Server Response: " + request.downloadHandler.text);
            }
            callback(false, 0, "error");
        }
    }
}

[Serializable]
public class RegisterResponse
{
    public bool success;
    public string player_id;
    public string game_id;
    public string message;
}

[Serializable]
public class GameStatusResponse
{
    public bool success;
    public string status;
    public int players_count;
    public string message;
}

[Serializable]
public class MoveData
{
    public string player_id;
    public string game_id;
    public int kronus;
    public int lyrion;
    public int mystara;
    public int eclipsia;
    public int fiora;
}

[Serializable]
public class MoveResponse
{
    public bool success;
    public string message;
}

[Serializable]
public class ResultsResponse
{
    public bool success;
    public GameResult[] results;
    public string message;
}

[Serializable]
public class GameResult
{
    public string username;
    public int score;
    public int kronus;
    public int lyrion;
    public int mystara;
    public int eclipsia;
    public int fiora;
}

[Serializable]
public class DisconnectResponse
{
    public bool success;
    public string message;
}

[Serializable]
public class PlayerCountResponse
{
    public bool success;
    public string error;
    public GameInfo game;
}

[Serializable]
public class GameInfo
{
    public int player_count;
    public string status;
}

// Допоміжний клас для відправки запиту на відключення
public class DisconnectHelper : MonoBehaviour
{
    private const string BASE_URL = "https://7985-91-243-2-33.ngrok-free.app/GameDev/";

    public void Disconnect(string playerId, string gameId, Action<bool, string> callback)
    {
        StartCoroutine(DisconnectCoroutine(playerId, gameId, callback));
    }

    private IEnumerator DisconnectCoroutine(string playerId, string gameId, Action<bool, string> callback)
    {
        var request = new UnityWebRequest(BASE_URL + "disconnect.php", "POST");
        var jsonData = JsonUtility.ToJson(new { player_id = playerId, game_id = gameId });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<DisconnectResponse>(request.downloadHandler.text);
                callback(response.success, response.message);
            }
            catch (Exception e)
            {
                callback(false, "Помилка обробки відповіді: " + e.Message);
            }
        }
        else
        {
            callback(false, "Помилка мережі: " + request.error);
        }
    }
} 