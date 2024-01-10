using System;
using Chess_Challenge.API;
using Chess_Challenge.Framework.Chess.Helpers;
using Move = Chess_Challenge.Framework.Chess.Board.Move;

namespace Chess_Challenge.Framework.Application.Helpers.API_Helpers
{

    public class MoveHelper
    {
        public static (Move move, PieceType pieceType, PieceType captureType) CreateMoveFromName(string moveNameUCI, API.Board board)
        {
            int indexStart = BoardHelper.SquareIndexFromName(moveNameUCI[0] + "" + moveNameUCI[1]);
            int indexTarget = BoardHelper.SquareIndexFromName(moveNameUCI[2] + "" + moveNameUCI[3]);
            char promoteChar = moveNameUCI.Length > 3 ? moveNameUCI[^1] : ' ';

            PieceType promotePieceType = promoteChar switch
            {
                'q' => PieceType.Queen,
                'r' => PieceType.Rook,
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                _ => PieceType.None
            };

            Square startSquare = new Square(indexStart);
            Square targetSquare = new Square(indexTarget);


            PieceType movedPieceType = board.GetPiece(startSquare).PieceType;
            PieceType capturedPieceType = board.GetPiece(targetSquare).PieceType;

            // Figure out move flag
            int flag = Move.NoFlag;

            if (movedPieceType == PieceType.Pawn)
            {
                if (targetSquare.Rank is 7 or 0)
                {
                    flag = promotePieceType switch
                    {
                        PieceType.Queen => Move.PromoteToQueenFlag,
                        PieceType.Rook => Move.PromoteToRookFlag,
                        PieceType.Knight => Move.PromoteToKnightFlag,
                        PieceType.Bishop => Move.PromoteToBishopFlag,
                        _ => 0
                    };
                }
                else
                {
                    if (Math.Abs(targetSquare.Rank - startSquare.Rank) == 2)
                    {
                        flag = Move.PawnTwoUpFlag;
                    }
                    // En-passant
                    else if (startSquare.File != targetSquare.File && board.GetPiece(targetSquare).IsNull)
                    {
                        flag = Move.EnPassantCaptureFlag;
                    }
                }
            }
            else if (movedPieceType == PieceType.King)
            {
                if (Math.Abs(startSquare.File - targetSquare.File) > 1)
                {
                    flag = Move.CastleFlag;
                }
            }

            Move coreMove = new Move(startSquare.Index, targetSquare.Index, flag);
            return (coreMove, movedPieceType, capturedPieceType);
        }


    }
}
