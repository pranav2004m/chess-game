using System.Collections;
using System.Collections.Generic;
using ChessModel;
using UnityEngine;
using UnityEngine.AI;

public class PieceManager : MonoBehaviour
{
    
    private BoardManager _boardManager;
    public GameObject explosion;

    void Awake()
    {
        _boardManager = GetComponentInParent<BoardManager>();
    }
    
    public void MovePiece(GameObject piece, Vector3 placement, bool rock = false)
    {
        PiecePieces piecePieces = piece.GetComponent<PiecePieces>();
        if (piecePieces != null)
        {
            piecePieces.Move(placement, rock);
        }
        else
        {
            // For standard chess pieces, just move directly without animation
            piece.transform.position = placement;
            FinishedAnim();
        }
    }

    public void AttackWithPiece(GameObject piece, Vector3 placement, Vector3 enemyPlacement, GameObject enemy)
    {
        PiecePieces piecePieces = piece.GetComponent<PiecePieces>();
        if (piecePieces != null)
        {
            piecePieces.Attack(placement, enemyPlacement, enemy);
        }
        else
        {
            // For standard chess pieces, move attacker and deactivate captured piece
            piece.transform.position = placement;
            enemy.SetActive(false);
            FinishedAnim();
        }
    }

    public void FinishedAnim()
    {
        _boardManager.NextTurn();
    }
}
