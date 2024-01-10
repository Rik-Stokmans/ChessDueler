using Chess_Challenge.Framework.Chess.Board;
using Move = Chess_Challenge.API.Move;

namespace Chess_Challenge.Framework.Application.Helpers
{
    public static class Warmer
    {

        public static void Warm()
        {
            Board b = new();
            b.LoadStartPosition();
            API.Board board = new API.Board(b);
            Move[] moves = board.GetLegalMoves();

            board.MakeMove(moves[0]);
            board.UndoMove(moves[0]);
        }

    }
}
