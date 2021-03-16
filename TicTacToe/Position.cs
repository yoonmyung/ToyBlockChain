using PracticeBlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Position
    {
        private readonly int x;
        private readonly int y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X
        {
            get; set;
        }

        public int Y
        {
            get; set;
        }
    }
}
