using PracticeBlockChain.Cryptography;
using PracticeBlockChain.Network;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        private static object lockThis = new object();

        public static async Task Main(string[] args)
        {
            var blockChain = new BlockChain();
            var privateKey = new PrivateKey();
            var node = new Node(isSeed: bool.Parse(args[0]), port: int.Parse(args[1]));

            if (!bool.Parse(args[0]))
            {
                // It's peer node.
                node.RotateRoutingTable
                (
                    string.Format(node.Address[0] + "," + node.Address[1])
                );
                while (node.RoutingTable.Count < 2) ;
                
                while (true)
                {
                    var sendingActionThread =
                        new Thread(() => SendAction(lockThis, privateKey, blockChain, node));
                    sendingActionThread.Priority = ThreadPriority.Lowest;
                    sendingActionThread.Start();
                    await Task.Delay(10000);
                }
            }
        }

        private static void SendAction
        (
            object lockThis, PrivateKey privateKey, BlockChain blockChain, Node node
        )
        {
            lock (lockThis)
            {
                var action = MakeAction(privateKey, blockChain);
                node.RotateRoutingTable
                (
                    new ArrayList { privateKey.PublicKey.Format(true), action.Signature, action.ActionId }
                );
            }
        }

        private static Action MakeAction(PrivateKey privateKey, BlockChain blockChain)
        {
            var address = new Address(privateKey.PublicKey);
            byte[] signature =
                privateKey.Sign
                (
                    new Action
                    (
                        txNonce: blockChain.GetHowmanyBlocksMinermade(address) + 1,
                        signer: address,
                        payload: null,
                        signature: null
                    ).Hash()
                );
            var action =
                new Action
                (
                    txNonce:
                    blockChain.GetHowmanyBlocksMinermade(address) + 1,
                    signer: address,
                    payload: null,
                    signature: signature
                );

            return action;
        }
    }
}