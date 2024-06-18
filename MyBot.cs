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
    private Dictionary<ulong, MinimaxResult> transpositionTable= new Dictionary<ulong, MinimaxResult>();

    Dictionary<string, int[]> scoreTable= new Dictionary<string, int[]>{
        {"Pawn", new int[] {
            100, 100, 100, 100, 100, 100, 100, 100,
            50,  50,  50,  50,  50,  50,  50,  50,
            10,  10,  20,  30,  30,  20,  10,  10,
            5,   5,   10,  25,  25,  10,  5,   5,
            0,   0,   0,   20,  20,  0,   0,   0,
            5,  -5,  -10,  0,   0,  -10, -5,   5,
            5,  10,  10, -20, -20,  10,  10,  5,
            0,   0,   0,   0,   0,   0,   0,   0
        }},
        {"Knight", new int[] {
            -50, -40, -30, -30, -30, -30, -40, -50,
            -40, -20,   0,   0,   0,   0, -20, -40,
            -30,   0,  10,  15,  15,  10,   0, -30,
            -30,   -10,  0,  20,  20,  0,   -10, -30,
            -30,   0,  15,  20,  20,  15,   0, -30,
            -30,   5,  10,  15,  15,  10,   5, -30,
            -40, -20,   0,   5,   5,   0, -20, -40,
            -50, -40, -30, -30, -30, -30, -40, -50
        }},
        {"Bishop", new int[] {
            -20, -10, -10, -10, -10, -10, -10, -20,
            -10,   0,   0,   0,   0,   0,   0, -10,
            -10,   0,   5,  10,  10,   5,   0, -10,
            -10,   5,   5,  10,  10,   5,   5, -10,
            -10,   0,  10,  10,  10,  10,   0, -10,
            -10,  10,  10,  10,  10,  10,  10, -10,
            -10,   5,   0,   0,   0,   0,   5, -10,
            -20, -10, -10, -10, -10, -10, -10, -20
        }},
        {"Rook", new int[] {
            0,   0,   0,   0,   0,   0,   0,   0,
            5,  10,  10,  10,  10,  10,  10,   5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            0,   0,   0,   5,   5,   0,   0,   0
        }},
        {"Queen", new int[]{
            -20, -10, -10,  -5,  -5, -10, -10, -20,
            -10,   0,   0,   0,   0,   0,   0, -10,
            -10,   0,   5,   5,   5,   5,   0, -10,
            -5,   0,   5,   5,   5,   5,   0,  -5,
            0,   0,   5,   5,   5,   5,   0,  -5,
            -10,   5,   5,   5,   5,   5,   0, -10,
            -10,   0,   5,   0,   0,   0,   0, -10,
            -20, -10, -10,  -5,  -5, -10, -10, -20
        }},
        {"King", new int[] {
            -10,   -10,   0,   0,   0,   0,   0,  0,
            0,   0,   0,   0,   0,   0,   0,  0,
            0,   0,   0,   0,   0,   0,   0,  0,
            0,   0,   0,   0,   0,   0,   0,  0,
            0,   0,   0,   0,   0,   0,   0,  0,
            0,   0,   0,   0,   0,   0,   0,  0,
            -10,   -10,   -10,   -10,   -10,   -10,   -10,  -10,
            25,   75,   10,   -75, -25,   10,   75,   25
        }},
    };
    public int[] flipDictionary(int[] dictionary){
        int[] flippedArray = new int[dictionary.Length];
        int rows = 8;
        int cols = 8;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                flippedArray[row * cols + (cols - 1 - col)] = dictionary[row * cols + col];
            }
        }

        return flippedArray;
    }
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
        bool botIsWhite = board.IsWhiteToMove;
        Random random = new Random();
        Move[] moves = board.GetLegalMoves();
        int randomNumber = random.Next(moves.Length);
        MinimaxResult result = Minimax(board, 3, botIsWhite, int.MinValue, int.MaxValue);
        
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
    public int ForceKingEval(Board board, bool botIsWhite){
        int eval=0;
        int multiplier=1;
        if(!botIsWhite){
            multiplier=-1;
        }
        
        if(board.HasQueensideCastleRight(botIsWhite)){
            eval+=250*multiplier;
        }
         if(board.HasKingsideCastleRight(botIsWhite)){
            eval+=250*multiplier;
        }
        eval+=ForceKingToCorner(board, board.GetKingSquare(botIsWhite), board.GetKingSquare(!botIsWhite), CalculateEndGameWeight(board));
        return eval;
    }
    public float CalculateEndGameWeight(Board board){
        int pieceCount = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();

        foreach(PieceList pieceList in pieceLists){
            pieceCount+=pieceList.Count;
        }

        return 3/pieceCount;
    }
    public int CalculateScoreTable(Board board,bool botCol){
        int eval=0;
        int m=1;
        if(!botCol){m=-1;}
        PieceList[] PieceLists = board.GetAllPieceLists();
        foreach(PieceList pieceList in PieceLists){
            for(int i=0; i<pieceList.Count; i++){
                Piece piece = pieceList.GetPiece(i);
                PieceType pT = piece.PieceType;
                bool PieceColor=piece.IsWhite;
                int PieceValue;
                if (PieceColor){
                    PieceValue = (scoreTable[$"{pT}"])[piece.Square.Index]*m;
                }else{
                    PieceValue = flipDictionary(scoreTable[$"{pT}"])[piece.Square.Index]*m;
                }
                eval+=PieceValue;
            }
        }
        
        return eval;
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
        return Evaluate+ForceKingEval(board, botColor)+CalculateScoreTable(board, botColor);
    }
    public MinimaxResult Minimax(Board board, int depth, bool maximizingPlayer, int alpha, int beta)
    {
        int evaluation = Eval(board, maximizingPlayer);
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
        {
            if(!transpositionTable.ContainsKey(board.ZobristKey)){
                MinimaxResult result = new MinimaxResult(evaluation, Move.NullMove);
                transpositionTable[board.ZobristKey]=result;
                return result;
            }
            return transpositionTable[board.ZobristKey];
            
        }

        Move bestMove = Move.NullMove;
        if (maximizingPlayer)
        {
            int bestEvaluation = int.MinValue;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                MinimaxResult result;
                if(transpositionTable.ContainsKey(board.ZobristKey)){
                    result = transpositionTable[board.ZobristKey];
                }else{
                    result = Minimax(board, depth - 1, false, alpha, beta);
                }
                
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
