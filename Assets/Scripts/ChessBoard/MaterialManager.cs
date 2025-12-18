using System.Collections;
using System.Collections.Generic;
using ChessModel;
using UnityEngine;

public class MaterialManager : MonoBehaviour {
    
    public Material PawnWhite;
    public Material PawnBlack;
    public Material BishopWhite;
    public Material BishopBlack;
    public Material RookWhite;
    public Material RookBlack;
    public Material KingWhite;
    public Material KingBlack;
    public Material KnightWhite;
    public Material KnightBlack;
    public Material QueenWhite;
    public Material QueenBlack;
    
    public GameObject changeMaterial(GameObject Piece, ChessType chessType, ChessColor color)
    {
        // Try to get MeshRenderer first (for standard chess pieces), then SkinnedMeshRenderer (for monster pieces)
        Renderer renderer = Piece.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            renderer = Piece.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        
        if (renderer == null)
        {
            Debug.LogWarning("No MeshRenderer or SkinnedMeshRenderer found on piece: " + Piece.name);
            return Piece;
        }

        switch (chessType) 
        {
            case ChessType.Bishop: 
                renderer.material = color == ChessColor.Black ? BishopBlack : BishopWhite; 
                break;
            case ChessType.Rook:
                renderer.material = color == ChessColor.Black ? RookBlack : RookWhite;
                break;
            case ChessType.Pawn: 
                renderer.material = color == ChessColor.Black ? PawnBlack : PawnWhite;
                break;
            case ChessType.King: 
                renderer.material = color == ChessColor.Black ? KingBlack : KingWhite;
                break;
            case ChessType.Knight: 
                renderer.material = color == ChessColor.Black ? KnightBlack : KnightWhite;
                break;
            case ChessType.Queen: 
                renderer.material = color == ChessColor.Black ? QueenBlack : QueenWhite;
                break;
        }
        return Piece;
    }
}
