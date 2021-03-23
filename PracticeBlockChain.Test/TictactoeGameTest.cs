using System;
using Xunit;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.Diagnostics;

namespace PracticeBlockChain.Test
{
    public class TictactoeGameTest
    {
        public static void Main()
        {
            // txNonce update 수정
            // state 제네릭화
            var blockChain = new BlockChain();

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

            // Print Genesis block.
            PrintCurrentState(blockChain);
            PrintTipofBlock(blockChain);
            var isFirstplayerTurn = true;

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
                    (position.X < 0) 
                    || (position.X > 2) 
                    || (position.Y < 0) 
                    || (position.Y > 2)
                    || !(GameStateController
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
                // First "board" is the next state, 
                // second "board" is the current state.
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
            string[,] currentState = blockChain.GetCurrentState();
            Console.WriteLine();
            Console.WriteLine("---------------------------");
            for(var row = 0; row < 3; row++)
            {
                for(var column = 0; column < 3; column++)
                {
                    if (currentState[row, column].Length > 0)
                    {
                        Console.Write($"   {currentState[row, column]}   ");
                    }
                    else
                    {
                        Console.Write($"         ");
                    }
                }
                Console.WriteLine("\n---------------------------\n");
            }
        }

        private static void PrintTipofBlock(BlockChain blockChain)
        {
            Debug.WriteLine("-----------------------------------------------");
            Debug.Write("Block #");
            Debug.WriteLine(blockChain.GetBlock(blockChain.HashofTipBlock).Index);
            Debug.Write("Nonce: ");
            try
            {
                Debug.WriteLine(
                    String.Join(
                        " ",
                        blockChain.GetBlock(blockChain.HashofTipBlock).Nonce.NonceValue
                    )
                );
            } catch(Exception exception)
            {
                Debug.WriteLine("null");
            }
            Debug.Write("Previous Hash: ");
            if (!(blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash is null))
            {
                Debug.WriteLine(
                    String.Join(
                        " ", 
                        blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash
                    )
                );
            }
            else
            {
                Debug.WriteLine("null");
            }
            Debug.Write("Current Hash: ");
            Debug.WriteLine(
                String.Join(
                    " ", 
                    blockChain.GetBlock(blockChain.HashofTipBlock).Hash()
                )
            );
            Debug.Write("TimeStamp: ");
            Debug.WriteLine(blockChain.GetBlock(blockChain.HashofTipBlock).TimeStamp);
            Debug.Write("Action: ");
            if (!(blockChain.GetBlock(blockChain.HashofTipBlock).GetAction is null))
            {
                Debug.WriteLine(
                    "Put (" +
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.Payload.X +
                    ", " +
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.Payload.Y +
                    ")"
                );
                Debug.Write("Action txNonce: ");
                Debug.WriteLine(
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.TxNonce
                );
            }
            else
            {
                Debug.WriteLine("null");
            }
            Debug.Write("Difficulty: ");
            Debug.WriteLine(blockChain.Difficulty);
            Debug.WriteLine("-----------------------------------------------");
        }
    }
}
