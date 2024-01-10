namespace Chess_Challenge.Framework.Chess.Result
{
    public enum GameResult
    {
        NotStarted,
        InProgress,
        WhiteIsMated,
        BlackIsMated,
        Stalemate,
        Repetition,
        FiftyMoveRule,
        InsufficientMaterial,
        DrawByArbiter,
        WhiteTimeout,
        BlackTimeout,
        WhiteIllegalMove,
        BlackIllegalMove
    }
}