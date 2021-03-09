using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<byte[], Block> blocks 
            = new Dictionary<byte[], Block>(new ByteArrayComparer());
        private readonly Block genesisBlock;

        public void MakeBlock(
            long index,
            byte[] previousHash, 
            byte[] hashValue, 
            DateTimeOffset timeStamp, 
            Nonce nonce,
            long difficulty
        )
        {
            blocks.Add(hashValue, 
                       new Block(index, previousHash, timeStamp, nonce, difficulty));
        }

        public Block GetBlock(byte[] hashValue)
        {
            Block returnedBlock = blocks[hashValue];

            return returnedBlock;
        }

/*        public byte[] GetHashofBlock(Block blockToGetHash)
        {
            var returnedHash = 
                blocks.FirstOrDefault(block => block.Value == blockToGetHash).Key;

            return returnedHash;
        }
*/
        public static IEnumerable<byte[]> IterateBlock()
        {
            foreach(Block block in blocks.Values)
            {
                yield return block;
            }
        }
    }
}
