# API Response Examples

All responses are in JSON format.

## 1. Create Game

**Request:**
```
POST http://localhost:5000/game/create
Content-Type: application/json
Body: (empty or {})
```

**Response (201):**
```json
{
  "success": true,
  "game_id": "a1b2c3d4",
  "message": "Game created successfully"
}
```

---

## 2. Join Game

**Request:**
```
POST http://localhost:5000/game/a1b2c3d4/join
Content-Type: application/json
Body: {
  "player_id": "player_001",
  "color": "white"
}
```

**Response (200) - First Player:**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": null,
    "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    "current_turn": "white",
    "game_status": "waiting",
    "winner": null,
    "reason": null,
    "move_count": 0,
    "moves": []
  }
}
```

**Response (200) - Second Player Joins:**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": "player_002",
    "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    "current_turn": "white",
    "game_status": "active",
    "winner": null,
    "reason": null,
    "move_count": 0,
    "moves": []
  }
}
```

**Response (400) - Color Already Taken:**
```json
{
  "success": false,
  "error": "Color white already taken"
}
```

**Response (404) - Game Not Found:**
```json
{
  "success": false,
  "error": "Game not found"
}
```

---

## 3. Make Move

**Request:**
```
POST http://localhost:5000/game/a1b2c3d4/move
Content-Type: application/json
Body: {
  "player_id": "player_001",
  "from": "e2",
  "to": "e4",
  "promotion": null
}
```

**Response (200) - Success:**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": "player_002",
    "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    "current_turn": "black",
    "game_status": "active",
    "winner": null,
    "reason": null,
    "move_count": 1,
    "moves": [
      {
        "player_id": "player_001",
        "color": "white",
        "from": "e2",
        "to": "e4",
        "promotion": null,
        "move_number": 1
      }
    ]
  }
}
```

**Response (400) - Not Your Turn:**
```json
{
  "success": false,
  "error": "Invalid move or not your turn"
}
```

**Response (400) - Game Not Active:**
```json
{
  "success": false,
  "error": "Game is not active"
}
```

---

## 4. Get Game State

**Request:**
```
GET http://localhost:5000/game/a1b2c3d4/state
```

**Response (200) - Waiting for Player:**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": null,
    "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    "current_turn": "white",
    "game_status": "waiting",
    "winner": null,
    "reason": null,
    "move_count": 0,
    "moves": []
  }
}
```

**Response (200) - Mid Game:**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": "player_002",
    "board_state": "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
    "current_turn": "black",
    "game_status": "active",
    "winner": null,
    "reason": null,
    "move_count": 1,
    "moves": [
      {
        "player_id": "player_001",
        "color": "white",
        "from": "e2",
        "to": "e4",
        "promotion": null,
        "move_number": 1
      }
    ]
  }
}
```

**Response (404) - Game Not Found:**
```json
{
  "success": false,
  "error": "Game not found"
}
```

---

## 5. End Game

**Request:**
```
POST http://localhost:5000/game/a1b2c3d4/end
Content-Type: application/json
Body: {
  "winner": "white",
  "reason": "checkmate"
}
```

**Response (200):**
```json
{
  "success": true,
  "game_state": {
    "game_id": "a1b2c3d4",
    "white_player_id": "player_001",
    "black_player_id": "player_002",
    "board_state": "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
    "current_turn": "black",
    "game_status": "finished",
    "winner": "white",
    "reason": "checkmate",
    "move_count": 1,
    "moves": [...]
  }
}
```

**Response (400) - Missing Data:**
```json
{
  "success": false,
  "error": "Missing winner or reason"
}
```

---

## 6. Delete Game

**Request:**
```
DELETE http://localhost:5000/game/a1b2c3d4/delete
```

**Response (200) - Success:**
```json
{
  "success": true,
  "message": "Game deleted"
}
```

**Response (404) - Game Not Found:**
```json
{
  "success": false,
  "error": "Game not found"
}
```

---

## 7. List Active Games

**Request:**
```
GET http://localhost:5000/games
```

**Response (200):**
```json
{
  "success": true,
  "games": [
    {
      "game_id": "a1b2c3d4",
      "white_player_id": "player_001",
      "black_player_id": "player_002",
      "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
      "current_turn": "white",
      "game_status": "active",
      "winner": null,
      "reason": null,
      "move_count": 0,
      "moves": []
    },
    {
      "game_id": "x9y8z7w6",
      "white_player_id": "player_003",
      "black_player_id": null,
      "board_state": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
      "current_turn": "white",
      "game_status": "waiting",
      "winner": null,
      "reason": null,
      "move_count": 0,
      "moves": []
    }
  ],
  "count": 2
}
```

---

## 8. Health Check

**Request:**
```
GET http://localhost:5000/health
```

**Response (200):**
```json
{
  "status": "healthy"
}
```

---

## Error Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (invalid data) |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Move Notation

Moves use algebraic notation:
- **From**: `a1` to `h8` (file + rank)
- **To**: `a1` to `h8`
- **Promotion**: `Q`, `R`, `B`, `N` (optional, for pawn promotion)

**Example Moves:**
```
"from": "e2", "to": "e4"           // Pawn push
"from": "g1", "to": "f3"           // Knight move
"from": "e7", "to": "e1", "promotion": "Q"  // Pawn promotion
```

---

## Move History Structure

Each move in the `moves` array contains:
```json
{
  "player_id": "player_001",
  "color": "white",
  "from": "e2",
  "to": "e4",
  "promotion": null,
  "move_number": 1
}
```

Move numbers are incremented for each move (1, 2, 3, etc.)
