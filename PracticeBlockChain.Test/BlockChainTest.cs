using System;
using Xunit;
using PracticeBlockChain.Cryptography;

namespace PracticeBlockChain.Test
{
    public static class BlockChainTest
    {
        [Fact]
        public static void MiningTest()
        {
            long txNonce = 0;
            long blockIndex = 1;
            BlockChain blockChain = new BlockChain();

            PrivateKey privateKey = new PrivateKey();
            PublicKey publicKey = privateKey.PublicKey;
            Address address = new Address(publicKey);
            Assert.NotNull(address.AddressValue);

            // Print a genesis block.
            PrintTipofBlock(blockChain);

            while (true)
            {
                // Player
                Console.Write("액션(트랜젝션) 입력: ");
                string payload = Console.ReadLine();
                if (payload.Length <= 0)
                {
                    break;
                }

                // l
                // l   Player는 BlockChain에게 Action 전달
                // l   (Action 객체 자체보다는.. "게임에서 내가 뫄뫄를 했습니다"를 알린다는 느낌이 강한듯)
                // ▼

                // BlockChain
                // Make Action.
                Action action = MakeAction(txNonce, address, payload);
                Assert.Equal(txNonce, action.TxNonce);
                Assert.Equal(address, action.Signer);
                Assert.Equal(payload, action.Payload);
                // Sign.
                byte[] signature = privateKey.Sign(action.Hash());
                action.signature = signature;
                // Verify. 
                // (사실 Verify는 blockchain이 한다기보다는.. 다른 노드들이 하는 것)
                // 이 Action의 유효성을 검증하기 위해
                bool isValidAction = publicKey.Verify(action.Hash(), signature);
                Assert.True(isValidAction);
                // Proof of work.
                Nonce nonce =
                    HashCash.CalculateHash(
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
                blockChain.AddBlock(block);
                // Print result.
                PrintTipofBlock(blockChain);
            }
        }

        public static Action MakeAction(long txNonce, Address address, string payload)
        {
            return new Action(
                txNonce: txNonce,
                signer: address,
                payload: payload,
            );
        }

        public static void PrintTipofBlock(BlockChain blockChain)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"<block index>\n{blockChain.GetBlock(blockChain.HashofTipBlock).Index}"
            );

            Console.WriteLine($"<block nonce>\n" +
                String.Join(
                    " ", 
                    blockChain.GetBlock(blockChain.HashofTipBlock).Nonce.NonceValue
                )
            );

            Console.WriteLine("<block previous hash>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(
                    String.Join(
                        " ", 
                        blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash
                    )
                );
            }

            Console.WriteLine($"<current hash>\n{String.Join(" ", blockChain.HashofTipBlock)}");

            Console.WriteLine("<block signature>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).GetAction is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(
                    String.Join(
                        " ", 
                        blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.signature
                    )
                );
            }

            Console.WriteLine(
                $"<block timestamp>\n{blockChain.GetBlock(blockChain.HashofTipBlock).TimeStamp}"
            );

            Console.WriteLine("<block action>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).GetAction is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(
                    blockChain.GetBlock(blockChain.HashofTipBlock).GetAction.Payload
                );
            }
            Console.WriteLine();
        }
    }
}
