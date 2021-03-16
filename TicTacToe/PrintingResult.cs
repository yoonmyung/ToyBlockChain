using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class PrintingResult
    {
        public static void PrintState((string player, Position position) storedState)
        {
            Console.WriteLine(
                $"{storedState.player}: " + 
                $"<{storedState.position.X}, {storedState.position.Y}>에 말을 둠"
            );
            Console.WriteLine("<현재 보드판>");
            Printboard(storedState.player, storedState.position);
        }

        private static void Printboard(string player, Position position)
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
