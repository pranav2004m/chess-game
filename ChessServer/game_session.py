"""
Game Session Manager - Handles in-memory chess game state
"""
import uuid
from typing import Dict, List, Optional


class GameSession:
    """Represents a single chess game session"""
    
    def __init__(self, game_id: str):
        self.game_id = game_id
        self.white_player_id: Optional[str] = None
        self.black_player_id: Optional[str] = None
        self.board_state: str = self._init_board_fen()  # FEN notation for board state
        self.moves: List[Dict] = []  # List of all moves made
        self.current_turn: str = "white"  # whose turn it is
        self.game_status: str = "waiting"  # waiting, active, finished
        self.winner: Optional[str] = None
        self.reason: Optional[str] = None
    
    def _init_board_fen(self) -> str:
        """Initialize board in FEN notation"""
        # Standard chess starting position FEN
        return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    
    def add_player(self, player_id: str, color: str) -> bool:
        """Add a player to the game"""
        if color == "white" and self.white_player_id is None:
            self.white_player_id = player_id
            if self.black_player_id is not None:
                self.game_status = "active"
            return True
        elif color == "black" and self.black_player_id is None:
            self.black_player_id = player_id
            if self.white_player_id is not None:
                self.game_status = "active"
            return True
        return False
    
    def is_ready(self) -> bool:
        """Check if both players have joined"""
        return self.white_player_id is not None and self.black_player_id is not None
    
    def add_move(self, player_id: str, move: Dict) -> bool:
        """Record a move - move should contain: from, to, promotion (optional)"""
        # Verify it's the correct player's turn
        expected_player = self.white_player_id if self.current_turn == "white" else self.black_player_id
        
        if player_id != expected_player:
            return False
        
        self.moves.append({
            "player_id": player_id,
            "color": self.current_turn,
            "from": move.get("from"),
            "to": move.get("to"),
            "promotion": move.get("promotion"),
            "move_number": len(self.moves) + 1
        })
        
        # Toggle turn
        self.current_turn = "black" if self.current_turn == "white" else "white"
        return True
    
    def get_game_state(self) -> Dict:
        """Get current game state"""
        return {
            "game_id": self.game_id,
            "white_player_id": self.white_player_id,
            "black_player_id": self.black_player_id,
            "board_state": self.board_state,
            "moves": self.moves,
            "current_turn": self.current_turn,
            "game_status": self.game_status,
            "winner": self.winner,
            "reason": self.reason,
            "move_count": len(self.moves)
        }
    
    def end_game(self, winner: str, reason: str):
        """End the game"""
        self.game_status = "finished"
        self.winner = winner  # "white", "black", or "draw"
        self.reason = reason  # "checkmate", "resignation", "timeout", "draw"


class GameSessionManager:
    """Manages all active game sessions (in-memory)"""
    
    def __init__(self):
        self.sessions: Dict[str, GameSession] = {}
    
    def create_game(self) -> str:
        """Create a new game session and return game_id"""
        game_id = str(uuid.uuid4())[:8]
        self.sessions[game_id] = GameSession(game_id)
        return game_id
    
    def get_session(self, game_id: str) -> Optional[GameSession]:
        """Get a game session by ID"""
        return self.sessions.get(game_id)
    
    def delete_session(self, game_id: str) -> bool:
        """Delete a game session"""
        if game_id in self.sessions:
            del self.sessions[game_id]
            return True
        return False
    
    def list_active_games(self) -> List[Dict]:
        """List all active games"""
        return [session.get_game_state() for session in self.sessions.values()]
