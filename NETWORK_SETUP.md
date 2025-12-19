# Quick Start Guide - Network Chess Multiplayer

## 🚀 Server Setup (5 minutes)

### 1. Install Python Dependencies
```bash
cd ChessServer
pip install -r requirements.txt
```

### 2. Start the Server
```bash
python app.py
```

You should see:
```
🎮 Chess Server Starting...
📡 Running on http://localhost:5000
```

---

## 🎮 Unity Client Setup

### 1. Add Network Components to Scene

In your existing chess scene:

1. Create empty GameObject: `_ChessNetworkManager`
2. Add two scripts:
   - `ChessNetworkClient` (for network communication)
   - `NetworkMultiplayerManager` (for game logic integration)

### 2. Wire Up References

In Inspector:
- `ChessNetworkClient.serverUrl` = `http://localhost:5000`
- `NetworkMultiplayerManager.networkClient` = drag the ChessNetworkClient
- `NetworkMultiplayerManager.boardManager` = drag your BoardManager

### 3. Update MainMenuScript

Add new menu option "Online Multiplayer" with buttons:
- "Create Game" (you host)
- "Join Game" (paste game ID)

Example code:
```csharp
public void StartOnlineMultiplayer()
{
    colorSelection.SetActive(true); // Select white/black
}

public void CreateOnlineGame()
{
    string selectedColor = GetSelectedColor(); // "white" or "black"
    networkMultiplayerManager.CreateOnlineGame(selectedColor);
}

public void JoinOnlineGame()
{
    // Show input field for game ID
    string gameId = GetInputGameId();
    string selectedColor = GetSelectedColor();
    networkMultiplayerManager.JoinOnlineGame(gameId, selectedColor);
}
```

### 4. Handle Moves in BoardManager

When a move is made locally:
```csharp
// After validating and playing move locally
if (isOnlineMode)
{
    networkMultiplayerManager.SendMove(from, to, promotion);
}
```

When move comes from server (via polling):
```csharp
// In your move handler
private void UpdateBoardFromNetwork(GameStateData gameState)
{
    // Parse moves from gameState.moves array
    // Update board visual representation
    // Highlight last move
}
```

---

## 🔄 Test Workflow

### Single Machine (Two Unity Editors)

1. **Unity Editor 1:**
   - File → New Scene or keep your current
   - Play
   - Click "Online Multiplayer" → "Create Game"
   - Share the Game ID (shown in console)

2. **Unity Editor 2:**
   - Open same scene in another editor window
   - Play
   - Click "Online Multiplayer" → "Join Game"
   - Paste Game ID from Editor 1
   - Select opposite color

3. **Make Moves:**
   - Editor 1 (White): Click a piece and move
   - Sees update on Editor 2 (Black) after ~500ms
   - Editor 2 (Black): Makes move
   - Sees update on Editor 1 after ~500ms

### Network (Different Computers)

1. Start server on accessible machine: `python app.py`
2. In both Unity clients, change server URL to: `http://SERVER_IP:5000`
3. Both players can then join from their machines

---

## 📊 Data Flow

```
Player 1 (White)                 Server                Player 2 (Black)
      │                            │                          │
      ├─ Create Game ─────────────→│                          │
      │                            │                          │
      │← Game ID (abc123) ─────────┤                          │
      │                            │                          │
      ├─ Join as White ───────────→│                          │
      │                            │                          │
      │                      [Waiting for 2nd player]        │
      │                            │                          │
      │                            │←─ Join as Black ────────┤
      │                            │                          │
      │←─ Game Active! ────────────┤─ Game Active! ───────→  │
      │                            │                          │
      ├─ Move e2→e4 ──────────────→│                          │
      │                            ├─ Store & Update ─────→  │
      │                            │                          │
      │                      [Poll every 500ms]              │
      │←─ Game State (incl. move) ←─┤                        │
      │                            │                          │
      │                            │←─ Move e7→e5 ─────────┤
      │                            │                         │
      │                      [Auto Poll]                      │
      │←─ Game State (incl. move) ←─┤                        │
      │                            │                          │
      └─────── Repeat ─────────────┴──────────────────────────┘
```

---

## 🧪 Testing Commands

### Test Server Directly

Create game:
```bash
curl -X POST http://localhost:5000/game/create
```

Example response:
```json
{
  "success": true,
  "game_id": "a1b2c3d4",
  "message": "Game created successfully"
}
```

Join as white:
```bash
curl -X POST http://localhost:5000/game/a1b2c3d4/join \
  -H "Content-Type: application/json" \
  -d '{"player_id": "player1", "color": "white"}'
```

Make move:
```bash
curl -X POST http://localhost:5000/game/a1b2c3d4/move \
  -H "Content-Type: application/json" \
  -d '{"player_id": "player1", "from": "e2", "to": "e4"}'
```

Get state:
```bash
curl http://localhost:5000/game/a1b2c3d4/state
```

---

## ⚠️ Common Issues

### "Connection refused"
- ❌ Server not running
- ✅ Start server: `python app.py`

### "Game not found"
- ❌ Wrong game ID
- ✅ Copy exact game ID from creator

### Moves not updating on other client
- ❌ Not polling (stopped listening)
- ✅ Ensure polling started after joining

### "Not your turn" error
- ❌ Both players trying to move simultaneously
- ✅ Wait for opponent's move to process (500ms polling)

---

## 📝 Integration Checklist

- [ ] Server running on port 5000
- [ ] ChessNetworkClient component in scene
- [ ] NetworkMultiplayerManager component in scene
- [ ] Server URL configured correctly
- [ ] Menu option for "Online Multiplayer" added
- [ ] Move input sends to `networkMultiplayerManager.SendMove()`
- [ ] Board updates when `OnGameStateChanged` event fires
- [ ] Move validation works (turn checking)
- [ ] Game end logic implemented
- [ ] Test with 2 Unity editors

---

## 🔗 File Structure

```
ChessGame/
├── ChessServer/
│   ├── app.py              ← Main server
│   ├── game_session.py     ← Game logic
│   ├── requirements.txt    ← Dependencies
│   └── README.md          ← Detailed docs
│
└── Assets/Scripts/
    ├── ChessNetworkClient.cs         ← Network communication
    ├── NetworkMultiplayerManager.cs  ← Game integration
    └── Menu/MainMenuScript.cs        ← Update with online option
```

---

## 🎯 Next Steps

1. ✅ Start server
2. ✅ Add components to Unity scene
3. ✅ Update MainMenuScript with online option
4. ✅ Wire up move sending and receiving
5. ✅ Test with 2 editors
6. ✅ Test over network (change server URL)
