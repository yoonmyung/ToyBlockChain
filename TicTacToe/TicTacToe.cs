using PracticeBlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class TicTacToe
    {
        private readonly string[,] board = new string[3, 3];
        private readonly Dictionary<string, Position> storedStates;

        public void Execute((string player, Position position) statetoUpdate)
        {
            board[statetoUpdate.position.X, statetoUpdate.position.Y] = 
                statetoUpdate.player;
            PrintingResult.PrintState(statetoUpdate);
            storedStates.Add(statetoUpdate.player, statetoUpdate.position);
        }

        public bool IsAbletoPut((string player, Position position) input)
        {
            // If IsAbletoPut() returns False, 
            // it means another player already put his token on that position.
            if (board[input.position.X, input.position.Y].Length > 0)
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

        public static byte[] Serialize(Position position)
        {
            byte[] input = (byte[])BitConverter.GetBytes(position.X)
                           .Concat(BitConverter.GetBytes(position.Y));
            return Serialization.Serialize(input);
        }
    }
}
