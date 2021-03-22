namespace PracticeBlockChain.TicTacToeGame
{
    public class Position
    {
        private readonly int _x;
        private readonly int _y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X
        {
            get;
        }

        public int Y
        {
            get;
        }
    }
}
