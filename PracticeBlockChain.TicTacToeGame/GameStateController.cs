using PracticeBlockChain.Cryptography;
using System;
using System.Linq;

namespace PracticeBlockChain.TicTacToeGame
{
    public static class GameStateController
    {
        public static bool IsAbletoPut(string[,] board, Position position)
        {
            // If IsAbletoPut() returns False, 
            // it means another player already put his token on that position.
            if (board[position.X, position.Y].Length > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsEnd(string[,] board)
        {
            var countofFilledTuples = 0;

            foreach(string tuple in board)
            {
                if (tuple.Length > 0)
                {
                    countofFilledTuples++;
                }
            }
            if (countofFilledTuples >= 9)
            {
                Console.WriteLine("무승부!");
                return true;
            }
            if(board[0, 0].Length > 0)
            {
                if(
                    String.Equals(
                        board[0, 0],
                        board[1, 1],
                        StringComparison.CurrentCulture
                    )
                    &&
                    String.Equals(
                        board[1, 1],
                        board[2, 2],
                        StringComparison.CurrentCulture
                    )
                )
                {
                    Console.WriteLine($"{board[0, 0]} 승리!");
                    return true;
                }
            }
            if (board[0, 2].Length > 0)
            {
                if (
                    String.Equals(
                        board[0, 2],
                        board[1, 1],
                        StringComparison.CurrentCulture
                    )
                    &&
                    String.Equals(
                        board[1, 1],
                        board[2, 0],
                        StringComparison.CurrentCulture
                    )
                )
                {
                    Console.WriteLine($"{board[0, 2]} 승리!");
                    return true;
                }
            }
            for (var row = 0; row < 3; row++)
            {
                if (board[row, 0].Length > 0)
                {
                    if (
                        String.Equals(
                            board[row, 0],
                            board[row, 1],
                            StringComparison.CurrentCulture
                        )
                        && 
                        String.Equals(
                            board[row, 1],
                            board[row, 2],
                            StringComparison.CurrentCulture
                        )
                    )
                    {
                        Console.WriteLine($"{board[row, 0]} 승리!");
                        return true;
                    }
                }
            }
            for (var column = 0; column < 3; column++)
            {
                if (board[0, column].Length > 0)
                {
                    if (
                        String.Equals(
                            board[0, column],
                            board[1, column],
                            StringComparison.CurrentCulture
                        )
                        &&
                        String.Equals(
                            board[1, column],
                            board[2, column],
                            StringComparison.CurrentCulture
                        )
                    )
                    {
                        Console.WriteLine($"{board[0, column]} 승리!");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
