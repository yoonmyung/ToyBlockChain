using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class PrintingResult
    {
        public static void PrintState(Tuple<string, Position> storedState)
        {
            Console.WriteLine(
                $"{storedState.Item1}: " + 
                $"<{storedState.Item2.X}, {storedState.Item2.Y}>에 말을 둠"
            );
            Console.WriteLine("<현재 보드판>");
            Printboard(storedState.Item1, storedState.Item2);
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
