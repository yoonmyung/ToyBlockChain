using System;
using System.Collections.Generic;
using System.Text;
using practiceBlockchain;

namespace practiceBlockchain.test
{
    public class BlockChainTest
    {
        public static void Main()
        {
            byte[] hashValue = null;
            int i = 0;

            //make genesis block
            hashValue = 
                BlockChain.makeBlock(
                    null,
                    new byte[] {
                        0xd6, 0x93, 0xda, 0x38, 0x66, 0xa3, 0x4d, 0x65, 0x9e, 0x01, 0x4f, 0x97,
                        0xc8, 0xfe, 0xb0, 0x8a, 0xfe, 0x2e, 0x97, 0xc9, 0x9e, 0x3f, 0x33, 0x89,
                        0xda, 0x02, 0x5f, 0xd0, 0x66, 0x5c, 0x62, 0x1c
                    },
                    new DateTimeOffset(2021, 3, 1, 0, 0, 0, TimeSpan.Zero),
                    5
                );

            //starts making blocks 
            while (i++ < 10)
            {
                hashValue = 
                    BlockChain.getBlock(hashValue)
                              .calculateHash(BlockChain.Difficulty, hashValue);
            }
            IEnumerable<byte[]> hashesinBlockChain = BlockChain.iterateBlock();
            foreach (byte[] hash in hashesinBlockChain)
            {
                Console.WriteLine($"Hash value: {string.Join("-", hash)}");
            }
        }
    }
}
