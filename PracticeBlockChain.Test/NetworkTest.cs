﻿using PracticeBlockChain.Cryptography;
using PracticeBlockChain.Network;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace PracticeBlockChain.Test
{
    public class NetworkTest
    {
        private const int _seedPort = 65000;
        private static string _playerStorage;

        public static void Main(string[] args)
        {
            var nodeType = args[0];
            var blockChain = new BlockChain();
            Node node;
            PrivateKey privateKey = null;

            if (nodeType.Equals("seed"))
            {
                node = new Node(port: _seedPort);
                node.StartListener();
            }
            else
            {
                node = new Node(port: int.Parse(args[1]));
                try
                {
                    if (Directory.Exists(Path.Combine(_playerStorage, args[2])))
                    {
                        keyValue = args[2];
                        privateKey = new PrivateKey
                        (
                            args[2].Split("-").Select(x => Convert.ToByte(x)).ToArray()
                        );
                    }
                    else
                    {
                        Console.WriteLine("Invalid private key.");
                        return;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    privateKey = new PrivateKey();
                    foreach (byte byteValue in privateKey.ByteArray)
                    {
                        keyValue += byteValue.ToString();
                        keyValue += "-";
                    }
                    Directory.CreateDirectory
                    (
                        Path.Combine(_playerStorage, keyValue)
                    );
                }
                finally
                {
                    _playerStorage = Path.Combine(_playerStorage, keyValue);
                }
                var blockChain = new BlockChain(_playerStorage);
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