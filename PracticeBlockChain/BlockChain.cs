using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using PracticeBlockChain.TicTacToeGame;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<BigInteger, Block> blocks 
            = new Dictionary<BigInteger, Block>();
        private readonly Block genesisBlock;
        private readonly long difficulty = 0;

        public BlockChain()
        {
            MakeGenesisBlock();
        }

        private void MakeGenesisBlock()
        {
            //빈 보드 상태
            Block block =
                new Block(
                    index: 0,
                    previousHash: 0,
                    timeStamp: DateTimeOffset.Now,
                    nonce: new NonceGenerator().GenerateNonce(),
                    signature: null,
                    state: new Board()
                );
            blocks.Add(0, block);
        }

        public void MakeBlock( BigInteger hashValue, Block block)
        {
            // After validate block, then add the block to the blockchain.
            blocks.Add(hashValue, block);
            Difficulty = DifficultyUpdater.UpdateDifficulty(
                Difficulty, blocks[block.PreviousHash].TimeStamp, block.TimeStamp
            );
        }

        public Block GetBlock(BigInteger hashValue)
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
