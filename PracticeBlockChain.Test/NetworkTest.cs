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
        private static Mutex _mutex;

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
                for (var i = 0; i < 5; i++)
                {
                    SynchronizeTransportingAction(privateKey, blockChain, node);
                }
            }
        }

        private static void SynchronizeTransportingAction
        (
            PrivateKey privateKey, BlockChain blockChain, Node node
        )
        {
            _mutex.WaitOne();
            var actionTask =
                Task.Factory.StartNew(() => TransportAction(privateKey, blockChain, node));
            actionTask.Wait();
            _mutex.ReleaseMutex();
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