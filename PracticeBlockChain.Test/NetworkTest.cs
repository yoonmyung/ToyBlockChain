using PracticeBlockChain.Network;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        public static void Main(string[] args)
        {
            var node = new Node(isSeed: bool.Parse(args[0]), port: int.Parse(args[1]));
            if (!bool.Parse(args[0]))
            {
                // It's peer node.
                node.RotateRoutingTable();
            }
        }
    }
}