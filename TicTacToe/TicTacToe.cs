using System;
using System.Collections.Generic;

namespace Game
{
    public class TicTacToe
    {
        private readonly string[,] board = new string[3, 3];
        private readonly Dictionary<string, Position> storedStates;

        public void Execute(Tuple<string, Position> statetoUpdate)
        {
            board[statetoUpdate.Item2.X, statetoUpdate.Item2.Y] = 
                statetoUpdate.Item1;
            PrintingResult.PrintState(statetoUpdate);
            storedStates.Add(statetoUpdate.Item1, statetoUpdate.Item2);
        }

        public bool IsAbletoPut(Tuple<string, Position> input)
        {
            // If IsAbletoPut() returns False, 
            // it means another player already put his token on that position.
            if (board[input.Item2.X, input.Item2.Y].Length > 0)
            {
                return false;
            }
            return true;
        }

        public bool IsEnd()
        {
            // If IsEnd() returns True, it means there's a winner.
            if (board[0, 0].Contains(board[1, 1])
                == board[2, 2].Contains(board[1, 1]))
            {
                return true;
            }
            for (int index = 0; index < 3; index++)
            {
                if (board[index, 0].Length > 0)
                {
                    if (board[index, 0].Contains(board[index, 1]) 
                        == board[index, 1].Contains(board[index, 2]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
