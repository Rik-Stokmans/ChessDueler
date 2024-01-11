using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;
using PieceList = ChessChallenge.API.PieceList;
using Timer = ChessChallenge.API.Timer;

public class MyBot : IChessBot
{
    private const int PawnValue = 100;
    private const int KnightValue = 300;
    private const int BishopValue = 300;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private const int KingValue = 10000;
    
    private static readonly int[] Pawns = {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5,  5, 10, 25, 25, 10,  5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5, -5,-10,  0,  0,-10, -5,  5,
        5, 10, 10,-20,-20, 10, 10,  5,
        0,  0,  0,  0,  0,  0,  0,  0
    };
    private static readonly int[] Knights = {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    };
    private static readonly int[] Bishops = {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    };
    private static readonly int[] Rooks = {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        0,  0,  0,  5,  5,  0,  0,  0
    };
    private static readonly int[] Queens = {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
        0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };
    private static readonly int[] KingMiddle = {
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        20, 20,  0,  0,  0,  0, 20, 20,
        20, 30, 10,  0,  0, 10, 30, 20
    };
    
    const float EndgameMaterialStart = RookValue * 2 + BishopValue + KnightValue;
    
    public Move Think(Board board, Timer timer)
    {
        return FindBestMove(board, timer);
    }
    
    private Move FindBestMove(Board board, Timer timer)
    {
        var moves = GetOrderedMoves(board.GetLegalMoves());
        
        var bestMove = moves[0];
        var bestEval = -900.0;

        var depth = DetermineDepth(board, timer, moves);
        
        foreach (var move in moves)
        {
            board.MakeMove(move);
            var eval = -Search(board, depth, double.MinValue, double.MaxValue);
            board.UndoMove(move);
            
            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    private int DetermineDepth(Board board, Timer timer, Move[] moves)
    {
        var pawnCount = board.GetPieceList(PieceType.Pawn, true).Count + board.GetPieceList(PieceType.Pawn, false).Count;
        var pieceCount = board.GetAllPieceLists().Sum(pieceList => pieceList.Count);
        var pieceCountWithoutKings = pieceCount - pawnCount/2 - 2;

        var moveAmountBothSides = moves.Length;
        if (moves.Length > 0)
        {
            board.MakeMove(moves[0]);
            moveAmountBothSides += board.GetLegalMoves().Length;
            board.UndoMove(moves[0]);
        }
        
        var depth = 3;
        if (pieceCountWithoutKings < 12 && moveAmountBothSides < 65) depth = 4;
        if (pieceCountWithoutKings < 9  && moveAmountBothSides < 50) depth = 5;
        if (pieceCountWithoutKings < 5  && moveAmountBothSides < 35) depth = 6;
        if (pieceCountWithoutKings < 2  && moveAmountBothSides < 25) depth = 7;
        if (timer.MillisecondsRemaining < 10000) depth -= 2;
        depth = 6;
        
        Console.WriteLine("Pieces left: " + pieceCountWithoutKings);
        Console.WriteLine("\nlegal moves: (This Side) " + moves.Length + ", (Total) " + moveAmountBothSides);
        Console.WriteLine("\ndepth: " + depth);
        
        return depth;
    } 
    
    private double Search(Board board, int depth, double alpha, double beta)
    {
        if (depth == 0) 
            return SearchAllCaptures(board, alpha, beta);
        
        if (board.IsInCheckmate()) return double.MinValue - (100 - depth);
        if (board.IsDraw()) return 0;

        var moves = GetOrderedMoves(board.GetLegalMoves());

        foreach (var move in moves)
        {
            board.MakeMove(move);
            double eval = -Search(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);
            
            if (move.IsCastles) eval += 500;
            
            if (eval >= beta) return beta;
            
            alpha = Math.Max(alpha, eval);
        }
        return alpha;
    }
    private double SearchAllCaptures(Board board, double alpha, double beta)
    {
        double eval = Eval(board);
        if (eval >= beta) return beta;
        alpha = Math.Max(alpha, eval);
        
        var moves = GetOrderedMoves(board.GetLegalMoves(true));
        
        foreach (var move in moves)
        {
            board.MakeMove(move);
            eval = -SearchAllCaptures(board, -beta, -alpha);
            board.UndoMove(move);
            
            if (eval >= beta) return beta;
            
            alpha = Math.Max(alpha, eval);
        }
        
        return alpha;
    }

    
    private int Eval(Board board)
    {
        int whiteEval = 0;
        int blackEval = 0;

        int whiteMaterial = CountMaterial(board, true);
        int blackMaterial = CountMaterial(board, false);
        
        int whiteMaterialWithoutPawns = whiteMaterial - board.GetPieceList(PieceType.Pawn, true).Count  * PawnValue;
        int blackMaterialWithoutPawns = blackMaterial - board.GetPieceList(PieceType.Pawn, false).Count * PawnValue;
        
        float whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialWithoutPawns);
        float blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialWithoutPawns);

        whiteEval += whiteMaterial;
        blackEval += blackMaterial;
        
        whiteEval += board.HasKingsideCastleRight(true) ? 20 : -15;
        whiteEval += board.HasQueensideCastleRight(true) ? 15 : -10;
        blackEval += board.HasKingsideCastleRight(false) ? 20 : -15;
        blackEval += board.HasQueensideCastleRight(false) ? 15 : -10;

        var moves = board.GetLegalMoves();
        if (board.IsWhiteToMove) whiteEval += moves.Length;
        else blackEval += moves.Length;
        
        whiteEval += MopUpEval (board, true , whiteMaterial, blackMaterial, blackEndgamePhaseWeight);
        blackEval += MopUpEval (board, false, blackMaterial, whiteMaterial, whiteEndgamePhaseWeight);
        
        whiteEval += EvaluatePieceSquareTables(board, board.IsWhiteToMove, blackEndgamePhaseWeight);
        blackEval += EvaluatePieceSquareTables(board, !board.IsWhiteToMove, whiteEndgamePhaseWeight);
        
        int eval = whiteEval - blackEval;
        int perspective = board.IsWhiteToMove ? 1 : -1;
        
        return eval * perspective;
    }
    
    private int CountMaterial(Board board, bool isWhite)
    {
        var score = 0;
        foreach (var pieceList in board.GetAllPieceLists())
        {
            if (pieceList.IsWhitePieceList == isWhite)
            {
                score += GetPieceScore(pieceList.TypeOfPieceInList) * pieceList.Count;
            }
        }
        return score;
    }
    
    private int GetPieceScore(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => PawnValue,
            PieceType.Knight => KnightValue,
            PieceType.Bishop => BishopValue,
            PieceType.Rook => RookValue,
            PieceType.Queen => QueenValue,
            _ => 0
        };
    }

    
    private Move[] GetOrderedMoves(Move[] moves)
    {
        return moves.OrderByDescending(GetMoveScore).ToArray();
    }
    private int GetMoveScore(Move move)
    {
        var moveScore = 0;
        var movePieceType = move.MovePieceType;
        var capturePieceType = move.CapturePieceType;

        if (capturePieceType != PieceType.None) 
            moveScore = 10 * GetPieceScore(capturePieceType) - GetPieceScore(movePieceType);

        if (move.IsPromotion) 
            moveScore += 250;
        
        if (move.MovePieceType == PieceType.King) 
            moveScore -= 100;
        
        return moveScore;
    }
    
    
    private float EndgamePhaseWeight(int materialCountWithoutPawns) 
    {
        const float multiplier = 1 / EndgameMaterialStart;
        return 1 - Math.Min(1, materialCountWithoutPawns * multiplier);
    }
    private int MopUpEval (Board board, bool white, int myMaterial, int opponentMaterial, float endgameWeight) {
        int mopUpScore = 0;
        if (myMaterial > opponentMaterial + PawnValue * 2 && endgameWeight > 0) {
            
            int friendlyKingSquare = board.GetKingSquare(white).Index;
            int opponentKingSquare = board.GetKingSquare(!white).Index;
            mopUpScore += PrecomputedMoveData.CentreManhattanDistance[opponentKingSquare] * 10;
            // use ortho dst to promote direct opposition
            mopUpScore += (14 - PrecomputedMoveData.NumRookMovesToReachSquare(friendlyKingSquare, opponentKingSquare)) * 4;

            return (int) (mopUpScore * endgameWeight);
        }
        return 0;
    }
    private int EvaluatePieceSquareTables (Board board, bool isWhite, float endgamePhaseWeight) {
        int value = 0;
        value += EvaluatePieceSquareTable(Pawns, board.GetPieceList(PieceType.Pawn, isWhite), isWhite);
        value += EvaluatePieceSquareTable(Rooks, board.GetPieceList(PieceType.Rook, isWhite), isWhite);
        value += EvaluatePieceSquareTable(Knights, board.GetPieceList(PieceType.Knight, isWhite), isWhite);
        value += EvaluatePieceSquareTable(Bishops, board.GetPieceList(PieceType.Bishop, isWhite), isWhite);
        value += EvaluatePieceSquareTable(Queens, board.GetPieceList(PieceType.Queen, isWhite), isWhite);
        int kingEarlyPhase = ReadPieceSquareTable(KingMiddle, board.GetKingSquare(isWhite).Index, isWhite);
        value += (int) (kingEarlyPhase * (1 - endgamePhaseWeight));
        //value += PieceSquareTable.Read (PieceSquareTable.kingMiddle, board.KingSquare[colourIndex], isWhite);

        return value;
    }
    private static int EvaluatePieceSquareTable(int[] table, PieceList pieceList, bool isWhite)
    {
        int value = 0;
        for (var i = 0; i < pieceList.Count; i++) {
            value += ReadPieceSquareTable(table, pieceList[i].Square.Index, isWhite);
        }
        return value;
    }
    private static int ReadPieceSquareTable(int[] table, int square, bool isWhite)
    {
        if (isWhite) {
            var file = BoardHelper.FileIndex(square);
            var rank = BoardHelper.RankIndex(square);
            rank = 7 - rank;
            square = rank * 8 + file;
        }

        return table[square];
    }
}