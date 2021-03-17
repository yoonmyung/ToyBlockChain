using PracticeBlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PracticeBlockChain.TicTacToeGame
{
    public class Board
    {
        private readonly string[,] board = new string[3, 3];
        private readonly Dictionary<string, Position> storedStates;

        public string[,] Put(
            string[,] currentBoard,
            Position position,
            Player player
        )
        {
            currentBoard[position.X, position.Y] = player.Name;
            string[,] updatedBoard = currentBoard;
            storedStates.Add(player.Name, position);
            return updatedBoard;
        }

        public void Execute((string player, Position position) statetoUpdate)
        {
            board[statetoUpdate.position.X, statetoUpdate.position.Y] = 
                statetoUpdate.player;
            PrintState();
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

        public void PrintState()
        {
        }

        private void Printboard(string player, Position position)
        {
            for (int index = 0; index < 3; index++)
            {
                Console.WriteLine("--------------------------------");
                if (index == position.X)
                {
                    for (int term = 0; term < 3; term++)
                    {
                        if (term == position.Y)
                        {
                            Console.Write($"  {player}  ");
                            continue;
                        }
                        Console.Write("       ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("--------------------------------");
            }
        }
    }
}
