using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<HashDigest, Block> blocks 
            = new Dictionary<HashDigest, Block>();
        private readonly Block genesisBlock;
        private readonly long difficulty = 0;

        public BlockChain(
            long index,
            HashDigest previousHash,
            HashDigest hashValue,
            DateTimeOffset timeStamp,
            Nonce nonce,
            byte[] signature
        )
        {
            genesisBlock = new Block(
                index,
                previousHash,
                timeStamp,
                nonce,
                signature
            );
            blocks.Add(hashValue,
                new Block(
                    index,
                    previousHash,
                    timeStamp,
                    nonce,
                    signature
                )
            );
        }

        public void MakeBlock(
            long index,
            HashDigest previousHash, 
            HashDigest hashValue, 
            DateTimeOffset timeStamp, 
            Nonce nonce,
            byte[] signature
        )
        {
            // After validate block, then add the block to the blockchain.
            blocks.Add(hashValue, 
                new Block(
                    index, 
                    previousHash, 
                    timeStamp, 
                    nonce, 
                    signature
                )
            );
            Difficulty = DifficultyUpdater.UpdateDifficulty(
                Difficulty, blocks[previousHash].TimeStamp, timeStamp
            );
        }

        public Block GetBlock(HashDigest hashValue)
        {
            Block returnedBlock = blocks[hashValue];
            return returnedBlock;
        }

        public IEnumerable<Block> IterateBlock()
        {
            foreach(Block block in blocks.Values)
            {
                yield return block;
            }
        }

        public long Difficulty
        {
            get; set;
        }
    }
}
