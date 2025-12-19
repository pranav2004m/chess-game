"""
Main Flask Server for Chess Game
Provides REST API endpoints for chess game management
"""
from flask import Flask, request, jsonify
from flask_cors import CORS
from game_session import GameSessionManager
import uuid

app = Flask(__name__)
CORS(app)  # Enable CORS for Unity clients

# Initialize session manager
session_manager = GameSessionManager()


# ==================== ENDPOINTS ====================

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy"}), 200


@app.route('/game/create', methods=['POST'])
def create_game():
    """
    Create a new game session
    Returns: {game_id, status}
    """
    game_id = session_manager.create_game()
    return jsonify({
        "success": True,
        "game_id": game_id,
        "message": "Game created successfully"
    }), 201


@app.route('/game/<game_id>/join', methods=['POST'])
def join_game(game_id):
    """
    Join an existing game
    Body: {player_id, color} where color is "white" or "black"
    Returns: {success, game_state}
    """
    data = request.get_json()
    player_id = data.get('player_id')
    color = data.get('color')
    
    if not player_id or not color:
        return jsonify({"success": False, "error": "Missing player_id or color"}), 400
    
    if color not in ['white', 'black']:
        return jsonify({"success": False, "error": "Color must be 'white' or 'black'"}), 400
    
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    if not session.add_player(player_id, color):
        return jsonify({"success": False, "error": f"Color {color} already taken"}), 400
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200


@app.route('/game/<game_id>/state', methods=['GET'])
def get_game_state(game_id):
    """
    Get current game state
    Returns: {game_state}
    """
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200


@app.route('/game/<game_id>/move', methods=['POST'])
def make_move(game_id):
    """
    Make a move in the game
    Body: {player_id, from, to, promotion (optional)}
    Returns: {success, game_state, error}
    """
    data = request.get_json()
    player_id = data.get('player_id')
    move_from = data.get('from')
    move_to = data.get('to')
    promotion = data.get('promotion')
    
    if not player_id or not move_from or not move_to:
        return jsonify({
            "success": False, 
            "error": "Missing player_id, from, or to"
        }), 400
    
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    if session.game_status != "active":
        return jsonify({
            "success": False, 
            "error": "Game is not active"
        }), 400
    
    # Create move object
    move = {
        "from": move_from,
        "to": move_to,
        "promotion": promotion
    }
    
    # Try to add move
    if not session.add_move(player_id, move):
        return jsonify({
            "success": False, 
            "error": "Invalid move or not your turn"
        }), 400
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200


@app.route('/game/<game_id>/end', methods=['POST'])
def end_game(game_id):
    """
    End the game
    Body: {winner, reason} where winner is "white", "black", or "draw"
    Returns: {success, game_state}
    """
    data = request.get_json()
    winner = data.get('winner')
    reason = data.get('reason')
    
    if not winner or not reason:
        return jsonify({
            "success": False, 
            "error": "Missing winner or reason"
        }), 400
    
    session = session_manager.get_session(game_id)
    if not session:
        return jsonify({"success": False, "error": "Game not found"}), 404
    
    session.end_game(winner, reason)
    
    return jsonify({
        "success": True,
        "game_state": session.get_game_state()
    }), 200


@app.route('/game/<game_id>/delete', methods=['DELETE'])
def delete_game(game_id):
    """
    Delete/cleanup a game session
    Returns: {success}
    """
    if session_manager.delete_session(game_id):
        return jsonify({"success": True, "message": "Game deleted"}), 200
    return jsonify({"success": False, "error": "Game not found"}), 404


@app.route('/games', methods=['GET'])
def list_games():
    """
    List all active games
    Returns: {games: []}
    """
    games = session_manager.list_active_games()
    return jsonify({
        "success": True,
        "games": games,
        "count": len(games)
    }), 200


# ==================== ERROR HANDLERS ====================

@app.errorhandler(404)
def not_found(error):
    return jsonify({"success": False, "error": "Endpoint not found"}), 404


@app.errorhandler(500)
def internal_error(error):
    return jsonify({"success": False, "error": "Internal server error"}), 500


if __name__ == '__main__':
    print("🎮 Chess Server Starting...")
    print("📡 Running on http://localhost:8888")
    print("📚 API Docs: http://localhost:8888/api/docs")
    app.run(debug=True, host='0.0.0.0', port=8888)
