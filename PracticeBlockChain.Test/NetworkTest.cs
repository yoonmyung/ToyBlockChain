using PracticeBlockChain.Network;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        public static void Main(string[] args)
        private static object lockThis = new object();

        {
            var node = new Node(isSeed: bool.Parse(args[0]), port: int.Parse(args[1]));
            if (!bool.Parse(args[0]))
            {
                // It's peer node.
                node.RotateRoutingTable();
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