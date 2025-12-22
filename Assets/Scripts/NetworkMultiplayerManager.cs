using System;
using System.Collections;
using ChessModel;
using Player;
using UnityEngine;
using System.Linq;

public class NetworkMultiplayerManager : MonoBehaviour
{
    [SerializeField] private ChessNetworkClient networkClient;
    [SerializeField] private BoardManager boardManager;
    
    private string gameId;
    private string playerColor;
    private GameStateData lastGameState;
    private int lastAppliedMoveCount = 0;
    
    private void OnEnable()
    {
        if (networkClient != null)
        {
            networkClient.OnGameStateChanged += HandleGameStateChanged;
            networkClient.OnError += HandleNetworkError;
        }
        if (boardManager != null)
        {
            boardManager.OnLocalMoveMade += HandleLocalMoveMade;
        }
    }

    private void OnDisable()
    {
        if (networkClient != null)
        {
            networkClient.OnGameStateChanged -= HandleGameStateChanged;
            networkClient.OnError -= HandleNetworkError;
        }
        if (boardManager != null)
        {
            boardManager.OnLocalMoveMade -= HandleLocalMoveMade;
        }
    }

    /// <summary>
    /// Create a new online game (Host)
    /// </summary>
    public void CreateOnlineGame(string color)
    {
        playerColor = color;
        networkClient.CreateGame(OnGameCreated);
    }

    private void OnGameCreated(string newGameId)
    {
        gameId = newGameId;
        Debug.Log($"📋 Game created! Share this ID: {gameId}");
        
        // Show UI to share game ID
        // Then automatically join as your selected color
        JoinOnlineGame(gameId, playerColor);
    }

    /// <summary>
    /// Join an existing online game (Client)
    /// </summary>
    public void JoinOnlineGame(string gameIdToJoin, string color)
    {
        gameId = gameIdToJoin;
        playerColor = color;
        networkClient.JoinGame(gameId, color);
    }
    
    /// <summary>
    /// Join game with automatic color selection based on what's available
    /// </summary>
    public void JoinOnlineGameAuto(string gameIdToJoin, System.Action<bool> onJoined = null)
    {
        StartCoroutine(JoinWithAutoColorCoroutine(gameIdToJoin, onJoined));
    }
    
    private IEnumerator JoinWithAutoColorCoroutine(string gameIdToJoin, System.Action<bool> onJoined)
    {
        bool joined = false;
        GameStateData stateData = null;
        
        Debug.Log($"🔍 Step 1: Querying game state for {gameIdToJoin}...");
        
        // Query game state to determine available color
        networkClient.QueryGameState(gameIdToJoin, (state) => {
            stateData = state;
        });
        
        // Wait for response
        yield return new WaitForSeconds(0.5f);
        
        if (stateData == null)
        {
            Debug.LogError($"❌ Could not query game {gameIdToJoin}");
            Debug.LogError($"   Possible reasons:");
            Debug.LogError($"   1. Game ID doesn't exist on server");
            Debug.LogError($"   2. Server is not running (check ChessServer folder)");
            Debug.LogError($"   3. Server URL is incorrect (check ChessNetworkClient.serverUrl)");
            onJoined?.Invoke(false);
            yield break;
        }
        
        Debug.Log($"🔍 Step 2: Game state retrieved successfully");
        Debug.Log($"   White player: {stateData.white_player_id ?? "NONE"}");
        Debug.Log($"   Black player: {stateData.black_player_id ?? "NONE"}");
        
        // Determine available color
        string colorToJoin = "white";
        
        if (stateData.white_player_id != null && stateData.black_player_id == null)
        {
            colorToJoin = "black";
            Debug.Log("🔍 Step 3: White is taken, joining as Black");
        }
        else if (stateData.black_player_id != null && stateData.white_player_id == null)
        {
            colorToJoin = "white";
            Debug.Log("🔍 Step 3: Black is taken, joining as White");
        }
        else if (stateData.white_player_id != null && stateData.black_player_id != null)
        {
            Debug.LogError("❌ Game is full! Both colors are taken.");
            onJoined?.Invoke(false);
            yield break;
        }
        else
        {
            // Both are null, default to black
            colorToJoin = "black";
            Debug.Log("🔍 Game is empty, defaulting to Black");
        }
        
        // Join with determined color
        JoinOnlineGame(gameIdToJoin, colorToJoin);
        onJoined?.Invoke(true);
    }

    private void HandleGameStateChanged(GameStateData gameState)
    {
        lastGameState = gameState;
        
        // Check if game is ready (both players joined)
        if (gameState.game_status == "active")
        {
            Debug.Log($"🎮 Game active! Your color: {playerColor}, Current turn: {gameState.current_turn}");
            // Ensure board only allows local player to move on their turn
            if (boardManager != null)
            {
                boardManager.onlineMultiplayer = true;
                boardManager.localHumanColor = (playerColor == "white") ? ChessColor.White : ChessColor.Black;
            }
            ApplyServerMoves(gameState);
        }
        
        // Check for game end
        if (gameState.game_status == "finished")
        {
            Debug.Log($"🏁 Game finished! Winner: {gameState.winner} ({gameState.reason})");
            OnGameEnded(gameState);
        }
    }

    private void ApplyServerMoves(GameStateData state)
    {
        if (state.moves == null || state.move_count <= 0)
            return;

        // Apply only new moves since lastAppliedMoveCount
        for (int i = lastAppliedMoveCount; i < state.move_count; i++)
        {
            var m = state.moves[i];
            // Skip moves initiated by this client (already applied locally)
            if (m.player_id == networkClient.GetPlayerId())
                continue;

            int fromIdx = SquareToIndex(m.from);
            int toIdx = SquareToIndex(m.to);
            if (fromIdx < 0 || toIdx < 0)
            {
                Debug.LogWarning($"⚠️ Could not parse server move: {m.from}->{m.to}");
                continue;
            }
            boardManager.ApplyRemoteMove(fromIdx, toIdx);
        }
        lastAppliedMoveCount = state.move_count;
    }

    private void HandleLocalMoveMade(ChessModel.Move move)
    {
        // Convert indices to algebraic squares and send to server
        string from = IndexToSquare(move.StartPosition);
        string to = IndexToSquare(move.EndPosition);
        SendMove(from, to, null);
    }

    private static int SquareToIndex(string square)
    {
        if (string.IsNullOrEmpty(square) || square.Length < 2)
            return -1;
        char fileChar = char.ToLowerInvariant(square[0]);
        if (fileChar < 'a' || fileChar > 'h')
            return -1;
        int file = fileChar - 'a';
        if (!int.TryParse(square.Substring(1), out int rank))
            return -1;
        if (rank < 1 || rank > 8)
            return -1;
        return (rank - 1) * 8 + file;
    }

    private static string IndexToSquare(int index)
    {
        int file = index % 8;
        int rank = index / 8 + 1;
        char fileChar = (char)('a' + file);
        return $"{fileChar}{rank}";
    }

    private void HandleNetworkError(string error)
    {
        Debug.LogError($"❌ Network Error: {error}");
        // Show error UI to player
    }

    /// <summary>
    /// Send a move to the server
    /// </summary>
    public void SendMove(string from, string to, string promotion = null)
    {
        if (!IsYourTurn())
        {
            Debug.LogWarning("⏸️ It's not your turn!");
            return;
        }

        networkClient.MakeMove(from, to, promotion, (success) =>
        {
            if (success)
            {
                Debug.Log($"✅ Move accepted: {from} → {to}");
            }
            else
            {
                Debug.LogWarning("❌ Move rejected by server");
            }
        });
    }

    /// <summary>
    /// Check if it's the current player's turn
    /// </summary>
    private bool IsYourTurn()
    {
        if (lastGameState == null)
            return false;

        bool isWhiteTurn = lastGameState.current_turn == "white";
        bool isYourTurn = (playerColor == "white" && isWhiteTurn) || 
                         (playerColor == "black" && !isWhiteTurn);
        return isYourTurn;
    }

    /// <summary>
    /// End the game
    /// </summary>
    public void EndOnlineGame(string winner, string reason)
    {
        networkClient.EndGame(winner, reason);
    }

    private void OnGameEnded(GameStateData gameState)
    {
        networkClient.StopPolling();
        // Notify BoardManager or UI to show end game screen
    }

    /// <summary>
    /// Disconnect from online game
    /// </summary>
    public void DisconnectFromGame()
    {
        networkClient.StopPolling();
        networkClient.DeleteGame();
        gameId = null;
    }

    public string GetGameId() => gameId;
    public string GetPlayerColor() => playerColor;
    public bool IsConnected() => gameId != null;
    public GameStateData GetLastGameState() => lastGameState;
}
