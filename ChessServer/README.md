# Chess Multiplayer Network Setup

## Architecture Overview

```
Unity Client 1 (White) ────┐
                             ├─ REST API ─── Python Flask Server
Unity Client 2 (Black) ─────┘
```

**Server**: Python Flask (in-memory, single game)
**Communication**: REST API (HTTP requests)
**Data Flow**: Client → Server → All Clients (via polling)

---

## Server Setup (Python)

### Prerequisites
- Python 3.7+
- pip

### Installation

1. Navigate to server directory:
```bash
cd ChessServer
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Run the server:
```bash
python app.py
```

Server will start on: `http://localhost:5000`

### Server Architecture

**Files:**
- `app.py` - Main Flask application with REST endpoints
- `game_session.py` - Game session management (in-memory)
- `requirements.txt` - Python dependencies

**Key Classes:**
- `GameSession` - Single game state
- `GameSessionManager` - Manages active games

---

## REST API Endpoints

### 1. Create Game
```
POST /game/create
Response: { success, game_id, message }
```
Creates a new game and returns a unique game ID.

**Example:**
```
curl -X POST http://localhost:5000/game/create
```

### 2. Join Game
```
POST /game/{game_id}/join
Body: { player_id, color }
Response: { success, game_state }
```
Players join a game with their color (white or black).

**Example:**
```bash
curl -X POST http://localhost:5000/game/abc123/join \
  -H "Content-Type: application/json" \
  -d '{"player_id": "player1", "color": "white"}'
```

### 3. Make Move
```
POST /game/{game_id}/move
Body: { player_id, from, to, promotion (optional) }
Response: { success, game_state, error }
```
Submit a move in standard chess notation (e.g., "e2" to "e4").

**Example:**
```bash
curl -X POST http://localhost:5000/game/abc123/move \
  -H "Content-Type: application/json" \
  -d '{"player_id": "player1", "from": "e2", "to": "e4"}'
```

### 4. Get Game State
```
GET /game/{game_id}/state
Response: { success, game_state }
```
Retrieve current game state including all moves and status.

### 5. End Game
```
POST /game/{game_id}/end
Body: { winner, reason }
Response: { success, game_state }
```
Finish a game with winner and reason (checkmate, resignation, timeout, draw).

### 6. Delete Game
```
DELETE /game/{game_id}/delete
Response: { success, message }
```
Clean up a completed game session.

### 7. List Active Games
```
GET /games
Response: { success, games, count }
```
List all active games on the server.

### 8. Health Check
```
GET /health
Response: { status }
```

---

## Unity Client Setup

### 1. Add Network Client to Scene

1. Create an empty GameObject: `_ChessNetworkManager`
2. Add `ChessNetworkClient` script component
3. Set Server URL to your server address (default: `http://localhost:5000`)

### 2. Script Integration

The `ChessNetworkClient` provides these public methods:

```csharp
// Create a new game
CreateGame(onGameCreated: (gameId) => { ... });

// Join existing game
JoinGame(gameId: "abc123", color: "white");

// Make a move
MakeMove(from: "e2", to: "e4", promotion: null, 
         onMoveResult: (success) => { ... });

// Get current game state
GetGameState(onStateReceived: (state) => { ... });

// End the game
EndGame(winner: "white", reason: "checkmate");

// Delete game from server
DeleteGame();
```

### 3. Events

Subscribe to network events:

```csharp
ChessNetworkClient client = GetComponent<ChessNetworkClient>();

// Listen for game state changes
client.OnGameStateChanged += (gameState) => {
    Debug.Log($"New turn: {gameState.current_turn}");
    Debug.Log($"Moves: {gameState.move_count}");
    UpdateBoardUI(gameState);
};

// Listen for errors
client.OnError += (error) => {
    Debug.LogError($"Network error: {error}");
};
```

---

## Game Flow Example

### Setup Phase
1. **Server starts** - Listening on port 5000
2. **Client 1 creates game** - `POST /game/create` → receives `game_id: "abc123"`
3. **Client 1 joins as white** - `POST /game/abc123/join` with `{player_id: "p1", color: "white"}`
4. **Client 2 joins as black** - `POST /game/abc123/join` with `{player_id: "p2", color: "black"}`
5. **Game status** → "active"

### Play Phase
1. **Client 1 moves** - `POST /game/abc123/move` with `{player_id: "p1", from: "e2", to: "e4"}`
2. **Server validates** - Turn check, move format verification
3. **Server broadcasts** - Game state updated in memory
4. **Client 2 polls** - `GET /game/abc123/state` → sees new move
5. **Client 2 responds** - Repeat

### End Phase
1. **Checkmate/Draw detected** - One client calls `EndGame()`
2. **Server marks game finished** - Sets winner and reason
3. **Both clients see result** - Via polling or direct call
4. **Cleanup** - Optional `DELETE /game/abc123/delete`

---

## Data Structures

### GameState
```json
{
  "game_id": "abc123",
  "white_player_id": "player1",
  "black_player_id": "player2",
  "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
  "current_turn": "white",
  "game_status": "active",
  "winner": null,
  "reason": null,
  "move_count": 0
}
```

### Move Format
```json
{
  "player_id": "player1",
  "from": "e2",
  "to": "e4",
  "promotion": null
}
```

---

## Configuration

### Server Configuration
Edit `app.py`:
```python
app.run(debug=True, host='0.0.0.0', port=5000)
```

### Client Configuration
In Unity Inspector:
- **Server URL**: http://your-server-ip:5000
- **Polling Interval**: 500ms (hardcoded, can be modified)

---

## Network Considerations

### Polling vs WebSockets
Current implementation uses **polling** (GET requests every 500ms):
- ✅ Simple to implement
- ✅ Works with standard HTTP
- ❌ Higher latency
- ❌ More server load

To optimize: Consider WebSocket upgrade later.

### Error Handling
- Network timeout: 60s (Unity default)
- Invalid moves: Rejected by server with error message
- Disconnection: Clients can rejoin with same game_id

### Latency Assumptions
- LAN: ~10-50ms
- WiFi same network: ~20-100ms
- Internet: ~100-500ms
- Polling adds ~500ms delay per action

---

## Troubleshooting

### Server won't start
```bash
# Check if port 5000 is in use
lsof -i :5000

# Kill process if needed
kill -9 <PID>
```

### Connection refused
- Verify server is running
- Check firewall settings
- Ensure client URL matches server address

### Moves not syncing
- Check polling interval (currently 500ms)
- Verify game_id is correct
- Check server logs for move validation errors

### CORS errors
- CORS is enabled in Flask (Flask-CORS)
- If issues persist, check server console

---

## Next Steps (Optional Enhancements)

1. **WebSocket Support** - Real-time updates instead of polling
2. **Move Validation** - Integrate with ChessBoard.cs validation
3. **Database** - Persist games beyond server restart
4. **Authentication** - Player login system
5. **Multiple Games** - Support more than one game simultaneously
6. **AI Integration** - AI player support in networked mode
