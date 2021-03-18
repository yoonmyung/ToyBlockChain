using System;
using Xunit;
using PracticeBlockChain;
using PracticeBlockChain.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace PracticeBlockChain.Test
{
    public class BlockChainTest
    {
        [Fact]
        public void MiningTest()
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

                Action action = MakeAction(txNonce, address, payload);
                Assert.Equal(txNonce, action.TxNonce);
                Assert.Equal(address, action.Signer);
                Assert.Equal(payload, action.Payload);

                // l
                // l   Player는 BlockChain에게 Action 전달
                // l
                // ▼

                // BlockChain
                byte[] signedAction = privateKey.Sign(action.Hash());
                bool isValidAction = publicKey.Verify(action.Hash(), signedAction);
                Assert.True(isValidAction);
                Nonce nonce =
                    HashCash.CalculateHash(
                        previousBlock: blockChain.GetBlock(blockChain.HashofTipBlock),
                        blockChain: blockChain
                    );
                // Need Validation.
                // If pass the validation,
                Block block = new Block(
                    index: blockChain.GetBlock(blockChain.HashofTipBlock).Index + 1,
                    previousHash: blockChain.HashofTipBlock,
                    timeStamp: blockChain.GetBlock(blockChain.HashofTipBlock).TimeStamp.AddHours(1),
                    nonce: nonce,
                    signature: signedAction,
                    payload: action.Payload
                );
                blockChain.MakeBlock(block.Hash(), block);
                PrintTipofBlock(blockChain);
            }
        }

        private Action MakeAction(long txNonce, Address address, string payload)
        {
            return new Action(
                txNonce: txNonce,
                signer: address,
                payload: payload
            );
        }

        private void PrintTipofBlock(BlockChain blockChain)
        {
            Console.WriteLine();
            Console.WriteLine($"<block index>\n{blockChain.GetBlock(blockChain.HashofTipBlock).Index}");

            Console.WriteLine($"<block nonce>\n" +
                String.Join(" ", blockChain.GetBlock(blockChain.HashofTipBlock).Nonce.NonceValue));

            Console.WriteLine("<block previous hash>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(
                    String.Join(" ", blockChain.GetBlock(blockChain.HashofTipBlock).PreviousHash)
                );
            }

            Console.WriteLine($"<current hash>\n{String.Join(" ", blockChain.HashofTipBlock)}");

            Console.WriteLine("<block signature>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).Signature is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(
                    String.Join(" ", blockChain.GetBlock(blockChain.HashofTipBlock).Signature)
                );
            }

            Console.WriteLine(
                $"<block timestamp>\n{blockChain.GetBlock(blockChain.HashofTipBlock).TimeStamp}"
            );

            Console.WriteLine("<block action>");
            if (blockChain.GetBlock(blockChain.HashofTipBlock).Payload is null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(blockChain.GetBlock(blockChain.HashofTipBlock).Payload);
            }
            Console.WriteLine();
        }

        public static void Main()
        {
            BlockChainTest blockChainTest = new BlockChainTest();
            blockChainTest.MiningTest();
        }
    }
}
