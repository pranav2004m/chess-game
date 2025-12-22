using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Multiplayer menu UI that provides options to Create or Join an online game
/// </summary>
public class MultiplayerMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject multiplayerMenuPanel;
    [SerializeField] private TMP_InputField gameIdInputField;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button backButton;
    
    [Header("Waiting Panel (Optional - No longer used)")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TMP_Text gameIdDisplayText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button copyGameIdButton;
    
    [Header("References")]
    [SerializeField] private MainMenuScpirt mainMenu;
    
    private string currentGameId;

    private void Start()
    {
        if (createGameButton != null)
            createGameButton.onClick.AddListener(OnCreateGameClicked);
        
        if (joinGameButton != null)
            joinGameButton.onClick.AddListener(OnJoinGameClicked);
        
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        if (copyGameIdButton != null)
            copyGameIdButton.onClick.AddListener(OnCopyGameIdClicked);
        
        HideAllPanels();
    }

    public void ShowMultiplayerMenu()
    {
        // Activate this GameObject (MultiplayerMenu) first
        gameObject.SetActive(true);
        
        // Then show the panel
        if (multiplayerMenuPanel != null)
            multiplayerMenuPanel.SetActive(true);
        
        // Clear input field
        if (gameIdInputField != null)
            gameIdInputField.text = "";
    }

    public void ShowWaitingPanel(string gameId)
    {
        currentGameId = gameId;
        
        if (waitingPanel != null)
            waitingPanel.SetActive(true);
        
        if (multiplayerMenuPanel != null)
            multiplayerMenuPanel.SetActive(false);
        
        if (gameIdDisplayText != null)
            gameIdDisplayText.text = $"Game ID: {gameId}";
        
        if (statusText != null)
            statusText.text = "Waiting for opponent to join...";
        
        Debug.Log($"📋 Game created! Share this Game ID: {gameId}");
    }

    public void HideWaitingPanel()
    {
        if (waitingPanel != null)
            waitingPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (multiplayerMenuPanel != null)
            multiplayerMenuPanel.SetActive(false);
        
        if (waitingPanel != null)
            waitingPanel.SetActive(false);
        
        // Hide the entire menu GameObject
        gameObject.SetActive(false);
    }

    private void OnCreateGameClicked()
    {
        Debug.Log("🎮 Create Game clicked - showing color selection");
        
        if (mainMenu != null)
        {
            // Tell main menu to show color selection for host
            mainMenu.ShowColorSelectionForMultiplayer(isHost: true);
        }
        
        HideAllPanels();
    }

    private void OnJoinGameClicked()
    {
        if (gameIdInputField == null || string.IsNullOrEmpty(gameIdInputField.text))
        {
            Debug.LogWarning("⚠️ Please enter a Game ID to join");
            // TODO: Show error message UI
            return;
        }

        string gameId = gameIdInputField.text.Trim();
        Debug.Log($"🔗 Join Game clicked with ID: {gameId}");
        
        if (mainMenu != null)
        {
            // Join directly without color selection (auto-picks opposite color)
            mainMenu.JoinGameDirectly(gameId);
        }
        
        HideAllPanels();
    }

    private void OnBackClicked()
    {
        Debug.Log("⬅️ Back to mode selection");
        
        if (mainMenu != null)
        {
            mainMenu.ReturnToModeSelection();
        }
        
        HideAllPanels();
    }

    private void OnCopyGameIdClicked()
    {
        if (!string.IsNullOrEmpty(currentGameId))
        {
            GUIUtility.systemCopyBuffer = currentGameId;
            Debug.Log($"📋 Copied Game ID to clipboard: {currentGameId}");
            
            // Optional: Show brief confirmation message
            if (statusText != null)
            {
                string originalText = statusText.text;
                statusText.text = "Game ID copied to clipboard!";
                Invoke(nameof(ResetStatusText), 2f);
            }
        }
    }

    private void ResetStatusText()
    {
        if (statusText != null)
            statusText.text = "Waiting for opponent to join...";
    }
}
