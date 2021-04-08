using PracticeBlockChain.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        public static void Main(string[] args)
        {
            if (args[0].Equals("8888"))
            {
                var seedNodeThread = new Thread(StartSeedNode);
                seedNodeThread.Start();
            }
            else
            {
                var nodeThread = new Thread(StartNode);
                nodeThread.Start();
            }
        }

        private static void StartSeedNode()
        {
            var seedNode = new Node();
        }
    }
}
