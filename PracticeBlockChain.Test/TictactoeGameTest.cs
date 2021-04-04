using System;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracticeBlockChain.Test
{
    public static class TictactoeGameTest
    {
        public static async Task Main(string[] args)
        {
            var blockChain = new BlockChain();
            var privateKey =
                new PrivateKey(Path.GetFileNameWithoutExtension(args[0]).Split("-")
                .Select(x => Convert.ToByte(x)).ToArray());
            var publicKey = privateKey.PublicKey;
            var playerAddress = new Address(publicKey);

            if (args[0] ==
                "134-160-127-193-238-72-55-180-4-150-254-223-49-225-182-115-90-" +
                "145-24-243-74-208-79-219-28-33-84-51-34-12-194-213"
            )
            {
                AddressPlayerMappingAttribute.AddPlayer
                (
                    address: playerAddress,
                    playerName: "Kim"
                );
            }
            else if (args[0] ==
                "175-146-93-29-240-159-132-41-122-119-133-215-9-143-204-204-216-" +
                "105-204-145-162-194-222-88-164-69-249-1-25-48-215-29"
            )
            {
                AddressPlayerMappingAttribute.AddPlayer
                (
                    address: playerAddress,
                    playerName: "Lee"
                );
            }

            blockChain.LoadTipBlock();
            PrintCurrentState(blockChain, playerAddress);
            FileWatcher.RunWatcher(blockChain, playerAddress);
            while (!(await GameStateController.IsEnd(blockChain.GetCurrentState(playerAddress))))
            {
                // Player
                Position position = null;
                if (isPlayerTurn(blockChain, playerAddress))
                {
                    await Task.Delay(100);
                    position = DecidePositiontoPut(playerAddress);
                }
                else
                {
                    continue;
                }
                // │
                // │   Player는 BlockChain에게 자신이 수행한 것을 전달
                // │
                // ▼
                // BlockChain
                // Make an action without signature.
                byte[] signature =
                    privateKey.Sign
                    (
                        new Action
                        (
                            txNonce: blockChain.GetHowmanyBlocksMinermade(playerAddress) + 1,
                            signer: playerAddress,
                            payload: position,
                            signature: null
                        ).Hash()
                    );
                // Add a signature to an action.
                Action action =
                    new Action
                    (
                        txNonce:
                        blockChain.GetHowmanyBlocksMinermade(playerAddress) + 1,
                        signer: playerAddress,
                        payload: position,
                        signature: signature
                    );
                // Verify an action. (Validation)
                // 본래는 같은 네트워크 상에 있는 다른 노드들이 검증함
                bool isValidAction = privateKey.PublicKey.Verify(action.Hash(), action.Signature);
                // 작업증명
                Block block =
                    new Block
                    (
                        index: blockChain.TipBlock.Index + 1,
                        previousHash: blockChain.TipBlock.Hash(),
                        timeStamp: DateTimeOffset.Now,
                        nonce: HashCash.CalculateHash(blockChain),
                        action: action,
                        difficulty: DifficultyUpdater.UpdateDifficulty(blockChain)
                    );
                // 만들어진 블록을 체인에 붙이기 전 검증 작업 필요 (Libplanet의 Policy)
                if (!(blockChain.AddBlock(block)))
                {
                    Console.WriteLine($"({position.X}, {position.Y})에 둘 수 없습니다");
                    continue;
                }
            }
        }

        private static bool isPlayerTurn(BlockChain blockChain, Address address)
        {
            if (blockChain.TipBlock.Index == 0)
            {
                if (AddressPlayerMappingAttribute.GetPlayer(address) == "Kim")
                {
                    return true;
                }
                return false;
            }
            else if 
                (blockChain.TipBlock.GetAction.Signer.AddressValue.SequenceEqual(address.AddressValue))
            {
                return false;
            }
            return true;
        }

        private static Position DecidePositiontoPut(Address address)
        {
            // Input "5 3" means player will put his tuple on the (5, 3).
            Console.Write($"{AddressPlayerMappingAttribute.GetPlayer(address)} 이동할 위치 입력: ");
            string[] input = Console.ReadLine().Split(' ');
            return new Position(int.Parse(input[0]), int.Parse(input[1]));
        }

        public static void PrintCurrentState(BlockChain blockChain, Address address)
        {
            string[,] currentState = blockChain.GetCurrentState(address);

            Console.WriteLine("\n---------------------------");
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    Console.Write(
                        "   " +
                        (currentState[row, column].Length == 0 ? 
                            "   " : currentState[row, column]) +
                        "   "
                    );
                }
                Console.WriteLine("\n---------------------------");
            }
        }

        private static void PrintTipofBlock(BlockChain blockChain)
        {
            byte[] serializedBlock = 
                GetObjectFromStorage(blockChain.BlockStorage, blockChain.TipBlock.Hash());
            Dictionary<string, object> tipBlock =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedBlock);
            Console.WriteLine("-----------------------------------------------");
            foreach (string tuple in tipBlock.Keys)
            {
                Console.Write(tuple + ": ");
                try
                {
                    if (tipBlock[tuple] is null)
                    {
                        // It's genesis block.
                        Console.WriteLine("null");
                    }
                    else if (tipBlock[tuple].GetType().Name == "Byte[]")
                    {
                        byte[] byteArray = (byte[])tipBlock[tuple];
                        Console.WriteLine(string.Join("-", byteArray));
                    }
                    else
                    {
                        Console.WriteLine(tipBlock[tuple]);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("null");
                }
            }
            PrintAction(blockChain);
            Console.WriteLine("-----------------------------------------------");
        }

        private static void PrintWholeBlocks(BlockChain blockChain)
        {
            // Print blocks.
            IEnumerator<Block> enumerator = blockChain.IterateBlock().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Console.WriteLine("****************************");
                Console.WriteLine($"block index: {enumerator.Current.Index}");
                Console.WriteLine($"block nonce: " +
                    String.Join(" ", enumerator.Current.Nonce.NonceValue));
                if (enumerator.Current.PreviousHash is null)
                {
                    Console.WriteLine($"block previous hash: null");
                }
                else
                {
                    Console.WriteLine($"block previous hash: " +
                        String.Join(" ", enumerator.Current.PreviousHash)
                    );
                }
                Console.WriteLine($"block timestamp: {enumerator.Current.TimeStamp}");
            }
        }

        private static void PrintAction(BlockChain blockChain)
        {
            byte[] serializedAction = 
                GetObjectFromStorage
                (
                    blockChain.ActionStorage, 
                    blockChain.TipBlock.GetAction.ActionId
                );
            Dictionary<string, object> action =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedAction);
            foreach (string tuple in action.Keys)
            {
                Console.Write(tuple + ": ");
                try
                {
                    if (action[tuple] is null)
                    {
                        // It's genesis block.
                        Console.WriteLine("null");
                    }
                    else if (action[tuple].GetType().Name == "Byte[]")
                    {
                        byte[] byteArray = (byte[])action[tuple];
                        Console.WriteLine(string.Join("-", byteArray));
                    }
                    else
                    {
                        Console.WriteLine(action[tuple]);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("null");
                }
            }
        }

        public static byte[] GetObjectFromStorage
            (string storageAddress, byte[] hashValue)
        {
            return File.ReadAllBytes(
                Path.Combine(storageAddress, string.Join("-", hashValue) + ".txt")
            );
        }
    }
}
