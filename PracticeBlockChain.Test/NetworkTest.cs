using PracticeBlockChain.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        public static void Main(string[] args)
        {
            bool isNodeMade = false;

            while (true)
            {
                if (!isNodeMade)
                {
                    if (args[0].Equals("8888"))
                    {
                        var seedNode = new Node(true);
                        var clientThread = new Thread(seedNode.Listen);
                        clientThread.Start();
                    }
                    else
                    {
                        var node = new Node(false);
                        node.ConnectToNode("127.0.0.1:8888");
                    }
                    isNodeMade = true;
                }
            }
        }
    }
}
