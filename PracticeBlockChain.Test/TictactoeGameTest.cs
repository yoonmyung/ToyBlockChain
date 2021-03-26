using System;
using Xunit;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace PracticeBlockChain.Test
{
    public class TictactoeGameTest
    {
        public static void Main()
        {
            // state 제네릭화 (보류)
            // Block Validation 추가

            var blockChain = new BlockChain();
            blockChain.SetGenesisBlock();

            // Set the first player.
            var firstPlayerPrivateKey = new PrivateKey();
            var firstPlayerPublicKey = firstPlayerPrivateKey.PublicKey;
            var firstPlayerAddress = new Address(firstPlayerPublicKey);
            Assert.NotNull(firstPlayerAddress.AddressValue);
            AddressPlayerMappingAttribute.AddPlayer(
                address: firstPlayerAddress, 
                playerName: "Kim"
            );
            // Set the second player.
            var secondPlayerPrivateKey = new PrivateKey();
            var secondPlayerPublicKey = secondPlayerPrivateKey.PublicKey;
            var secondPlayerAddress = new Address(secondPlayerPublicKey);
            Assert.NotNull(secondPlayerAddress.AddressValue);
            AddressPlayerMappingAttribute.AddPlayer(
                address: secondPlayerAddress,
                playerName: "Lee"
            );
            var isFirstplayerTurn = true;

            // Print Genesis block.
            PrintCurrentState(blockChain);
            PrintTipofBlock(blockChain);
            while (!(GameStateController.IsEnd(blockChain.GetCurrentState())))
            {
                // Player
                Position position = DecidePositiontoPut(
                    (
                        isFirstplayerTurn ? 
                        firstPlayerAddress : secondPlayerAddress
                    )
                );
                if(
                    (position.X < 0) || 
                    (position.X > 2) || 
                    (position.Y < 0) ||
                    (position.Y > 2) ||
                    !(GameStateController
                        .IsAbletoPut(blockChain.GetCurrentState(), position)
                    )
                )
                {
                    Console.WriteLine($"({position.X}, {position.Y})에 둘 수 없습니다");
                    continue;
                }
                // │
                // │   Player는 BlockChain에게 자신이 수행한 것을 전달
                // │
                // ▼
                // BlockChain
                // Make an action without signature.
                byte[] signature =
                    (isFirstplayerTurn ? firstPlayerPrivateKey : secondPlayerPrivateKey)
                    .Sign(
                        new Action(
                            txNonce: 
                            blockChain.GetHowmanyBlocksMinermade
                            (
                                (
                                    isFirstplayerTurn ?
                                    firstPlayerAddress : secondPlayerAddress
                                )
                            ) + 1, 
                            signer: 
                            (
                                isFirstplayerTurn ? 
                                firstPlayerAddress : secondPlayerAddress
                            ),
                            payload: position, 
                            signature: null
                        )
                        .Hash()
                    );
                // Add a signature to an action.
                Action action = 
                    new Action(
                        txNonce:
                        blockChain.GetHowmanyBlocksMinermade
                        (
                            (
                                isFirstplayerTurn ?
                                firstPlayerAddress : secondPlayerAddress
                            )
                        ) + 1,
                        signer: 
                        (
                            isFirstplayerTurn ? 
                            firstPlayerAddress : secondPlayerAddress
                        ), 
                        payload: position, 
                        signature: signature
                    );
                // Verify an action. (Validation)
                // 본래는 같은 네트워크 상에 있는 다른 노드들이 검증함
                bool isValidAction =
                    (isFirstplayerTurn ? firstPlayerPublicKey : secondPlayerPublicKey)
                    .Verify(action.Hash(), action.Signature);
                Assert.True(isValidAction);
                // 작업증명
                Nonce nonce =
                    HashCash
                    .CalculateHash
                    (
                        previousBlock: blockChain.GetBlock(blockChain.HashofTipBlock),
                        blockChain: blockChain
                    );
                // 작업증명에 대한 검증 작업 필요 (Libplanet의 Policy)
                // Make block with executing action.
                Block block = new Block(
                    index: blockChain.GetBlock(blockChain.HashofTipBlock).Index + 1,
                    previousHash: blockChain.HashofTipBlock,
                    timeStamp: DateTimeOffset.Now,
                    nonce: nonce,
                    action: action
                );
                blockChain.AddBlock(block);
                isFirstplayerTurn = !(isFirstplayerTurn);
                //Print current state and the tip of block.
                PrintCurrentState(blockChain);
                PrintTipofBlock(blockChain);
            }
        }

        private static Position DecidePositiontoPut(Address address)
        {
            // Input "5 3" means player will put his tuple on the (5, 3).
            Console.Write(
                $"{AddressPlayerMappingAttribute.GetPlayer(address)}이 이동할 위치 입력: "
            );
            string[] input = Console.ReadLine().Split(' ');
            return new Position(int.Parse(input[0]), int.Parse(input[1]));
        }

        private static void PrintCurrentState(BlockChain blockChain)
        {
            byte[] serializedState = 
                GetObjectFromStorage(blockChain, blockChain.HashofTipBlock);
            Dictionary<int, string> currentState = 
                (Dictionary<int, string>)
                ByteArrayConverter.DeSerialize(serializedState);
            Console.WriteLine("---------------------------");
            foreach (int tuple in currentState.Keys)
            {
                Console.Write(
                    "   " + 
                    (currentState[tuple].Length == 0 ? "   " : currentState[tuple]) + 
                    "   "
                );
                if (tuple % 3 == 0)
                {
                    Console.WriteLine("\n---------------------------");
                }
            }
        }

        private static void PrintTipofBlock(BlockChain blockChain)
        {
            byte[] serializedBlock = 
                GetObjectFromStorage(blockChain, blockChain.HashofTipBlock);
            Dictionary<string, object> tipBlock =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedBlock);
            Debug.WriteLine("-----------------------------------------------");
            foreach (string tuple in tipBlock.Keys)
            {
                Debug.Write(tuple + ": ");
                try
                {
                    if (tipBlock[tuple] is null)
                    {
                        // It's genesis block.
                        Debug.WriteLine("null");
                    }
                    else if (tipBlock[tuple].GetType().Name == "Byte[]")
                    {
                        byte[] byteArray = (byte[])tipBlock[tuple];
                        Debug.WriteLine(string.Join("", byteArray));
                    }
                    else
                    {
                        Debug.WriteLine(tipBlock[tuple]);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("null");
                }
            }
            PrintAction(blockChain);
            Debug.WriteLine("-----------------------------------------------");
        }

        private static void PrintAction(BlockChain blockChain)
        {
            byte[] serializedAction = 
                GetObjectFromStorage
                (
                    blockChain, 
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.ActionId
                );
            Dictionary<string, object> action =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedAction);
            foreach (string tuple in action.Keys)
            {
                Debug.Write(tuple + ": ");
                try
                {
                    if (action[tuple] is null)
                    {
                        // It's genesis block.
                        Debug.WriteLine("null");
                    }
                    else if (action[tuple].GetType().Name == "Byte[]")
                    {
                        byte[] byteArray = (byte[])action[tuple];
                        Debug.WriteLine(string.Join("", byteArray));
                    }
                    else
                    {
                        Debug.WriteLine(action[tuple]);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("null");
                }
            }
        }

        public static byte[] GetObjectFromStorage
            (BlockChain blockChain, byte[] hashValue)
        {
            return File.ReadAllBytes(
                blockChain.ActionStorage + "\\"
                + string.Join("", hashValue)
                + ".txt"
            );
        }
    }
}
