# Implementation Checklist

Use this checklist to track your setup progress.

## ✅ Server Setup

- [ ] Python 3.7+ installed
- [ ] Navigated to `ChessServer/` directory
- [ ] Ran `pip install -r requirements.txt`
- [ ] Server starts without errors: `python app.py`
- [ ] Can access `http://localhost:5000/health`
- [ ] Server shows: "📡 Running on http://localhost:5000"

## ✅ Unity Client Setup

### Scene Preparation
- [ ] Add empty GameObject named `_ChessNetworkManager`
- [ ] Attach `ChessNetworkClient` script to it
- [ ] Attach `NetworkMultiplayerManager` script to it
- [ ] In Inspector, set Server URL: `http://localhost:5000`
- [ ] Wire `networkClient` reference in NetworkMultiplayerManager
- [ ] Wire `boardManager` reference in NetworkMultiplayerManager

### Script Files Added
- [ ] `ChessNetworkClient.cs` in `Assets/Scripts/`
- [ ] `NetworkMultiplayerManager.cs` in `Assets/Scripts/`
- [ ] `INTEGRATION_EXAMPLE.cs` as reference (no need to add to scene)

## ✅ Menu Integration

### MainMenuScript Updates
- [ ] Add "Online Multiplayer" button to main menu
- [ ] Add UI for color selection (white/black)
- [ ] Add UI for game creation/joining
- [ ] Add input field for game ID
- [ ] Add reference to `NetworkMultiplayerManager`
- [ ] Call `CreateOnlineGame()` when creating
- [ ] Call `JoinOnlineGame()` when joining

### UI Components Needed
- [ ] Button: "Online Multiplayer"
- [ ] Button: "Create Game"
- [ ] Button: "Join Game"
- [ ] Input Field: For game ID
- [ ] Text Display: For game ID (after create)
- [ ] Toggle or Buttons: White/Black color selection

## ✅ BoardManager Integration

### Detect Online Mode
- [ ] Add `isOnlineMode` boolean to BoardManager
- [ ] Add reference to `NetworkMultiplayerManager`
- [ ] Set `isOnlineMode = true` when starting online game

### Move Submission
- [ ] After validating move locally, send to server:
  ```csharp
  if (isOnlineMode && networkManager != null)
  {
      networkManager.SendMove(from, to, promotion);
  }
  ```

### Receiving Moves
- [ ] Subscribe to `networkManager.OnGameStateChanged`
- [ ] Update board visual when opponent moves
- [ ] Check `current_turn` to disable input when not your turn
- [ ] Handle `game_status: "finished"` for end game

### Position Conversion
- [ ] Implement `ConvertPositionToChessNotation()` (0-63 → "a1"-"h8")
- [ ] Implement `ConvertChessNotationToPosition()` ("a1" → 0-63)

### Game End
- [ ] Call `EndOnlineGame()` when checkmate detected
- [ ] Call `EndOnlineGame()` when player resigns
- [ ] Call `DisconnectFromOnlineGame()` when leaving

## ✅ Testing - Local Machine

### Single Machine, Two Editors
- [ ] Start server: `python app.py`
- [ ] Open scene in Editor 1
- [ ] Play in Editor 1
- [ ] Open scene in Editor 2 (open as new editor window)
- [ ] Play in Editor 2
- [ ] Editor 1: Click "Online Multiplayer" → "Create"
- [ ] Copy game ID from console
- [ ] Editor 2: Click "Online Multiplayer" → "Join" → paste ID
- [ ] Both editors: Verify game shows "active"
- [ ] Editor 1 (White): Click a piece, move it
- [ ] Wait 500ms
- [ ] Editor 2 (Black): See move appears
- [ ] Editor 2 (Black): Make a move
- [ ] Wait 500ms
- [ ] Editor 1 (White): See move appears
- [ ] Repeat moves for several turns

## ✅ Testing - Network

### Different Computers
- [ ] Find server machine IP address: `ifconfig` (Mac/Linux) or `ipconfig` (Windows)
- [ ] Start server on accessible machine: `python app.py`
- [ ] Machine A: Update server URL to `http://SERVER_IP:5000`
- [ ] Machine B: Update server URL to `http://SERVER_IP:5000`
- [ ] Machine A: Play game, click "Online Multiplayer"
- [ ] Machine A: Create game, copy game ID
- [ ] Machine B: Join with copied game ID
- [ ] Play game between machines
- [ ] Verify moves sync between machines

## ✅ API Testing (With cURL)

### Manual Testing (Optional)
- [ ] `curl -X POST http://localhost:5000/game/create`
- [ ] Note the returned `game_id`
- [ ] `curl -X POST http://localhost:5000/game/{id}/join -H "Content-Type: application/json" -d '{"player_id":"p1","color":"white"}'`
- [ ] `curl -X POST http://localhost:5000/game/{id}/join -H "Content-Type: application/json" -d '{"player_id":"p2","color":"black"}'`
- [ ] `curl http://localhost:5000/game/{id}/state`
- [ ] `curl -X POST http://localhost:5000/game/{id}/move -H "Content-Type: application/json" -d '{"player_id":"p1","from":"e2","to":"e4"}'`
- [ ] `curl http://localhost:5000/game/{id}/state` (verify move recorded)

## ✅ UI/UX Polish

### Visual Feedback
- [ ] Show "Waiting for opponent..." while in "waiting" status
- [ ] Show "Your turn" / "Opponent's turn" based on current_turn
- [ ] Highlight whose turn it is
- [ ] Disable move input when not your turn
- [ ] Show move history (optional)

### Error Handling
- [ ] Display connection errors to player
- [ ] Display "Invalid move" messages
- [ ] Handle "Not your turn" gracefully
- [ ] Handle player disconnect
- [ ] Auto-reconnect on network hiccup (optional)

### Game End
- [ ] Show winner on screen
- [ ] Show end reason (checkmate, resignation, etc.)
- [ ] Show option to return to menu
- [ ] Show option to play again

## ✅ Performance Check

- [ ] Server responds in <100ms locally
- [ ] Polling every 500ms (no network spam)
- [ ] No memory leaks after 50+ moves
- [ ] No UI freezing when syncing

## ✅ Documentation Review

- [ ] Read `NETWORK_SETUP.md`
- [ ] Read `ChessServer/README.md`
- [ ] Read `ChessServer/API_RESPONSES.md`
- [ ] Understand game flow
- [ ] Understand REST endpoints

## ✅ Deployment Prep (Optional)

- [ ] Server can run on headless machine
- [ ] Server handles player disconnects gracefully
- [ ] Server recovers from crashes
- [ ] Server logs all activity (optional)
- [ ] Consider firewall/port forwarding for internet

## ✅ Future Enhancements (Optional)

- [ ] WebSocket support for real-time updates
- [ ] Database persistence (not just in-memory)
- [ ] Multiple simultaneous games
- [ ] Player accounts/authentication
- [ ] AI support in online mode
- [ ] Game replay/analysis
- [ ] Ranking system

---

## Estimated Timeline

| Task | Estimated Time |
|------|-----------------|
| Server setup | 5 minutes |
| Add scripts to Unity | 10 minutes |
| Menu integration | 20-30 minutes |
| BoardManager integration | 30-45 minutes |
| Testing (local) | 15 minutes |
| Testing (network) | 10 minutes |
| Bug fixes & polish | 30-60 minutes |
| **Total** | **2-3 hours** |

---

## Troubleshooting Quick Fixes

| Problem | Solution |
|---------|----------|
| "Connection refused" | Make sure `python app.py` is running |
| "Game not found" | Verify game ID is copied exactly |
| Moves not updating | Check polling interval, should be every 500ms |
| CORS error | Already enabled, but check server console for details |
| Turn validation fails | Check player_id is sent correctly |
| Port 5000 in use | Kill process: `lsof -i :5000 \| kill` |
| Unity can't reach server | Check server URL in inspector |
| Infinite loops in console | Check for recursive event subscriptions |

---

## Getting Help

If stuck:
1. Check console for error messages
2. Review `ChessServer/README.md` for API details
3. Check `NETWORK_SETUP.md` for quick start
4. Review `INTEGRATION_EXAMPLE.cs` for sample code
5. Test with cURL to isolate client/server issues

---

## Files Created

```
✅ ChessServer/
   ├── app.py
   ├── game_session.py
   ├── requirements.txt
   ├── README.md
   └── API_RESPONSES.md

✅ Assets/Scripts/
   ├── ChessNetworkClient.cs
   ├── NetworkMultiplayerManager.cs
   └── INTEGRATION_EXAMPLE.cs

✅ Root/
   ├── NETWORK_SETUP.md
   └── IMPLEMENTATION_SUMMARY.md
```

---

## Success Criteria

Your implementation is complete when:

- ✅ Server runs without errors
- ✅ Two Unity editors can create/join a game
- ✅ Moves sync between clients
- ✅ Turn validation works (can't move twice)
- ✅ Game end detection works
- ✅ Two separate machines can play against each other
- ✅ UI shows whose turn it is
- ✅ No console errors during gameplay

**Congratulations! You now have online multiplayer chess! 🎉**
