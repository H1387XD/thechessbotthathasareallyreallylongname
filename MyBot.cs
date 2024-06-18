using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ChessChallenge.API;
using static ChessChallenge.Application.ConsoleHelper;

public class MinimaxResult
{
    public int evaluation;
    public Move move;

    public MinimaxResult(int evaluation, Move move)
    {
        this.evaluation = evaluation;
        this.move = move;
    }
}
public class MyBot : IChessBot
{
    bool botIsWhite;
    Dictionary<string, int[]> WhitePieceBonuses = new Dictionary<string, int[]>
    {
        { "Pawn", new int[] {
                            0  ,0  ,0  ,0  ,0  ,0  ,0  ,0  ,
                            70 ,70 ,70 ,70 ,70 ,70 ,70 ,70 ,
                            60 ,60 ,60 ,60 ,60 ,60 ,60 ,60 ,
                            30 ,30 ,30 ,30 ,30 ,30 ,30 ,30 ,
                            0  ,0  ,0  ,50 ,50 ,0  ,0  ,0  ,
                            0  ,0  ,25 ,25 ,25 ,25  ,0  ,0 ,
                            0  ,0  ,0  ,0  ,0  ,0  ,0  ,0  ,
                            0  ,0  ,0  ,0  ,0  ,0  ,0  ,0}},
        { "Knight", new int[] {
                            -50   ,-25,-25,-25,-25,-25,-25,-50  ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,5  ,5  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50  ,
                            -50   ,0  ,20 ,0  ,0  ,20 ,0  ,-50 ,
                            -50   ,0  ,15 ,0  ,0  ,15 ,0 ,-50  ,
                            -100  ,0  ,0  ,0  ,0  ,0  ,0  ,-100}},
        { "King", new int[] {
                            -90,-90,-90,-90,-90,-90,-90,-90,
                            -80,-80,-80,-80,-80,-80,-80,-80,
                            -50,-50,-50,-50,-50,-50,-50,-50,
                            -30,-30,-30,-30,-30,-30,-30,-30,
                            -20,-20,-20,-20,-20,-20,-20,-20,
                            -20,-20,-20,-20,-20,-20,-20,-20,
                            -10,-10 ,-10 ,-10,-10,-10,-10,-10,
                            -10,0  ,70  ,0  ,0  ,0  ,70  ,-10
        }}
    };
    
    Dictionary<string, int[]> BlackPieceBonuses = new Dictionary<string, int[]>
    {
        { "Pawn", new int[]{0,0,0,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,
                            0  ,0  ,25 ,25 ,25 ,25  ,0  ,0 ,
                            0,0,0,50,50,0,0,0,
                            30,30,30,30,30,30,30,30,
                            60 ,60 ,60 ,60 ,60 ,60 ,60 ,60 ,
                            70 ,70 ,70 ,70 ,70 ,70 ,70 ,70 ,
                            0  ,0  ,0  ,0  ,0  ,0  ,0  ,0}},
        { "Knight", new int[] {
                            -100  ,0  ,0  ,0  ,0  ,0  ,0  ,-100,
                            -50   ,0  ,15 ,0  ,0  ,15 ,0  ,-50 ,
                            -50   ,0  ,20 ,0  ,0  ,20 ,0  ,-50 ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,5  ,5  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50 ,
                            -50   ,0  ,0  ,0  ,0  ,0  ,0  ,-50 ,
                            -50   ,-25,-25,-25,-25,-25,-25,-50}},
        { "King", new int[] {
                            -10,0  ,70  ,0  ,0  ,0  ,70  ,-10
                            -10,-10 ,-10 ,-10,-10,-10,-10,-10,
                            -20,-20,-20,-20,-20,-20,-20,-20,
                            -20,-20,-20,-20,-20,-20,-20,-20,
                            -30,-30,-30,-30,-30,-30,-30,-30,
                            -50,-50,-50,-50,-50,-50,-50,-50,
                            -80,-80,-80,-80,-80,-80,-80,-80,
                            -90,-90,-90,-90,-90,-90,-90,-90
        }}
    };
    public int ForceKingToCorner(Board board, Square friendlyKingSquare, Square opponentKingSquare, float endGameWeight){
        int eval=0;

        int opponentKingRank = friendlyKingSquare.Rank;
        int opponentKingFile = friendlyKingSquare.File;

        int dstToCenterRank = Math.Max(3-opponentKingRank, opponentKingRank-4);
        int dstToCenterFile = Math.Max(3-opponentKingFile, opponentKingFile-4);
        int dstToCenter=dstToCenterFile+dstToCenterRank;

        eval+=dstToCenter;

        int dstBetweenKingsFile = Math.Abs(friendlyKingSquare.File - opponentKingFile);
        int dstBetweenKingsRank = Math.Abs(friendlyKingSquare.Rank - opponentKingRank);
        eval+=14-(dstBetweenKingsRank+dstBetweenKingsFile);

        return (int)(eval*10*endGameWeight);
    }
    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;
        Random random = new Random();
        Move[] moves = board.GetLegalMoves();
        int randomNumber = random.Next(moves.Length);
        MinimaxResult result = Minimax(board, 4, botIsWhite, int.MinValue, int.MaxValue);
        if(result.evaluation==int.MaxValue || result.evaluation==int.MinValue){
            Log($"CHECKMATE | {result.move}");
        }else{
            Log($"{result.evaluation} | {result.move}");
        }
        
        if(result.move==Move.NullMove){
            return moves[0];
        }
        return result.move;
    }
    public static int GetPieceValue(string pieceName)
    {
        switch (pieceName)
        {
            case "None":
                return 0;
            case "Pawn":
                return 100;
            case "Knight":
                return 300;
            case "Bishop":
                return 320;
            case "Rook":
                return 500;
            case "Queen":
                return 900;
            case "King":
                return 0;
            default:
                return 0; // Default value if pieceName does not match any case
        }
    }
    public int ForceKingEval(Board board){
        int eval=0;

        eval+=ForceKingToCorner(board, board.GetKingSquare(botIsWhite), board.GetKingSquare(!botIsWhite), CalculateEndGameWeight(board));
        return eval;
    }
    public int PieceBonus_OPENING(Board board){
        PieceList[] PieceLists = board.GetAllPieceLists();
        int Evaluate=0;
        foreach(PieceList pieceList in PieceLists){
            for(int i=0; i<pieceList.Count; i++){
                Piece piece = pieceList.GetPiece(i);
                bool PieceColor=piece.IsWhite;
                if(piece.IsPawn){
                    
                    if (PieceColor){
                        int PieceValue = BlackPieceBonuses["Pawn"][piece.Square.Index];
                        Evaluate+=PieceValue;
                    }else{
                        int PieceValue = WhitePieceBonuses["Pawn"][piece.Square.Index];
                        Evaluate-=PieceValue;
                    }

                }
                if(piece.IsKnight){
                    
                    if (PieceColor){
                        int PieceValue = BlackPieceBonuses["Knight"][piece.Square.Index];
                        Evaluate+=PieceValue;
                    }else{
                        int PieceValue = WhitePieceBonuses["Knight"][piece.Square.Index];
                        Evaluate-=PieceValue;
                    }

                }
                if(piece.IsKing){
                    
                    if (PieceColor){
                        int PieceValue = BlackPieceBonuses["King"][piece.Square.Index];
                        Evaluate+=PieceValue;
                    }else{
                        int PieceValue = WhitePieceBonuses["King"][piece.Square.Index];
                        Evaluate-=PieceValue;
                    }

                }
                
            }
        }
        return Evaluate;
    }
    public float CalculateEndGameWeight(Board board){
        int pieceCount = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();

        foreach(PieceList pieceList in pieceLists){
            pieceCount+=pieceList.Count;
        }

        return pieceCount/64;
    }
    public int Eval(Board board, bool botColor){
        if(board.IsInCheckmate()){
            if(board.IsWhiteToMove){
                return int.MinValue;
            }else{
                return int.MaxValue;
            }
        }
        if(board.IsDraw()){
            return 0;
        }
        PieceList[] PieceLists = board.GetAllPieceLists();
        int Evaluate=0;
        foreach(PieceList pieceList in PieceLists){
            for(int i=0; i<pieceList.Count; i++){
                Piece piece = pieceList.GetPiece(i);
                PieceType pT = piece.PieceType;
                bool PieceColor=piece.IsWhite;
                int PieceValue = GetPieceValue($"{pT}");
                if (PieceColor){
                    Evaluate+=PieceValue;
                }else{
                    Evaluate-=PieceValue;
                }
            }
        }
        return Evaluate + PieceBonus_OPENING(board)+ForceKingEval(board);
    }
    public MinimaxResult Minimax(Board board, int depth, bool maximizingPlayer, int alpha, int beta)
    {
        int evaluation = Eval(board, maximizingPlayer);
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
        {
            return new MinimaxResult(evaluation, Move.NullMove);
        }

        Move bestMove = Move.NullMove;
        if (maximizingPlayer)
        {
            int bestEvaluation = int.MinValue;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                MinimaxResult result = Minimax(board, depth - 1, false, alpha, beta);
                if (result.evaluation > bestEvaluation)
                {
                    bestEvaluation = result.evaluation;
                    bestMove = move;
                }
                board.UndoMove(move);
                alpha = Math.Max(alpha, bestEvaluation);
                if(beta <= alpha){break;}
                
            }
            return new MinimaxResult(bestEvaluation, bestMove);
        }
        else
        {
            int bestEvaluation = int.MaxValue;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                MinimaxResult result = Minimax(board, depth - 1, true, alpha, beta);
                if (result.evaluation < bestEvaluation)
                {
                    bestEvaluation = result.evaluation;
                    bestMove = move;
                }
                
                board.UndoMove(move);
                beta = Math.Min(beta, bestEvaluation);
                if(beta <= alpha){break;}
                
            }
            return new MinimaxResult(bestEvaluation, bestMove);
        }
    }

}