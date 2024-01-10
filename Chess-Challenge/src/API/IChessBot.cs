
namespace Chess_Challenge.API
{
    public interface IChessBot
    {
        Move Think(Board board, Timer timer);
    }
}
