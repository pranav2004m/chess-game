# Quick Reference Card

## 🚀 Server Commands

```bash
# Start server
cd ChessServer && python app.py

# Test server
bash ChessServer/test_server.sh

# Check if running
curl http://localhost:5000/health
```

## 🎮 Unity Setup (In Inspector)

```
GameObject: _ChessNetworkManager
├── ChessNetworkClient
│   └── Server URL: http://localhost:5000
└── NetworkMultiplayerManager
    ├── Network Client: (drag ChessNetworkClient)
    └── Board Manager: (drag your BoardManager)
```

## 📝 Game Flow Codes

| Status | Meaning |
|--------|---------|
| `"waiting"` | Waiting for 2nd player |
| `"active"` | Game is playing |
| `"finished"` | Game ended |

| Turn | Meaning |
|------|---------|
| `"white"` | White's turn |
| `"black"` | Black's turn |

| Reason | Meaning |
|--------|---------|
| `"checkmate"` | Checkmate |
| `"resignation"` | Player resigned |
| `"draw"` | Agreed draw |
| `"timeout"` | Time ran out |

## 🔄 Main API Calls

```csharp
// Create game (Host)
networkManager.CreateOnlineGame("white");

// Join game (Client)
networkManager.JoinOnlineGame("abc123", "black");

// Send move
networkManager.SendMove("e2", "e4");

// Get state
networkManager.GetGameState((state) => {...});

// End game
networkManager.EndGame("white", "checkmate");

// Disconnect
networkManager.DisconnectFromGame();
```

## 📡 cURL Commands (Testing)

```bash
# Create
curl -X POST http://localhost:5000/game/create

# Join White
curl -X POST http://localhost:5000/game/ABC/join \
  -H "Content-Type: application/json" \
  -d '{"player_id":"p1","color":"white"}'

# Join Black
curl -X POST http://localhost:5000/game/ABC/join \
  -H "Content-Type: application/json" \
  -d '{"player_id":"p2","color":"black"}'

# Move
curl -X POST http://localhost:5000/game/ABC/move \
  -H "Content-Type: application/json" \
  -d '{"player_id":"p1","from":"e2","to":"e4"}'

# State
curl http://localhost:5000/game/ABC/state

# End
curl -X POST http://localhost:5000/game/ABC/end \
  -H "Content-Type: application/json" \
  -d '{"winner":"white","reason":"checkmate"}'

# List
curl http://localhost:5000/games
```

## 🔌 Event Subscription

```csharp
// Listen for game state updates
networkClient.OnGameStateChanged += (state) => {
    Debug.Log($"Turn: {state.current_turn}");
    Debug.Log($"Moves: {state.move_count}");
    UpdateBoard(state);
};

// Listen for errors
networkClient.OnError += (error) => {
    Debug.LogError($"Error: {error}");
};
```

## ♟️ Move Format

```
From: "a1" to "h8"    // Standard algebraic notation
Promotion: "Q", "R", "B", "N" (optional)

Examples:
"e2" → "e4"           // Pawn push
"g1" → "f3"           // Knight
"e7" → "e1", promotion: "Q"  // Pawn promotion
```

## 🎯 Polling Info

- **Interval**: 500ms
- **Enabled**: After joining game
- **Disabled**: On disconnect or game delete
- **Frequency**: Auto-called, no manual trigger needed

## 🌐 Network URLs

| Setup | URL |
|-------|-----|
| Local machine | `http://localhost:5000` |
| Same WiFi | `http://192.168.1.X:5000` |
| Internet | `http://your-ip:5000` |

*Find IP: `ifconfig` (Mac/Linux) or `ipconfig` (Windows)*

## 📊 Response Structure

```json
{
  "success": true/false,
  "game_state": {
    "game_id": "abc123",
    "current_turn": "white",
    "game_status": "active",
    "move_count": 5,
    "moves": [...]
  },
  "error": "message if failed"
}
```

## 🐛 Common Issues

| Problem | Fix |
|---------|-----|
| Port in use | Kill: `lsof -i :5000` |
| Connection failed | Check server running |
| Game not found | Verify game ID |
| Not your turn | Wait for opponent |
| No updates | Check polling (500ms) |
| CORS error | Restart server |

## 📋 Integration Checklist

- [ ] Server started
- [ ] Scripts added to Assets/Scripts/
- [ ] GameObject created with components
- [ ] References wired in Inspector
- [ ] Menu button added
- [ ] Move sending implemented
- [ ] Move receiving implemented
- [ ] Turn display working
- [ ] Game end working
- [ ] Tested with 2 editors
- [ ] Tested over network

## 🎓 Documentation Map

```
START HERE ──→ NETWORK_SETUP.md (5 min read)
     ↓
   DEEP DIVE ──→ IMPLEMENTATION_SUMMARY.md
     ↓
   IMPLEMENT ──→ IMPLEMENTATION_CHECKLIST.md
     ↓
   API DOCS ──→ ChessServer/README.md
     ↓
   EXAMPLES ──→ ChessServer/API_RESPONSES.md
     ↓
   CODE ──→ INTEGRATION_EXAMPLE.cs
```

## 🔑 Key Variables

```csharp
string gameId             // Unique game identifier
string playerId          // Unique player identifier
string playerColor       // "white" or "black"
int pollInterval         // 500ms (hardcoded)
string serverUrl         // Default: http://localhost:5000
GameStateData gameState  // Current game state
```

## 🎯 Success Indicators

✅ Server console shows: "Running on http://localhost:5000"
✅ Unity console shows: "✅ Game created" or "✅ Joined game"
✅ Both editors show opposite colors
✅ After move: other editor sees it in 500-700ms
✅ "Your turn" / "Opponent's turn" displays correctly
✅ Can't move when not your turn
✅ Game end shows winner

## 🚀 Deployment Notes

- Server works on **any machine** with Python 3.7+
- Port **5000** is configurable in `app.py`
- **Firewall**: May need to open port 5000
- **CORS**: Already enabled for all origins
- **Memory**: ~10KB per active game
- **Persistence**: In-memory (clears on restart)

## 📞 Getting Help

1. Error in console? → Read error message carefully
2. Network issue? → Check server URL in Inspector
3. Logic issue? → Review INTEGRATION_EXAMPLE.cs
4. API issue? → Check API_RESPONSES.md for examples
5. Setup issue? → Follow IMPLEMENTATION_CHECKLIST.md

---

**Remember:** Polling adds ~500ms delay. This is normal for turn-based chess!
