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
            var txNonce = 0;
            var blockIndex = 1;
            var blockChain = new BlockChain();

            // Initialize board.
            string[,] board = InitializeBoard();

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
            PrintBoard(board);
            PrintTipofBlock(blockChain);
            var _isFirstplayerTurn = true;

            while (!(StateController.IsEnd(board)))
            {
                // Player
                Position position = DecidePositiontoPut(
                    address: 
                    (
                        _isFirstplayerTurn ? 
                        firstPlayerAddress : secondPlayerAddress
                    ), 
                    txNonce: txNonce
                );
                if(
                    (position.X < 0) 
                    || (position.X > 2) 
                    || (position.Y < 0) 
                    || (position.Y > 2)
                    || !(StateController.IsAbletoPut(board, position))
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
                // Sign the action.
                byte[] signature =
                    (_isFirstplayerTurn ? firstPlayerPrivateKey : secondPlayerPrivateKey)
                    .Sign(
                        new Action(
                            txNonce: txNonce, 
                            signer: 
                            (
                                _isFirstplayerTurn ? 
                                firstPlayerAddress : secondPlayerAddress
                            ),
                            payload: position, 
                            signature: null
                        )
                        .Hash()
                    );
                // Verify the action. 
                // Suppose that the player who isn't this turn 
                // tries to verify the action of this turn player.
                Action action = 
                    new Action(
                        txNonce: txNonce, 
                        signer: 
                        (
                            _isFirstplayerTurn ? 
                            firstPlayerAddress : secondPlayerAddress
                        ), 
                        payload: position, 
                        signature: signature
                    );
                bool isValidAction =
                    (_isFirstplayerTurn ? firstPlayerPublicKey : secondPlayerPublicKey)
                    .Verify(action.Hash(), action.Signature);
                Assert.True(isValidAction);

                // Proof of work.
                Nonce nonce =
                    HashCash
                    .CalculateHash
                    (
                        previousBlock: blockChain.GetBlock(blockChain.HashofTipBlock),
                        blockChain: blockChain
                    );

                // Need Validation.
                // If pass the validation, make block (with executing action).
                Block block = new Block(
                    index: blockChain.GetBlock(blockChain.HashofTipBlock).Index + 1,
                    previousHash: blockChain.HashofTipBlock,
                    timeStamp: DateTimeOffset.Now,
                    nonce: nonce,
                    action: action
                );
                // first "board" = next state, second "board" = current state.
                board = blockChain.AddBlock(block, board);
                _isFirstplayerTurn = !(_isFirstplayerTurn);

                //Print current block.
                PrintBoard(board);
                PrintTipofBlock(blockChain);
            }
        }

        private static string[,] InitializeBoard()
        {
            var board = new string[3, 3];
            for (var row = 0; row < 3; row++)
            {
                for (var calmn = 0; calmn < 3; calmn++)
                {
                    board[row, calmn] = "";
                }
            }
            return board;
        }

        private static Position DecidePositiontoPut(Address address, long txNonce)
        {
            // Input "5 3" means player will put his tuple on the (5, 3).
            Console.Write(
                $"{AddressPlayerMappingAttribute.GetPlayer(address)}이 이동할 위치 입력: "
            );
            string[] input = Console.ReadLine().Split(' ');
            return new Position(int.Parse(input[0]), int.Parse(input[1]));
        }

        private static void PrintBoard(string[,] board)
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------");
            for(var row = 0; row < 3; row++)
            {
                for(var column = 0; column < 3; column++)
                {
                    if (board[row, column].Length > 0)
                    {
                        Console.Write($"   {board[row, column]}   ");
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
                    "Move to (" +
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.Payload.X +
                    ", " +
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.Payload.Y +
                    ")"
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
