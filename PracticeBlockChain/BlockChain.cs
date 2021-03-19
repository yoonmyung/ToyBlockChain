using PracticeBlockChain.TicTacToeGame;
using System;
using System.Collections.Generic;

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
                    action: null
                );
            this.hashofTipBlock = block.Hash();
            blocks.Add(hashofTipBlock, block);
            return block;
        }

        public string[,] AddBlock(Block block, string[,] currentBoard)
        {
            // After validate block, then add the block to the blockchain.
            this.hashofTipBlock = block.Hash();
            blocks.Add(HashofTipBlock, block);

            // Execute action.
            string[,] updatedBoard = 
                StateController.Execute(
                    currentState: currentBoard, 
                    position: block.GetAction.Payload, 
                    address: block.GetAction.Signer
                ); 

            // Update Difficulty.
            if (block.PreviousHash is null)
            {
                // It's a genesis block.
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
            return updatedBoard;
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
