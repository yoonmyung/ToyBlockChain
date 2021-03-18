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
        private readonly Dictionary<byte[], Block> blocks 
            = new Dictionary<byte[], Block>();
        private byte[] hashofTipBlock;
        private readonly Block genesisBlock;
        private readonly long difficulty = 0;

        public BlockChain()
        {
            genesisBlock = MakeGenesisBlock();
        }

        public byte[] HashofTipBlock
        {
            get
            {
                return this.hashofTipBlock;
            }
        }

        public long Difficulty
        {
            get; set;
        }

        private Block MakeGenesisBlock()
        {
            //빈 보드 상태
            Block block =
                new Block(
                    index: 0,
                    previousHash: null,
                    timeStamp: DateTimeOffset.Now,
                    nonce: new NonceGenerator().GenerateNonce(),
                    signature: null,
                    payload: null
                );
            this.hashofTipBlock = block.Hash();
            blocks.Add(hashofTipBlock, block);
            return block;
        }

        public void MakeBlock(byte[] hashValue, Block block)
        {
            // After validate block, then add the block to the blockchain.
            blocks.Add(hashValue, block);
            hashofTipBlock = hashValue;
//            Board.Put(block.State, player); 
            if (block.PreviousHash is null)
            {
                Difficulty = DifficultyUpdater.UpdateDifficulty(
                    Difficulty, genesisBlock.TimeStamp, block.TimeStamp
                );
            }
            else
            {
                Difficulty = DifficultyUpdater.UpdateDifficulty(
                    Difficulty, blocks[block.PreviousHash].TimeStamp, block.TimeStamp
                );
            }
        }

        public Block GetBlock(byte[] hashValue)
        {
            try
            {
                Block returnedBlock = blocks[hashValue];
                return returnedBlock;
            }
            catch (KeyNotFoundException exception)
            {
                return null;
            }
            catch (ArgumentNullException exception)
            {
                return genesisBlock;
            }
        }

        public IEnumerable<Block> IterateBlock()
        {
            foreach(Block block in blocks.Values)
            {
                yield return block;
            }
        }
    }
}
