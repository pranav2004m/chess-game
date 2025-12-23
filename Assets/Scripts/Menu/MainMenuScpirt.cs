using System;
using System.Collections;
using System.Collections.Generic;
using ChessModel;
using Player;
using UnityEngine;
using TMPro;

public enum Difficulty
{
    Random = 0,
    Easy = 2,
    Medium = 4,
    Hard = 6
}

public class MainMenuScpirt : MonoBehaviour {

    public GameObject firstMenu;
    public GameObject modeSelection;
    public GameObject colorSelection;
    public GameObject multiplayerColorSelection;
    public GameObject aiDifficulty;
    public GameObject pauseMenu;
    
    public BoardManager boardManager;
    public NetworkMultiplayerManager networkManager;
    public MultiplayerMenuUI multiplayerMenuUI;
    
    [Header("Multiplayer UI Elements")]
    public TMP_Text gameIdDisplayText; // Text field to display game ID on color selection screen
    public GameObject blackColorButton; // Reference to black button to hide for host

    private GameMode _gameMode;
    private Dictionary<ChessColor, Player.Player> _players;
    private ChessColor _playerColor;
    
    // Multiplayer state tracking
    private bool _isMultiplayerHost = false;
    private string _pendingGameId = "";
    private bool _waitingToStartGame = false; // Flag to indicate game is created and waiting for Start Play

    // Testing helpers to control host/join behavior per editor instance (DEPRECATED - use UI instead)
    [SerializeField] private bool joinExistingGameForTesting = false;
    [SerializeField] private string testGameId = ""; // Set this in Editor 2 to the host's Game ID
    [SerializeField] private string testJoinColor = "black"; // "white" or "black"
    
    void Start()
    {
        firstMenu.SetActive(true);
        modeSelection.SetActive(false);
        colorSelection.SetActive(false);
        if (multiplayerColorSelection != null)
        {
            multiplayerColorSelection.SetActive(false);
        }
        aiDifficulty.SetActive(false);
        pauseMenu.SetActive(false);

        _players = new Dictionary<ChessColor, Player.Player>
        {
            [ChessColor.Black] = null,
            [ChessColor.White] = null
        };

        _playerColor = ChessColor.White;
    }

    private void Update()
    {
        if (boardManager.playing && Input.GetKeyDown(KeyCode.Escape))
        {
            boardManager.playing = false;
            pauseMenu.SetActive(true);
        }
        else if (pauseMenu.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            boardManager.playing = true;
            pauseMenu.SetActive(false);
        }
    }

    public void Resume()
    {
        boardManager.playing = true;
        pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        boardManager.RestartGame();
        pauseMenu.SetActive(false);
    }

    private IEnumerator TravelToMenu()
    {
        yield return new WaitForSeconds(2f);
        boardManager.RestartGame();
        boardManager.playing = false;
        firstMenu.SetActive(true);
        _players = new Dictionary<ChessColor, Player.Player>
        {
            [ChessColor.Black] = null,
            [ChessColor.White] = null
        };

        _playerColor = ChessColor.White;
    }
    public void BackToMenu()
    {
        pauseMenu.SetActive(false);
        boardManager.menuCam.SetActive(true);
        boardManager.GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().Play();
        boardManager.whiteCam.SetActive(true);
        StartCoroutine(TravelToMenu());
    }

    public void SelectColorWhite()
    {
        // If waiting to start multiplayer game, just start it
        if (_waitingToStartGame)
        {
            Debug.Log("[Multiplayer] Starting game as White...");
            _waitingToStartGame = false;
            StartGame();
            return;
        }
        
        // White button should select White
        SelectColor(ChessColor.White);
    }
    
    public void SelectColorBlack()
    {
        // Black button should select Black
        SelectColor(ChessColor.Black);
    }

    private void SelectColor(ChessColor color)
    {
        _playerColor = color;
        if (_gameMode == GameMode.OnlineMultiplayer)
        {
            if (multiplayerColorSelection != null)
            {
                multiplayerColorSelection.SetActive(false);
            }
        }
        else
        {
            if (colorSelection != null)
            {
                colorSelection.SetActive(false);
            }
        }
        
        if (_gameMode == GameMode.OnlineMultiplayer)
        {
            string colorString = _playerColor == ChessColor.White ? "white" : "black";
            
            // Join flow - join the game with selected color
            if (!_isMultiplayerHost && !string.IsNullOrEmpty(_pendingGameId))
            {
                JoinOnlineGame(_pendingGameId, colorString);
            }
            // Note: Host flow is handled by CreateOnlineGameAndDisplayId now
        }
        else
        {
            ShowAiDifficulty();
        }
    }

    private void ShowColorSelection()
    {
        if (_gameMode == GameMode.Pvai)
        {
            colorSelection.SetActive(true);
        }
        else
        {
            ShowAiDifficulty();
            _playerColor = ChessColor.White;
        }
    }

    private void StartGame()
    {
        firstMenu.SetActive(false);
        modeSelection.SetActive(false);
        colorSelection.SetActive(false);
        if (multiplayerColorSelection != null)
        {
            multiplayerColorSelection.SetActive(false);
        }
        aiDifficulty.SetActive(false);
        
        GetComponent<AudioSource>().Stop();
        boardManager.InitialisePlay(_players);
    }

    private void ShowAiDifficulty()
    {
        if (_gameMode == GameMode.Pvai || _gameMode == GameMode.Aivai)
        {
            aiDifficulty.SetActive(true);
        }
        else
        {
            StartGame();
        }
    }

    public void SelectDifficultyRandom()
    {
        AssignDifficulty(Difficulty.Random);
    }
    
    public void SelectDifficultyEasy()
    {
        AssignDifficulty(Difficulty.Easy);
    }
    
    public void SelectDifficultyMedium()
    {
        AssignDifficulty(Difficulty.Medium);
    }
    
    public void SelectDifficultyHard()
    {
        AssignDifficulty(Difficulty.Hard);
    }

    private void AssignDifficulty(Difficulty difficulty)
    {
        var aiColor = _playerColor.Reverse();
        Player.Player player = difficulty == Difficulty.Random ? new RandomPlayer(aiColor) : (Player.Player) new MinmaxPlayer(aiColor, (int)difficulty);

        _players[aiColor] = player;
        _players[_playerColor] = null;
        
        if (_gameMode == GameMode.Aivai && _playerColor == ChessColor.White)
        {
            // Specifically for AI vs AI, we might need two AIs?
            // The logic here seems to handle one AI creation.
            // If GameMode is AI vs AI, standard flow might be different.
            // But for PvAI, the above is correct.
            // Let's preserve existing AI vs AI check logic but adapting to new variable names if needed.
            // Originals: if (_gameMode == GameMode.Aivai && _playerColor == ChessColor.White) _playerColor = ChessColor.Black; 
            // This seems to be a mechanism to swap turn/color or something. 
            // I'll keep the block below as is, just noting the AI assignment above.
            _playerColor = ChessColor.Black;
        }
        else
        {
            StartGame();
            aiDifficulty.SetActive(false);
        }
    }
    

    private void SelectGameMode(GameMode gameMode)
    {
        modeSelection.SetActive(false);
        _gameMode = gameMode;
        ShowColorSelection();
    }

    public void SelectPvpGameMode()
    {
        SelectGameMode(GameMode.Pvp);
    }

    public void SelectPvAiGameMode()
    {
        SelectGameMode(GameMode.Pvai);
    }

    public void SelectAivaiGameMode()
    {
        SelectGameMode(GameMode.Aivai);
    }

    public void SelectOnlineMultiplayerMode()
    {
        // Check if using legacy testing mode (DEPRECATED)
        if (joinExistingGameForTesting && !string.IsNullOrEmpty(testGameId))
        {
            string joinColor = string.IsNullOrEmpty(testJoinColor) ? "white" : testJoinColor.ToLowerInvariant();
            if (joinColor != "white" && joinColor != "black")
            {
                joinColor = "white";
            }
            Debug.Log($"[Online] DEPRECATED: Join override enabled. Joining game {testGameId} as {joinColor}.");
            JoinOnlineGame(testGameId, joinColor);
            return;
        }

        // Show the new multiplayer menu UI
        _gameMode = GameMode.OnlineMultiplayer;
        modeSelection.SetActive(false);
        
        if (multiplayerMenuUI != null)
        {
            multiplayerMenuUI.ShowMultiplayerMenu();
        }
        else
        {
            Debug.LogError("MultiplayerMenuUI not assigned! Using fallback host flow.");
            // Fallback to old behavior
            colorSelection.SetActive(true);
            _isMultiplayerHost = true;
        }
    }

    public void ToMainMenuFromModeSelection()
    {
        firstMenu.SetActive(true);
        modeSelection.SetActive(false);
    }

    public void ToGameModeSelectionFromMainMenu()
    {
        firstMenu.SetActive(false);
        modeSelection.SetActive(true);
    }
    
    /// <summary>
    /// Called by MultiplayerMenuUI to show color selection for host or join
    /// </summary>
    public void ShowColorSelectionForMultiplayer(bool isHost, string gameId = "")
    {
        _isMultiplayerHost = isHost;
        _pendingGameId = gameId;
        _gameMode = GameMode.OnlineMultiplayer;
        
        if (colorSelection != null)
        {
            colorSelection.SetActive(false);
        }
        if (multiplayerColorSelection != null)
        {
            multiplayerColorSelection.SetActive(true);
        }
        
        if (isHost)
        {
            // Host flow: Create game immediately and display ID
            Debug.Log("[Multiplayer] Host - Creating game as White...");
            
            // Hide black button for host
            if (blackColorButton != null)
            {
                blackColorButton.SetActive(false);
            }
            
            // Hide game ID initially, will show after creation
            if (gameIdDisplayText != null)
            {
                gameIdDisplayText.gameObject.SetActive(false);
            }
            
            // Create the game as white
            CreateOnlineGameAndDisplayId("white");
        }
        else
        {
            // Join flow: Show both buttons, hide game ID
            Debug.Log($"[Multiplayer] Join - Select your color to join game {gameId}");
            
            if (blackColorButton != null)
            {
                blackColorButton.SetActive(true);
            }
            
            if (gameIdDisplayText != null)
            {
                gameIdDisplayText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Called by MultiplayerMenuUI back button
    /// </summary>
    public void ReturnToModeSelection()
    {
        modeSelection.SetActive(true);
        _isMultiplayerHost = false;
        _pendingGameId = "";
    }
    
    public void Exit()
    {
        Application.Quit();
    }

    // ========== ONLINE MULTIPLAYER METHODS ==========
    
    /// <summary>
    /// Create game and display ID on screen, wait for user to click Start Play
    /// </summary>
    private void CreateOnlineGameAndDisplayId(string color)
    {
        if (networkManager == null)
        {
            Debug.LogError("NetworkMultiplayerManager not assigned in Inspector!");
            return;
        }
        
        // Create game
        networkManager.CreateOnlineGame(color);
        
        // Display the game ID and wait for user to click Start Play
        StartCoroutine(DisplayGameIdAfterCreation());
    }
    
    private IEnumerator DisplayGameIdAfterCreation()
    {
        // Wait for game ID to be set by NetworkMultiplayerManager
        string gameId = null;
        float timeout = 5f;
        float elapsed = 0f;
        
        while (string.IsNullOrEmpty(gameId) && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
            gameId = networkManager.GetGameId();
        }
        
        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError("Failed to get game ID after timeout!");
            yield break;
        }
        
        Debug.Log("═══════════════════════════════════════════════");
        Debug.Log($"🎮 GAME CREATED! Share this ID with your opponent:");
        Debug.Log($"📋 Game ID: {gameId}");
        Debug.Log("═══════════════════════════════════════════════");
        
        // Display game ID on screen
        if (gameIdDisplayText != null)
        {
            gameIdDisplayText.text = $"Game ID: {gameId}\n";
            gameIdDisplayText.gameObject.SetActive(true);
        }
        
        // Set flag so Start Play button will work
        _waitingToStartGame = true;
    }
    
    public void JoinOnlineGame(string gameId, string color)
    {
        if (networkManager == null)
        {
            Debug.LogError("NetworkMultiplayerManager not assigned in Inspector!");
            return;
        }
        
        networkManager.JoinOnlineGame(gameId, color);
        Debug.Log($"Joining game {gameId} as {color}");
        
        StartGame();
    }
    
    /// <summary>
    /// Join game directly without color selection - auto-picks available color
    /// </summary>
    public void JoinGameDirectly(string gameId)
    {
        if (networkManager == null)
        {
            Debug.LogError("NetworkMultiplayerManager not assigned in Inspector!");
            return;
        }
        
        _gameMode = GameMode.OnlineMultiplayer;
        _isMultiplayerHost = false;
        
        // Query server to determine available color and join
        StartCoroutine(JoinGameWithAutoColor(gameId));
    }
    
    private IEnumerator JoinGameWithAutoColor(string gameId)
    {
        Debug.Log($"🔗 Joining game {gameId} as Black (default joining color)");
        
        // Simply join as black - no server query needed
        networkManager.JoinOnlineGame(gameId, "black");
        
        // Wait a moment for connection
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"✅ Joined game {gameId}");
        StartGame();
    }

    public enum GameMode {
        Pvp,
        Pvai,
        Aivai,
        OnlineMultiplayer
    }

    // Dedicated join action to wire to a button for Editor 2
    public void JoinOnlineNow()
    {
        if (!joinExistingGameForTesting || string.IsNullOrEmpty(testGameId))
        {
            Debug.LogError("[Online] JoinOnlineNow called but test join settings are not configured.");
            return;
        }
        var joinColor = string.IsNullOrEmpty(testJoinColor) ? "white" : testJoinColor.ToLowerInvariant();
        if (joinColor != "white" && joinColor != "black")
        {
            joinColor = "white";
        }
        Debug.Log($"[Online] Forcing join via button. Joining game {testGameId} as {joinColor}.");
        JoinOnlineGame(testGameId, joinColor);
    }
}
