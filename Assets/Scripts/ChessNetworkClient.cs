using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ChessNetworkClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://localhost:5000";
    
    private string gameId;
    private string playerId;
    private string playerColor;
    
    public delegate void GameStateChangedDelegate(GameStateData gameState);
    public event GameStateChangedDelegate OnGameStateChanged;
    
    public delegate void ErrorDelegate(string errorMessage);
    public event ErrorDelegate OnError;
    
    private Coroutine pollCoroutine;

    private void Awake()
    {
        if (playerId == null)
        {
            playerId = System.Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Create a new game on the server
    /// </summary>
    public void CreateGame(Action<string> onGameCreated)
    {
        StartCoroutine(CreateGameCoroutine(onGameCreated));
    }

    private IEnumerator CreateGameCoroutine(Action<string> onGameCreated)
    {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{serverUrl}/game/create", ""))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<CreateGameResponse>(request.downloadHandler.text);
                gameId = response.game_id;
                onGameCreated?.Invoke(gameId);
                Debug.Log($"✅ Game created: {gameId}");
            }
            else
            {
                string error = $"Failed to create game: {request.error}";
                OnError?.Invoke(error);
                Debug.LogError(error);
            }
        }
    }

    /// <summary>
    /// Join an existing game
    /// </summary>
    public void JoinGame(string joinGameId, string color)
    {
        gameId = joinGameId;
        playerColor = color;
        StartCoroutine(JoinGameCoroutine(color));
    }

    private IEnumerator JoinGameCoroutine(string color)
    {
        var joinData = new JoinGameRequest
        {
            player_id = playerId,
            color = color
        };

        string jsonData = JsonUtility.ToJson(joinData);

        using (UnityWebRequest request = new UnityWebRequest($"{serverUrl}/game/{gameId}/join", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GameStateResponse>(request.downloadHandler.text);
                OnGameStateChanged?.Invoke(response.game_state);
                Debug.Log($"✅ Joined game as {color}");
                StartPolling();
            }
            else
            {
                string error = $"Failed to join game: {request.error}";
                OnError?.Invoke(error);
                Debug.LogError(error);
            }
        }
    }

    /// <summary>
    /// Make a move in the game
    /// </summary>
    public void MakeMove(string from, string to, string promotion = null, Action<bool> onMoveResult = null)
    {
        StartCoroutine(MakeMoveCoroutine(from, to, promotion, onMoveResult));
    }

    private IEnumerator MakeMoveCoroutine(string from, string to, string promotion, Action<bool> onMoveResult)
    {
        var moveData = new MoveRequest
        {
            player_id = playerId,
            from = from,
            to = to,
            promotion = promotion
        };

        string jsonData = JsonUtility.ToJson(moveData);

        using (UnityWebRequest request = new UnityWebRequest($"{serverUrl}/game/{gameId}/move", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GameStateResponse>(request.downloadHandler.text);
                if (response.success)
                {
                    OnGameStateChanged?.Invoke(response.game_state);
                    onMoveResult?.Invoke(true);
                    Debug.Log($"✅ Move made: {from} -> {to}");
                }
                else
                {
                    OnError?.Invoke(response.error ?? "Move failed");
                    onMoveResult?.Invoke(false);
                }
            }
            else
            {
                string error = $"Failed to make move: {request.error}";
                OnError?.Invoke(error);
                onMoveResult?.Invoke(false);
                Debug.LogError(error);
            }
        }
    }

    /// <summary>
    /// Get current game state
    /// </summary>
    public void GetGameState(Action<GameStateData> onStateReceived = null)
    {
        StartCoroutine(GetGameStateCoroutine(onStateReceived));
    }

    private IEnumerator GetGameStateCoroutine(Action<GameStateData> onStateReceived)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}/game/{gameId}/state"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GameStateResponse>(request.downloadHandler.text);
                OnGameStateChanged?.Invoke(response.game_state);
                onStateReceived?.Invoke(response.game_state);
            }
            else
            {
                Debug.LogError($"Failed to get game state: {request.error}");
            }
        }
    }

    /// <summary>
    /// Start polling for game state updates (every 500ms)
    /// </summary>
    private void StartPolling()
    {
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
        }
        pollCoroutine = StartCoroutine(PollGameState());
    }

    private IEnumerator PollGameState()
    {
        while (gameId != null)
        {
            yield return new WaitForSeconds(0.5f);
            GetGameState();
        }
    }

    /// <summary>
    /// Stop polling
    /// </summary>
    public void StopPolling()
    {
        if (pollCoroutine != null)
        {
            StopCoroutine(pollCoroutine);
        }
    }

    /// <summary>
    /// End the game
    /// </summary>
    public void EndGame(string winner, string reason)
    {
        StartCoroutine(EndGameCoroutine(winner, reason));
    }

    private IEnumerator EndGameCoroutine(string winner, string reason)
    {
        var endData = new EndGameRequest
        {
            winner = winner,
            reason = reason
        };

        string jsonData = JsonUtility.ToJson(endData);

        using (UnityWebRequest request = new UnityWebRequest($"{serverUrl}/game/{gameId}/end", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Game ended on server");
                StopPolling();
            }
            else
            {
                Debug.LogError($"Failed to end game: {request.error}");
            }
        }
    }

    /// <summary>
    /// Delete game session from server
    /// </summary>
    public void DeleteGame()
    {
        StartCoroutine(DeleteGameCoroutine());
    }

    private IEnumerator DeleteGameCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Delete($"{serverUrl}/game/{gameId}/delete"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Game deleted from server");
                StopPolling();
                gameId = null;
            }
            else
            {
                Debug.LogError($"Failed to delete game: {request.error}");
            }
        }
    }

    public string GetGameId() => gameId;
    public string GetPlayerId() => playerId;
    public string GetPlayerColor() => playerColor;
}

// ==================== DATA STRUCTURES ====================

[System.Serializable]
public class CreateGameResponse
{
    public bool success;
    public string game_id;
    public string message;
}

[System.Serializable]
public class JoinGameRequest
{
    public string player_id;
    public string color;
}

[System.Serializable]
public class MoveRequest
{
    public string player_id;
    public string from;
    public string to;
    public string promotion;
}

[System.Serializable]
public class EndGameRequest
{
    public string winner;
    public string reason;
}

[System.Serializable]
public class GameStateData
{
    public string game_id;
    public string white_player_id;
    public string black_player_id;
    public string board_state;
    public string current_turn;
    public string game_status;
    public string winner;
    public string reason;
    public int move_count;
    public MoveEntry[] moves; // server-provided chronological move list
}

[System.Serializable]
public class GameStateResponse
{
    public bool success;
    public GameStateData game_state;
    public string error;
}

[System.Serializable]
public class MoveEntry
{
    public string player_id;
    public string color; // "white" or "black"
    public string from;  // e.g., "e2"
    public string to;    // e.g., "e4"
    public string promotion; // e.g., "q" or null
    public int move_number;
}
