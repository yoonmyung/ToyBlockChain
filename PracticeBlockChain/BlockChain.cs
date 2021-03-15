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
        private readonly long difficulty = 0;

        public BlockChain(
            long index,
            byte[] previousHash,
            byte[] hashValue,
            DateTimeOffset timeStamp,
            Nonce nonce,
            Action action
        )
        {
            genesisBlock = new Block(
                index,
                previousHash,
                timeStamp,
                nonce,
                action
            );
            blocks.Add(hashValue,
                new Block(
                    index,
                    previousHash,
                    timeStamp,
                    nonce,
                    action
                )
            );
        }

        public void MakeBlock(
            long index,
            byte[] previousHash, 
            byte[] hashValue, 
            DateTimeOffset timeStamp, 
            Nonce nonce,
            Action action
        )
        {
            // After validate block, then add the block to the blockchain.
            blocks.Add(hashValue, 
                new Block(
                    index, 
                    previousHash, 
                    timeStamp, 
                    nonce, 
                    action
                )
            );
            Difficulty = DifficultyUpdater.UpdateDifficulty(
                Difficulty, blocks[previousHash].TimeStamp, timeStamp
            );
        }

        // GetBlock과 GetHashofBlock은 Block.cs로 가는 게 맞음
        public Block GetBlock(byte[] hashValue)
        {
            Block returnedBlock = blocks[hashValue];
            return returnedBlock;
        }

        public byte[] GetHashofBlock(Block blockToGetHash)
        {
            var returnedHash = 
                blocks.FirstOrDefault(block => block.Value == blockToGetHash).Key;
            return returnedHash;
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
