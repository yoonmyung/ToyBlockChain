using PracticeBlockChain.Cryptography;
using PracticeBlockChain.Network;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        private const int _seedPort = 65000;

        public static void Main(string[] args)
        {
            var nodeType = args[0];
            var blockChain = new BlockChain();
            var privateKey = new PrivateKey();

            Node node;

            if (nodeType.Equals("seed"))
            {
                node = new Node(port: _seedPort);
                node.StartListener();
            }
            else
            {
                node = new Node(port: int.Parse(args[1]));
                node.StartListener();
                node.Send
                (
                    _seedPort, 
                    string.Format(node.Address.client + "," + node.Address.listener)
                );
                node.RotateRoutingTable
                (
                    string.Format(node.Address.client + "," + node.Address.listener)
                );
                Task.Run
                (
                    () =>
                    {
                        if (nodeType.Equals("miner"))
                        {
                            while (true)
                            {
                                var block = Mine(blockChain);
                                node.RotateRoutingTable(block.BlockHeader.Hash());
                            }
                        }
                        else if (nodeType.Equals("player"))
                        {
                            while (true)
                            {
                                if (node.RoutingTable.Count >= 2)
                                {
                                    var action = MakeAction(privateKey, blockChain);
                                    node.RotateRoutingTable
                                    (
                                        new ArrayList
                                        {
                                        privateKey.PublicKey.Format(true),
                                        action.Signature,
                                        action.ActionId
                                        }
                                    );
                                }
                            }
                        }
                    }
                );
            }
            Task.WaitAll();
        }

        private static Action MakeAction(PrivateKey privateKey, BlockChain blockChain)
        {
            var address = new Address(privateKey.PublicKey);
            var random = new Random();
            var action =
                new Action
                (
                    txNonce:
                    blockChain.GetHowmanyBlocksMinermade(address) + 1,
                    signer: address,
                    payload: new TicTacToeGame.Position(random.Next(), random.Next()),
                    signature:
                        privateKey.Sign
                        (
                            new Action
                            (
                                txNonce: blockChain.GetHowmanyBlocksMinermade(address) + 1,
                                signer: address,
                                payload: new TicTacToeGame.Position(random.Next(), random.Next()),
                                signature: null
                            ).Hash()
                        )
                );

            return action;
        }

        private static Block Mine(BlockChain blockChain)
        {
            var blockHash = HashCash.CalculateBlockHash(blockChain);
            var blockHeader = new BlockHeader
                (
                    index: blockChain.TipBlock.BlockHeader.Index + 1,
                    previousHash: blockChain.TipBlock.BlockHeader.Hash(),
                    timeStamp: DateTimeOffset.Now,
                    nonce: blockHash.nonce,
                    difficulty: blockHash.difficulty
                );
            Console.WriteLine
            (
                $"Nonce: {string.Join("-", blockHash.nonce.NonceValue)}, " +
                $"difficulty: {blockHash.difficulty}\n"
            );
            var block = new Block(blockHeader: blockHeader, action: null);

            return block;
        }
    }
}