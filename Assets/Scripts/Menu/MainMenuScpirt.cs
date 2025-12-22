using System;
using System.Collections;
using System.Collections.Generic;
using ChessModel;
using Player;
using UnityEngine;

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
    public GameObject aiDifficulty;
    public GameObject pauseMenu;
    
    public BoardManager boardManager;
    public NetworkMultiplayerManager networkManager;

    private GameMode _gameMode;
    private Dictionary<ChessColor, Player.Player> _players;
    private ChessColor _playerColor;

    // Testing helpers to control host/join behavior per editor instance
    [SerializeField] private bool joinExistingGameForTesting = false;
    [SerializeField] private string testGameId = ""; // Set this in Editor 2 to the host's Game ID
    [SerializeField] private string testJoinColor = "black"; // "white" or "black"
    
    void Start()
    {
        firstMenu.SetActive(true);
        modeSelection.SetActive(false);
        colorSelection.SetActive(false);
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
        colorSelection.SetActive(false);
        
        if (_gameMode == GameMode.OnlineMultiplayer)
        {
            // For online mode, create a game and join as selected color
            string colorString = _playerColor == ChessColor.White ? "white" : "black";
            CreateOnlineGame(colorString);
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
        // If configured to join an existing game directly (Editor 2 flow), do that and return
        if (joinExistingGameForTesting && !string.IsNullOrEmpty(testGameId))
        {
            string joinColor = string.IsNullOrEmpty(testJoinColor) ? "white" : testJoinColor.ToLowerInvariant();
            if (joinColor != "white" && joinColor != "black")
            {
                joinColor = "white";
            }
            Debug.Log($"[Online] Join override enabled. Joining game {testGameId} as {joinColor}.");
            JoinOnlineGame(testGameId, joinColor);
            return;
        }

        // Default: Host flow — select a color then create the online game
        _gameMode = GameMode.OnlineMultiplayer;
        Debug.Log("[Online] Host flow selected. Showing color selection for create.");
        colorSelection.SetActive(true);
        modeSelection.SetActive(false);

        // Default (host) flow: choose color then create game on server
        modeSelection.SetActive(false);
        colorSelection.SetActive(true);
        _gameMode = GameMode.OnlineMultiplayer;
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
    
    public void Exit()
    {
        Application.Quit();
    }

    // ========== ONLINE MULTIPLAYER METHODS ==========
    
    private void CreateOnlineGame(string color)
    {
        if (networkManager == null)
        {
            Debug.LogError("NetworkMultiplayerManager not assigned in Inspector!");
            return;
        }
        
        // Create game and display the game ID for opponent
        networkManager.CreateOnlineGame(color);
        
        Debug.Log("Game created! Share the Game ID with your opponent.");
        Debug.Log($"Game ID: {networkManager.GetGameId()}");
        
        // TODO: Show UI with game ID and "Waiting for opponent..." message
        StartGame();
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
