using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameAPIClient : MonoBehaviour
{
    private const string BASE_URL = "https://7c36-91-243-2-33.ngrok-free.app/GameDev/";
    private string playerId;
    private string gameId;
    private bool isGameStarted = false;
    private float checkInterval = 2f; // Перевіряти кожні 2 секунди

    public bool IsGameStarted => isGameStarted;

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

    public void RegisterPlayer(string username, Action<bool, string> callback)
    {
        StartCoroutine(RegisterPlayerCoroutine(username, callback));
    }

    private IEnumerator RegisterPlayerCoroutine(string username, Action<bool, string> callback)
    {
        var request = new UnityWebRequest(BASE_URL + "register.php", "POST");
        var jsonData = JsonUtility.ToJson(new { username = username });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    playerId = response.player_id;
                    gameId = response.game_id;
                    callback(true, "Реєстрація успішна");
                }
                else
                {
                    callback(false, response.message);
                }
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

        var request = new UnityWebRequest(BASE_URL + "submit_move.php", "POST");
        var jsonData = JsonUtility.ToJson(moveData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<MoveResponse>(request.downloadHandler.text);
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

    public void GetResults(Action<bool, GameResult[]> callback)
    {
        StartCoroutine(GetResultsCoroutine(callback));
    }

    private IEnumerator GetResultsCoroutine(Action<bool, GameResult[]> callback)
    {
        var request = UnityWebRequest.Get(BASE_URL + "get_results.php?game_id=" + gameId);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<ResultsResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    callback(true, response.results);
                }
                else
                {
                    callback(false, null);
                }
            }
            catch (Exception e)
            {
                callback(false, null);
                Debug.LogError("Помилка обробки результатів: " + e.Message);
            }
        }
        else
        {
            callback(false, null);
            Debug.LogError("Помилка мережі: " + request.error);
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
            StartCoroutine(DisconnectCoroutine((success, message) => 
            {
                Debug.Log("Відключення при закритті: " + message);
            }));
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