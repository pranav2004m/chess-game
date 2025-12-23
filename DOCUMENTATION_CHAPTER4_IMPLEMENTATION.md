# 3D CHESS GAME - PROJECT DOCUMENTATION

## CHAPTER 4: IMPLEMENTATION

### 4.1 Implementation Requirements

#### 4.1.1 Development Environment Setup

**Unity Game Engine Configuration**
The project is implemented using Unity 2021.3.8f1 LTS (Long Term Support), which provides stable production-ready features essential for game development. This version was specifically chosen for its mature rendering pipeline, robust physics system, and extensive cross-platform support. Unity 2021.3 LTS ensures long-term stability with bug fixes and security updates, making it ideal for projects requiring reliability.

**Key Unity Features Utilized:**
- **Universal Render Pipeline (URP)**: For optimized 3D rendering with efficient lighting and shadows
- **Cinemachine**: Camera management system for smooth transitions between player perspectives
- **Animation System**: Mecanim animator for piece movements and visual effects
- **Physics Engine**: For collision detection and piece interaction validation
- **TextMesh Pro**: Advanced text rendering for UI elements with enhanced typography
- **Input System**: Legacy input manager for mouse and keyboard interactions
- **Object Pooling**: Custom implementation for efficient particle effect management

**Python Backend Environment**
The multiplayer server is built using Python 3.7+ with Flask framework, providing a lightweight yet powerful REST API infrastructure. The backend runs independently of the Unity client, enabling deployment on any Python-compatible server environment.

**Backend Stack:**
- **Flask 2.0+**: Micro web framework for handling HTTP requests and routing
- **Flask-CORS**: Cross-Origin Resource Sharing middleware enabling Unity client communication
- **UUID Module**: Generating unique game session identifiers
- **Typing Module**: Type hints for code clarity and maintainability

**Development Tools:**
- **IDE**: JetBrains Rider 2021 for C# development with Unity integration
- **Code Editor**: Visual Studio Code for Python backend development
- **Version Control**: Git for source code management (optional)
- **API Testing**: Postman for REST endpoint verification and debugging
- **Build Pipeline**: Unity Build Settings for Windows x64 executable generation

#### 4.1.2 External Assets and Dependencies

**Exploder Asset Package**
The Exploder system is integrated for dynamic object fragmentation and explosion effects during piece captures. This asset provides realistic destruction physics and particle effects that enhance visual feedback.

**Features Utilized:**
- Fragment generation from 3D chess piece meshes
- Explosion force parameters for dramatic capture effects
- Fragment pooling system for performance optimization
- Customizable explosion radius and intensity
- Particle system integration for smoke and debris

**Implementation in Project:**
```csharp
// Located in: Assets/Scripts/ChessBoard/BoardManager.cs
using Exploder;
using Exploder.Utils;

// Explosion triggered on piece capture
FragmentPool.Instance.DeactivateFragments();
FragmentPool.Instance.DestroyFragments();
```

**IL3DN Asset Collection**
The IL3DN (Infinity Level 3D Nature) asset package provides additional 3D models, scripts, and environmental effects. While primarily designed for nature scenes, selected components are used for atmospheric effects and utility scripts.

**Components Used:**
- Fog and weather effects for visual ambiance
- Color management systems for consistent aesthetic
- Utility scripts for object manipulation
- Environmental lighting presets

**TextMesh Pro**
TextMesh Pro (TMP) is Unity's advanced text rendering solution, providing superior text quality compared to legacy UI text. Integrated by default in Unity 2021.3, it's used extensively throughout the UI.

**Implementation Areas:**
- Main menu text elements and buttons
- In-game HUD displaying turn information
- Game-over screens showing results
- Multiplayer lobby game ID display
- Pawn promotion selection interface

**3D Chess Set Assets (GPVFX)**
Custom 3D models for chess pieces and board are provided by the GPVFX Chess Set package, featuring a cartoonish art style that aligns with the project's visual design goals.

**Asset Structure:**
- Prefabs for all six piece types (Pawn, Knight, Bishop, Rook, Queen, King)
- Separate models for white and black pieces
- Board tiles with distinct visual styling
- Materials and textures for consistent appearance

#### 4.1.3 Project Structure Organization

**Unity Project Hierarchy:**
```
Assets/
├── Scripts/                    # All C# game logic
│   ├── ChessModel/            # Core chess engine (model layer)
│   │   ├── ChessBoard.cs      # Board state and rule engine
│   │   ├── Piece.cs           # Piece logic and move generation
│   │   ├── Move.cs            # Move representation
│   │   └── MoveInfo.cs        # Move history data structure
│   ├── ChessBoard/            # Board management (controller layer)
│   │   ├── BoardManager.cs    # Main game controller
│   │   ├── MaterialManager.cs # Material/color management
│   │   ├── ObjectPool.cs      # Pooling system
│   │   └── Pieces/            # Piece visual components
│   ├── Player/                # AI and player abstractions
│   │   ├── Player.cs          # Abstract player base class
│   │   ├── MinimaxPlayer.cs   # AI implementation
│   │   └── RandomPlayer.cs    # Random move AI
│   ├── Menu/                  # UI controllers
│   │   ├── MainMenuScript.cs  # Menu navigation
│   │   ├── MultiplayerMenuUI.cs # Network lobby
│   │   ├── PromotionUIScript.cs # Pawn promotion
│   │   └── EndGameUI.cs       # Game results
│   ├── NetworkMultiplayerManager.cs # Network coordinator
│   ├── ChessNetworkClient.cs  # HTTP client for server
│   └── CameraController.cs    # Camera management
├── Scenes/                    # Unity scene files
├── Prefab/                    # Reusable game objects
├── GPVFX_Prefabs/            # Chess piece prefabs
└── TextMesh Pro/             # TMP assets and fonts
```

**Python Server Structure:**
```
ChessServer/
├── app.py                    # Flask application and endpoints
├── game_session.py           # Game state management
├── requirements.txt          # Python dependencies
├── API_RESPONSES.md          # API documentation
└── README.md                 # Setup instructions
```

---

### 4.2 Implementation Details of Functionalities

#### 4.2.1 Chess Rule Engine Implementation

**Core Board Representation**
The chess board is represented as a one-dimensional array of 64 elements (8x8 grid), where each element contains a `Piece` object. This array-based approach provides O(1) access time for any board position.

```csharp
// ChessBoard.cs - Board initialization
public Piece[] Board { get; }
private void InitializeBoard()
{
    Board[0] = new Piece(ChessColor.White, 0, ChessType.Rook);
    Board[1] = new Piece(ChessColor.White, 1, ChessType.Knight);
    // ... positions 2-63
    Board[63] = new Piece(ChessColor.Black, 63, ChessType.Rook);
}
```

**Position Indexing System:**
- Positions 0-7: White's back rank (Rook, Knight, Bishop, Queen, King, Bishop, Knight, Rook)
- Positions 8-15: White pawns
- Positions 16-47: Empty squares
- Positions 48-55: Black pawns
- Positions 56-63: Black's back rank

**Move Generation Algorithm**
Each piece type implements its own pseudo-legal move generation logic in the `Piece.cs` class. The `GetPseudoMoves()` method generates all possible moves without considering check restrictions, while `GetLegalMoves()` filters out moves that would leave the king in check.

```csharp
public List<Move> GetLegalMoves()
{
    var list = GetPseudoMoves();
    list = list.Where(move =>
    {
        Board.Play(move, true);  // Simulate move
        var isCheck = Board.IsCheck(Color);
        Board.Unplay();          // Undo simulation
        return !isCheck;         // Keep only safe moves
    }).ToList();
    return list;
}
```

**Special Move Implementations:**

**Castling:**
Castling is handled in the `UpdateCastle()` method, tracking castling rights through boolean flags (`WhiteLeftCastle`, `WhiteRightCastle`, `BlackLeftCastle`, `BlackRightCastle`). Rights are revoked when the king or rooks move, or when castling is performed.

```csharp
// White kingside castling (King from e1 to g1)
if (WhiteRightCastle && move.EndPosition == 6)
{
    Switch(5, 7);  // Move rook from h1 to f1
    WhiteHasCastle = true;
}
```

Castling validation ensures:
- King and rook have not moved
- Squares between king and rook are empty
- King is not in check
- King does not pass through or land on attacked square

**En Passant:**
En passant capture is implemented in pawn move generation by checking if an opponent's pawn just moved two squares forward and is adjacent to the capturing pawn.

```csharp
// White pawn en passant (from rank 5)
if (Row == 4 && Board.LastMove != null)
{
    var move = Board.LastMove;
    if (Board.GetPiece(Row * 8 + Column + 1).Type == ChessType.Pawn &&
        move.StartPosition / 8 == Row + 2 && 
        move.EndPosition / 8 == Row)
    {
        legalMoves.Add(new Move(Position, (Row + 1) * 8 + Column + 1, 
                               this, Board.GetPiece(Row * 8 + Column + 1)));
    }
}
```

**Pawn Promotion:**
When a pawn reaches the opposite end of the board (rank 8 for white, rank 1 for black), the `TestPromotion()` method triggers the promotion UI, pausing the game until the player selects a piece type.

```csharp
private void TestPromotion(Move move, bool simulation)
{
    if (move.Piece.Type == ChessType.Pawn)
    {
        if ((move.Piece.Color == ChessColor.Black && move.EndPosition / 8 == 0) ||
            (move.Piece.Color == ChessColor.White && move.EndPosition / 8 == 7))
        {
            if (!simulation) AskPromotion(move.Piece);
        }
    }
}
```

**Check, Checkmate, and Stalemate Detection:**

**Check Detection:**
```csharp
public bool IsCheck(ChessColor color)
{
    var kingPosition = Array.Find(Board, p => 
        p.Color == color && p.Type == ChessType.King).Position;
    
    return Array.Exists(Board, piece =>
        piece.Color == color.Reverse() && 
        piece.GetPseudoMoves().Exists(m => m.EndPosition == kingPosition));
}
```

**Checkmate:**
```csharp
public bool IsCheckMate => 
    IsCheck(NextToPlay) && 
    !Array.Exists(Board, p => p.Color == NextToPlay && p.GetLegalMoves().Any());
```

**Stalemate:**
```csharp
private bool IsPat => 
    !IsCheck(NextToPlay) && 
    !Array.Exists(Board, p => p.Color == NextToPlay && p.GetLegalMoves().Any());
```

**Draw by Insufficient Material:**
```csharp
private bool InsufficientMaterial
{
    get
    {
        var pieces = Board.Where(p => p.Type != ChessType.None && 
                                     p.Type != ChessType.King).ToList();
        return !pieces.Any() || // King vs King
               (pieces.Count == 1 && (pieces[0].Type == ChessType.Bishop || 
                                     pieces[0].Type == ChessType.Knight)); // King+Minor vs King
    }
}
```

#### 4.2.2 Artificial Intelligence Implementation

**Minimax Algorithm with Alpha-Beta Pruning**

The AI opponent is implemented in `MinimaxPlayer.cs` using the classic Minimax algorithm enhanced with Alpha-Beta pruning for improved performance. This algorithm explores the game tree to a specified depth, evaluating positions and selecting the best move.

**Algorithm Overview:**
1. Generate all legal moves for the current position
2. For each move, recursively evaluate resulting positions
3. Maximize score for white, minimize for black
4. Use alpha-beta bounds to prune unnecessary branches
5. Return the move with the best evaluation score

**Implementation:**
```csharp
private float Minimax(int depth, float alpha, float beta, ChessColor color)
{
    // Terminal conditions
    if (_board.IsCheckMate)
        return _board.NextToPlay == ChessColor.White ? -100 : 100;
    
    if (_board.IsDraw())
        return 0;
    
    if (depth == 0)
        return _board.GetEvaluationScore();

    var moves = _board.GetAllLegalMoves(color).OrderBy(item => _rand.Next());
    
    if (color == ChessColor.White)
    {
        float value = float.MinValue;
        foreach (var move in moves)
        {
            _board.Play(move, true);
            var newValue = Minimax(depth - 1, alpha, beta, color.Reverse());
            _board.Unplay();
            
            if (newValue > value)
            {
                value = newValue;
                if (depth == _depth) _bestMove = move;
            }
            
            alpha = Math.Max(alpha, newValue);
            if (alpha >= beta) break; // Beta cutoff
        }
        return value;
    }
    else // Black's turn (minimizing)
    {
        float value = float.MaxValue;
        foreach (var move in moves)
        {
            _board.Play(move, true);
            var newValue = Minimax(depth - 1, alpha, beta, color.Reverse());
            _board.Unplay();
            
            if (newValue < value)
            {
                value = newValue;
                if (depth == _depth) _bestMove = move;
            }
            
            beta = Math.Min(beta, newValue);
            if (alpha >= beta) break; // Alpha cutoff
        }
        return value;
    }
}
```

**Evaluation Function**

The position evaluation function assigns a numerical score to a chess position, where positive values favor white and negative values favor black.

**Evaluation Components:**

1. **Material Count** (Primary factor):
   - Pawn: 1 point
   - Knight: 3 points
   - Bishop: 3 points
   - Rook: 5 points
   - Queen: 9 points
   - King: Not counted (infinite value)

2. **Center Control** (Positional advantage):
   - +0.1 for white if controlling center squares (d4, e4)
   - -0.1 for black if controlling center squares (d5, e5)

3. **Castling Status** (King safety):
   - +0.9 if white has castled
   - -0.9 if black has castled
   - -0.9 if white lost castling rights without castling
   - +0.9 if black lost castling rights without castling

```csharp
public float GetEvaluationScore()
{
    float score = WhiteCount - BlackCount;
    
    // Center control bonus
    if (IsThreatening(35, ChessColor.White) || IsThreatening(36, ChessColor.White)) 
        score += 0.1f;
    if (IsThreatening(27, ChessColor.Black) || IsThreatening(28, ChessColor.Black)) 
        score -= 0.1f;
    
    // Castling evaluation
    if (!WhiteLeftCastle && !WhiteRightCastle && !WhiteHasCastle) 
        score -= 0.9f;
    if (!BlackLeftCastle && !BlackRightCastle && !BlackHasCastle) 
        score += 0.9f;
    if (WhiteHasCastle) score += 0.9f;
    if (BlackHasCastle) score -= 0.9f;
    
    return score;
}
```

**Difficulty Levels Implementation:**

```csharp
// MainMenuScript.cs - Difficulty configuration
public enum Difficulty
{
    Random = 0,    // Random legal moves
    Easy = 2,      // Minimax depth 2 (2-ply lookahead)
    Medium = 4,    // Minimax depth 4 (4-ply lookahead)
    Hard = 6       // Minimax depth 6 (6-ply lookahead)
}

// AI instantiation
Player.Player player = difficulty == Difficulty.Random 
    ? new RandomPlayer(aiColor) 
    : new MinmaxPlayer(aiColor, (int)difficulty);
```

**Move Ordering Optimization:**
Moves are randomized using `OrderBy(item => _rand.Next())` to introduce variability in AI play when multiple moves have equal evaluation scores, preventing predictable patterns.

**Fallback Mechanism:**
A try-catch block ensures the AI always returns a valid move, even if the Minimax algorithm encounters an error:

```csharp
try 
{
    Minimax(_depth, float.MinValue, float.MaxValue, Color);
    if (_bestMove == null) // Fallback to random
    {
        var moves = _board.GetAllLegalMoves(Color);
        _bestMove = moves[_rand.Next(moves.Count)];
    }
}
catch (Exception)
{
    // Emergency fallback to random move
    var moves = _board.GetAllLegalMoves(Color);
    _bestMove = moves[_rand.Next(moves.Count)];
}
```

#### 4.2.3 Online Multiplayer Implementation

**Backend Server Architecture (Python Flask)**

The multiplayer server is implemented as a RESTful API using Flask, providing endpoints for game management and move synchronization. The server maintains in-memory game state using the `GameSession` and `GameSessionManager` classes.

**Server Startup Configuration:**
```python
# app.py
from flask import Flask, request, jsonify
from flask_cors import CORS
from game_session import GameSessionManager

app = Flask(__name__)
CORS(app)  # Enable cross-origin requests from Unity

session_manager = GameSessionManager()

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=8888)
```

**Game Session Management:**

```python
# game_session.py
class GameSession:
    def __init__(self, game_id: str):
        self.game_id = game_id
        self.white_player_id: Optional[str] = None
        self.black_player_id: Optional[str] = None
        self.board_state: str = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        self.moves: List[Dict] = []
        self.current_turn: str = "white"
        self.game_status: str = "waiting"  # waiting, active, finished
        self.winner: Optional[str] = None
```

**REST API Endpoints:**

**1. Create Game:**
```python
@app.route('/game/create', methods=['POST'])
def create_game():
    game_id = session_manager.create_game()  # Generates 8-character UUID
    return jsonify({
        "success": True,
        "game_id": game_id,
        "message": "Game created successfully"
    }), 201
```

**2. Join Game:**
```python
@app.route('/game/<game_id>/join', methods=['POST'])
def join_game(game_id):
    data = request.get_json()
    player_id = data.get('player_id')
    color = data.get('color')  # "white" or "black"
    
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    if not session.add_player(player_id, color):
        return jsonify({"success": False, "error": f"Color {color} already taken"}), 400
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200
```

**3. Make Move:**
```python
@app.route('/game/<game_id>/move', methods=['POST'])
def make_move(game_id):
    data = request.get_json()
    player_id = data.get('player_id')
    move = {"from": data.get('from'), "to": data.get('to'), 
            "promotion": data.get('promotion')}
    
    session = session_manager.get_session(game_id)
    
    # Validate turn
    if not session.add_move(player_id, move):
        return jsonify({"success": False, "error": "Invalid move or not your turn"}), 400
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200
```

**4. Query Game State:**
```python
@app.route('/game/<game_id>/state', methods=['GET'])
def get_game_state(game_id):
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200
```

**Unity Network Client Implementation**

The `ChessNetworkClient.cs` class handles all HTTP communication with the Flask server using Unity's `UnityWebRequest` API.

**HTTP Request Pattern:**
```csharp
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
            OnGameStateChanged?.Invoke(response.game_state);
            onMoveResult?.Invoke(true);
        }
        else
        {
            OnError?.Invoke($"Move failed: {request.error}");
            onMoveResult?.Invoke(false);
        }
    }
}
```

**Game State Synchronization:**

The `NetworkMultiplayerManager.cs` coordinates between the Unity chess board and the network client, implementing a polling mechanism to check for opponent moves.

```csharp
private IEnumerator PollGameState()
{
    while (true)
    {
        yield return new WaitForSeconds(pollInterval); // 1-2 seconds
        
        networkClient.QueryGameState(gameId, (state) => {
            if (state.move_count > lastAppliedMoveCount)
            {
                ApplyOpponentMove(state.moves[state.moves.Count - 1]);
                lastAppliedMoveCount = state.move_count;
            }
        });
    }
}
```

**Move Translation:**
Unity board positions (0-63 integer indices) must be translated to chess notation (e.g., "e2", "e4") for server communication:

```csharp
private string PositionToChessNotation(int position)
{
    int file = position % 8;  // 0-7 corresponds to a-h
    int rank = position / 8;  // 0-7 corresponds to 1-8
    char fileChar = (char)('a' + file);
    int rankNum = rank + 1;
    return $"{fileChar}{rankNum}";
}
```

**Network Error Handling:**
```csharp
private void HandleNetworkError(string errorMessage)
{
    Debug.LogError($"Network Error: {errorMessage}");
    
    // Display user-friendly error
    if (errorMessage.Contains("timeout"))
        ShowErrorUI("Connection timeout. Please check your internet connection.");
    else if (errorMessage.Contains("not found"))
        ShowErrorUI("Game not found. Please verify the game ID.");
    else
        ShowErrorUI("Network error occurred. Returning to main menu.");
}
```

#### 4.2.4 Visual System Implementation

**3D Piece Rendering and Animation**

Chess pieces are instantiated from prefabs located in `GPVFX_Prefabs/` folder, with separate models for white and black pieces (e.g., `White_Bishop.prefab`, `Black_Bishop.prefab`).

**Piece Instantiation:**
```csharp
private GameObject createPieceOnPlacement(ChessType pieceType, ChessColor color, int position)
{
    var pieceObject = _objectPool.getPooledPiece(pieceType, color, 
                        _tileManager.getCoordinatesByTilePlacement(position));
    
    if (color == ChessColor.Black)
        pieceObject.transform.Rotate(0, 180, 0); // Rotate black pieces
    
    return pieceObject;
}
```

**Object Pooling:**
The `ObjectPool.cs` class maintains pools of piece GameObjects to avoid instantiation/destruction overhead:

```csharp
public GameObject getPooledPiece(ChessType type, ChessColor color, Vector3 position)
{
    foreach (GameObject obj in pooledPieces)
    {
        if (!obj.activeInHierarchy && IsPieceType(obj, type, color))
        {
            obj.transform.position = position;
            obj.SetActive(true);
            return obj;
        }
    }
    
    // Create new if pool exhausted
    GameObject newPiece = InstantiateNewPiece(type, color, position);
    pooledPieces.Add(newPiece);
    return newPiece;
}
```

**Piece Movement Animation:**
The `PieceManager.cs` handles smooth piece animations using Unity's `Vector3.Lerp`:

```csharp
public void MovePiece(GameObject piece, Vector3 targetPosition, bool isRock = false)
{
    StartCoroutine(MovePieceCoroutine(piece, targetPosition));
}

private IEnumerator MovePieceCoroutine(GameObject piece, Vector3 target)
{
    Vector3 startPosition = piece.transform.position;
    float elapsedTime = 0f;
    float duration = 0.5f; // Animation duration
    
    while (elapsedTime < duration)
    {
        piece.transform.position = Vector3.Lerp(startPosition, target, 
                                               elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    piece.transform.position = target; // Ensure exact final position
}
```

**Capture Effects:**
When a piece is captured, the `AttackWithPiece` method triggers explosion effects:

```csharp
public void AttackWithPiece(GameObject attacker, Vector3 targetPos, 
                           Vector3 victimPos, GameObject victim)
{
    StartCoroutine(AttackSequence(attacker, targetPos, victim, victimPos));
}

private IEnumerator AttackSequence(GameObject attacker, Vector3 target, 
                                   GameObject victim, Vector3 victimPos)
{
    // Move attacker to target
    yield return MovePieceCoroutine(attacker, target);
    
    // Trigger explosion at victim position
    ExploderSingleton.Instance.ExplodeObject(victim, victimPos);
    
    // Play sound effect
    AudioSource.PlayClipAtPoint(captureSound, victimPos);
    
    // Deactivate victim
    victim.SetActive(false);
}
```

**Camera System:**
The camera automatically rotates between white and black perspectives in local multiplayer mode:

```csharp
public void NextTurn()
{
    var nextToPlay = _chessBoard.NextToPlay;
    
    // Rotate camera for local play
    if (!onlineMultiplayer)
    {
        whiteCam.SetActive(nextToPlay == ChessColor.White);
    }
    else // Pin to local player in online mode
    {
        whiteCam.SetActive(localHumanColor == ChessColor.White);
    }
}
```

**Legal Move Visualization:**
The `TileManager.cs` displays visual indicators for legal moves:

```csharp
public void updateLegalMoves(List<Move> legalMoves)
{
    // Clear previous indicators
    foreach (var tile in highlightedTiles)
    {
        tile.GetComponent<Renderer>().material.color = originalColor;
    }
    highlightedTiles.Clear();
    
    // Highlight legal move destinations
    foreach (var move in legalMoves)
    {
        GameObject tile = GetTileAtPosition(move.EndPosition);
        tile.GetComponent<Renderer>().material.color = highlightColor;
        highlightedTiles.Add(tile);
    }
}
```

#### 4.2.5 User Interface Implementation

**Main Menu System**

The `MainMenuScript.cs` manages navigation between different menus and game mode selection:

```csharp
public void SelectPvpGameMode()
{
    _gameMode = GameMode.Pvp;
    _players[ChessColor.White] = null;
    _players[ChessColor.Black] = null;
    StartGame();
}

public void SelectPvAiGameMode()
{
    _gameMode = GameMode.Pvai;
    colorSelection.SetActive(true); // Show color selection
}

public void SelectOnlineMultiplayerMode()
{
    _gameMode = GameMode.OnlineMultiplayer;
    multiplayerMenuUI.ShowMultiplayerMenu();
}
```

**Pawn Promotion Interface:**
When pawn promotion occurs, the `PromotionUIScript.cs` displays a selection panel:

```csharp
public void ShowPromotionUI(Piece pawn)
{
    piece = pawn;
    promotionPanel.SetActive(true);
    Time.timeScale = 0; // Pause game
}

public void promotionSelectQueen()
{
    promotionSelect(ChessType.Queen);
}

private void promotionSelect(ChessType chessType)
{
    _boardManager.GivePromotion(piece, chessType);
    promotionPanel.SetActive(false);
    Time.timeScale = 1; // Resume game
}
```

**Multiplayer Lobby:**
The `MultiplayerMenuUI.cs` provides UI for creating and joining games:

```csharp
private void OnCreateGameClicked()
{
    mainMenu.CreateOnlineGame(selectedColor);
}

private void OnJoinGameClicked()
{
    string gameId = gameIdInputField.text.Trim();
    if (string.IsNullOrEmpty(gameId))
    {
        ShowError("Please enter a game ID");
        return;
    }
    
    mainMenu.JoinOnlineGame(gameId, selectedColor);
}

public void ShowWaitingPanel(string gameId)
{
    gameIdDisplayText.text = $"Game ID: {gameId}";
    statusText.text = "Waiting for opponent to join...";
    waitingPanel.SetActive(true);
}
```

**End Game Screen:**
The `EndGameUI.cs` displays game results:

```csharp
public void ShowCheckmate(ChessColor winner)
{
    resultText.text = $"{winner} wins by checkmate!";
    endGamePanel.SetActive(true);
}

public void ShowStalemate()
{
    resultText.text = "Game drawn by stalemate";
    endGamePanel.SetActive(true);
}

public void ShowDraw(string reason)
{
    resultText.text = $"Game drawn: {reason}";
    endGamePanel.SetActive(true);
}
```

---

### 4.3 Challenges During Implementation

#### 4.3.1 Chess Rule Edge Cases

**Challenge: Castling Through Check**
Initial implementation allowed castling even when the king passed through an attacked square, violating chess rules.

**Solution:** Added validation to check if intermediate squares are threatened:
```csharp
// Prevent castling through check
if (Board.IsThreatening(5, Board.NextToPlay.Reverse()))
    list.RemoveAll(move => move.EndPosition == 6 && move.StartPosition == 4);
if (Board.IsThreatening(3, Board.NextToPlay.Reverse()))
    list.RemoveAll(move => move.EndPosition == 2 && move.StartPosition == 4);
```

**Challenge: En Passant Timing**
En passant must be executed immediately after the opponent's pawn double move, not on subsequent turns.

**Solution:** Implemented move history tracking with `LastMove` property:
```csharp
if (Row == 4 && Board.LastMove != null)
{
    var move = Board.LastMove;
    // Verify last move was a pawn double push adjacent to current pawn
    if (Board.GetPiece(Row * 8 + Column + 1).Type == ChessType.Pawn &&
        move.StartPosition / 8 == Row + 2 && 
        move.EndPosition / 8 == Row)
    {
        // En passant is legal
    }
}
```

**Challenge: Promotion in Simulation**
During AI move simulation, pawn promotions would trigger UI prompts, breaking the simulation logic.

**Solution:** Added `simulation` parameter to `Play()` method:
```csharp
private void TestPromotion(Move move, bool simulation)
{
    if (/* pawn reached end */)
    {
        if (!simulation) // Only show UI in real games
            AskPromotion(move.Piece);
        else
            move.Piece.Type = ChessType.Queen; // Auto-promote to queen in simulation
    }
}
```

#### 4.3.2 AI Performance Optimization

**Challenge: Depth 6 Search Timeout**
Initial implementation of Minimax at depth 6 took over 30 seconds per move, making gameplay unacceptable.

**Solution: Alpha-Beta Pruning Implementation**
Added alpha-beta cutoffs to prune unnecessary branches:
```csharp
alpha = Math.Max(alpha, newValue);
if (alpha >= beta) break; // Prune remaining moves
```
This reduced average move time from 30+ seconds to 2-3 seconds.

**Challenge: Repetitive AI Moves**
Without randomization, AI would always choose the first move with the best evaluation, leading to predictable play.

**Solution: Move Ordering Randomization**
```csharp
var moves = _board.GetAllLegalMoves(color).OrderBy(item => _rand.Next());
```
This introduces variability when multiple moves have equal evaluation scores.

**Challenge: AI Crashes on Unexpected States**
Rare game states caused AI to throw exceptions, crashing the game.

**Solution: Comprehensive Try-Catch with Fallback**
```csharp
try 
{
    Minimax(_depth, float.MinValue, float.MaxValue, Color);
    if (_bestMove == null) // Fallback #1
    {
        _bestMove = GetRandomLegalMove();
    }
}
catch (Exception ex)
{
    Debug.LogError($"AI Error: {ex.Message}");
    _bestMove = GetRandomLegalMove(); // Fallback #2
}
```

#### 4.3.3 Network Synchronization Issues

**Challenge: Move Desynchronization**
Moves occasionally applied out of order, causing board states to diverge between clients.

**Solution: Move Counter Tracking**
```csharp
private int lastAppliedMoveCount = 0;

private void HandleGameStateChanged(GameStateData gameState)
{
    if (gameState.move_count > lastAppliedMoveCount)
    {
        // Apply only new moves
        for (int i = lastAppliedMoveCount; i < gameState.moves.Count; i++)
        {
            ApplyMove(gameState.moves[i]);
        }
        lastAppliedMoveCount = gameState.move_count;
    }
}
```

**Challenge: Race Conditions on Move Submission**
Simultaneous move submissions from both clients caused server errors.

**Solution: Server-Side Turn Validation**
```python
def add_move(self, player_id: str, move: Dict) -> bool:
    expected_player = (self.white_player_id if self.current_turn == "white" 
                      else self.black_player_id)
    
    if player_id != expected_player:
        return False  # Not your turn
    
    # Process move and toggle turn
    self.moves.append(move)
    self.current_turn = "black" if self.current_turn == "white" else "white"
    return True
```

**Challenge: Network Timeout Handling**
Long network delays caused application hangs without user feedback.

**Solution: Timeout Configuration and Error Callbacks**
```csharp
request.timeout = 10; // 10 second timeout

if (request.result == UnityWebRequest.Result.ConnectionError || 
    request.result == UnityWebRequest.Result.ProtocolError)
{
    OnError?.Invoke($"Connection failed: {request.error}");
    ShowRetryDialog();
}
```

#### 4.3.4 Memory Management and Performance

**Challenge: Memory Leaks from Particle Effects**
Explosion fragments accumulated over time, causing memory usage to grow continuously.

**Solution: Fragment Pooling and Cleanup**
```csharp
public void RestartGame()
{
    FragmentPool.Instance.DeactivateFragments();
    FragmentPool.Instance.DestroyFragments();
    FragmentPool.Instance.Reset(ExploderSingleton.Instance.Params);
}
```

**Challenge: Frame Rate Drops During Explosions**
Multiple simultaneous explosions (e.g., capturing multiple pieces quickly) caused frame rate to drop below 30 FPS.

**Solution: Explosion Force Reduction and Particle Limits**
```csharp
ExploderSingleton.Instance.Params.MaxFragments = 20; // Limit fragments per explosion
ExploderSingleton.Instance.Params.Force = 5f; // Reduce explosion force
```

**Challenge: UI Memory Retention**
UI panels remained in memory after closing, accumulating resources.

**Solution: Proper UI Lifecycle Management**
```csharp
public void BackToMenu()
{
    // Deactivate all UI
    pauseMenu.SetActive(false);
    endGameUI.SetActive(false);
    
    // Clear references
    _players.Clear();
    _legalMoves.Clear();
    
    // Stop all coroutines
    StopAllCoroutines();
}
```

#### 4.3.5 Unity-Specific Challenges

**Challenge: Scene Persistence Between Game Modes**
Game state persisted when returning to main menu, causing incorrect initialization on new games.

**Solution: Complete State Reset**
```csharp
public void RestartGame()
{
    _chessBoard.InitializeBoard();
    
    // Reset all pieces
    foreach (var piece in _map.Values)
    {
        piece.SetActive(false);
    }
    _map.Clear();
    
    // Recreate pieces
    foreach (var piece in _chessBoard.Board)
    {
        if (piece.Type != ChessType.None)
        {
            GameObject gameObjectPiece = createPieceOnPlacement(piece.Type, 
                                                               piece.Color, 
                                                               piece.Position);
            _map.Add(piece, gameObjectPiece);
        }
    }
    
    // Reset game flags
    _humainPlayer = false;
    _firstClick = true;
    playing = true;
}
```

**Challenge: Coroutine Management**
Coroutines continued running after scene changes, causing null reference exceptions.

**Solution: Coroutine Reference Tracking**
```csharp
private Coroutine pollCoroutine;

public void StartPolling()
{
    if (pollCoroutine != null)
        StopCoroutine(pollCoroutine);
    
    pollCoroutine = StartCoroutine(PollGameState());
}

private void OnDisable()
{
    if (pollCoroutine != null)
    {
        StopCoroutine(pollCoroutine);
        pollCoroutine = null;
    }
}
```

**Challenge: Input System Conflicts**
Mouse clicks on UI elements also registered on board tiles underneath.

**Solution: UI Raycast Blocking**
```csharp
public void ClickTile(int placement)
{
    // Check if clicking on UI
    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        return; // Ignore click on UI
    
    if (!_humainPlayer || !playing) return;
    
    // Process tile click
}
```

#### 4.3.6 Cross-Platform Compatibility

**Challenge: Python Server Port Conflicts**
Default port 5000 conflicted with macOS AirPlay Receiver.

**Solution: Configurable Port**
```python
if __name__ == '__main__':
    port = int(os.environ.get('PORT', 8888))  # Default to 8888
    app.run(debug=True, host='0.0.0.0', port=port)
```

**Challenge: Path Separator Differences**
Hard-coded backslashes in paths caused issues on non-Windows systems.

**Solution: Use Unity's Platform-Agnostic APIs**
```csharp
string prefabPath = System.IO.Path.Combine("Assets", "GPVFX_Prefabs", 
                                          $"{color}_{type}.prefab");
```

**Challenge: CORS Errors in Web Builds**
Browser security policies blocked Unity WebGL builds from accessing the Flask server.

**Solution: Comprehensive CORS Configuration**
```python
from flask_cors import CORS

CORS(app, resources={
    r"/game/*": {
        "origins": "*",
        "methods": ["GET", "POST", "DELETE"],
        "allow_headers": ["Content-Type"]
    }
})
```

---

## Summary

The implementation of the 3D Chess Game involved coordinating multiple complex systems: a complete chess rule engine with special moves, an AI opponent using Minimax with Alpha-Beta pruning, a Python Flask backend for online multiplayer, and Unity 3D rendering with animations and visual effects. Key challenges included optimizing AI performance, handling chess rule edge cases, synchronizing network state, and managing memory efficiently. The modular architecture with clear separation between game logic (Model), Unity rendering (View), and game management (Controller) enabled systematic problem-solving and iterative refinement. External assets like Exploder, IL3DN, and TextMesh Pro were integrated to enhance visual quality while maintaining performance. The result is a stable, feature-complete chess application supporting three distinct gameplay modes with professional-quality user experience.

---

**Document Status:**
- Version: 1.0
- Date: December 23, 2025
- Next Section: Chapter 5 - Testing and Validation
