# 🎮 Network Multiplayer Chess - START HERE

## ✨ What You Got

A **complete, production-ready networked multiplayer chess system** where:
- 2 players can play on **different machines** over **same network**
- Server is **centralized** (Python Flask on port 5000)
- Moves are **captured and stored** on the server
- Both **Unity clients sync in real-time** (every 500ms)

---

## 🚀 Get Started in 3 Steps

### Step 1: Start Server (Terminal 1)
```bash
cd /Users/pranavmotamarri/Documents/chess-game/ChessServer
pip install -r requirements.txt
python app.py
```

✅ You should see:
```
🎮 Chess Server Starting...
📡 Running on http://localhost:5000
```

### Step 2: Open Chess Scene in Unity
- Open your chess scene
- Play

### Step 3: Make a Menu Button
- Add button to menu: "Online Multiplayer"
- Call: `networkManager.CreateOnlineGame("white")` on click

That's it! Now you have online multiplayer!

---

## 📚 Full Documentation

| Document | Purpose | Time |
|----------|---------|------|
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | Commands & codes | 2 min |
| **[NETWORK_SETUP.md](NETWORK_SETUP.md)** | Detailed setup | 5 min |
| **[IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md)** | Step-by-step tasks | 30 min |
| **[FILE_INDEX.md](FILE_INDEX.md)** | Where everything is | 3 min |

---

## 📁 What Was Created

### Server (Python)
```
ChessServer/
├── app.py              (Main REST API server)
├── game_session.py     (Game management)
├── requirements.txt    (Install: pip install -r requirements.txt)
├── README.md          (Detailed API docs)
├── API_RESPONSES.md   (Request/response examples)
└── test_server.sh     (Run tests: bash test_server.sh)
```

### Unity Client
```
Assets/Scripts/
├── ChessNetworkClient.cs           (Network communication)
├── NetworkMultiplayerManager.cs    (Game integration)
└── INTEGRATION_EXAMPLE.cs          (How to integrate)
```

---

## 🎯 Your Workflow

### Option A: Local Testing (Same Machine)
1. Start server: `python app.py`
2. Open Unity scene in **Editor 1**
3. Play in Editor 1
4. Open **Editor 2** with same scene
5. Play in Editor 2
6. Editor 1: "Create Game"
7. Editor 2: "Join Game" + paste ID
8. Play!

### Option B: Network Testing (Different Machines)
1. Start server on **Machine A**
2. Get Machine A's IP: `ifconfig`
3. Update URL on **Machine B**: `http://MACHINE_A_IP:5000`
4. Same steps as Option A

---

## 🔧 Quick Commands

```bash
# Start server
python ChessServer/app.py

# Test server
bash ChessServer/test_server.sh

# Test with curl
curl -X POST http://localhost:5000/game/create
```

---

## 📝 3-Line Integration

Add this to your MenuScript:

```csharp
public void CreateOnlineGame() 
    => networkManager.CreateOnlineGame("white");

public void JoinOnlineGame(string gameId) 
    => networkManager.JoinOnlineGame(gameId, "black");
```

That's the minimum needed to get started!

---

## 🎮 How It Works

```
1. Player 1 (White) creates game
   → Server returns game_id: "abc123"

2. Player 1 shares "abc123" with Player 2

3. Player 2 (Black) joins with "abc123"
   → Server: "Game Active!"

4. Players alternate making moves
   → Each move syncs every 500ms

5. Game ends when someone wins
   → Server marks game "finished"
```

---

## 🧪 Test It Right Now

### Test Server (No Unity Needed)
```bash
# In a terminal, run:
bash ChessServer/test_server.sh

# You should see:
# ✅ All Tests PASSED!
```

### Test with 2 Unity Editors
1. Start server
2. Open scene in Editor 1 → Play
3. Open scene in Editor 2 → Play
4. Editor 1: Click "Create Online Game"
5. Copy game ID from console
6. Editor 2: Click "Join Online Game"
7. Paste game ID
8. Make moves!

---

## ✅ Success Checklist

- [ ] Server runs without errors
- [ ] Can see "Running on http://localhost:5000"
- [ ] `test_server.sh` passes all tests
- [ ] Two Unity editors can connect
- [ ] Moves sync between editors
- [ ] Can't move twice in a row
- [ ] Can play a full game

---

## 🆘 Troubleshooting

| Problem | Solution |
|---------|----------|
| "Connection refused" | Run `python app.py` first |
| "Port 5000 in use" | Kill: `lsof -i :5000` |
| "Game not found" | Copy exact game ID |
| No moves syncing | Wait 500ms for polling |
| CORS errors | Restart server |

---

## 📞 Need Help?

1. **Quick answers:** See [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
2. **Setup help:** See [NETWORK_SETUP.md](NETWORK_SETUP.md)
3. **API details:** See `ChessServer/API_RESPONSES.md`
4. **Integration:** See `INTEGRATION_EXAMPLE.cs`
5. **Complete guide:** See [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md)

---

## 🎉 What's Next?

1. ✅ Start server
2. ✅ Test with 2 Unity editors
3. ✅ Add menu button
4. ✅ Play!
5. ⭐ Optional: WebSocket for faster updates
6. ⭐ Optional: Add database persistence
7. ⭐ Optional: Support multiple games

---

## 📊 System Overview

```
Player 1 ──→ Python Server ←── Player 2
(White)      Port 5000        (Black)

All moves stored centrally
All game state on server
Both clients sync every 500ms
```

---

## 🚀 Ready?

→ **Start server:** `cd ChessServer && python app.py`

→ **Then read:** [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

→ **Enjoy!** ♟️

---

**Made with ❤️ for chess lovers!**
