using PracticeBlockChain.TicTacToeGame;
using System;
using System.Collections.Generic;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<byte[], Block> _blocks;
        private byte[] _hashofTipBlock;
        private readonly Block _genesisBlock;
        private long _difficulty;

        public BlockChain()
        {
            Random random = new Random();
            _blocks = new Dictionary<byte[], Block>();
            _genesisBlock = MakeGenesisBlock();
            _difficulty = random.Next(10000);
        }

        public byte[] HashofTipBlock
        {
            get
            {
                return this._hashofTipBlock;
            }
        }

        public long Difficulty
        {
            get 
            {
                return this._difficulty;
            }
            set
            {
                this._difficulty = value;
            }
        }

        private Block MakeGenesisBlock()
        {
            // Add an empty block.
            var block =
                new Block
                (
                    index: 0,
                    previousHash: null,
                    timeStamp: DateTimeOffset.Now,
                    nonce: new NonceGenerator().GenerateNonce(),
                    action: null
                );
            this._hashofTipBlock = block.Hash();
            _blocks.Add(_hashofTipBlock, block);
            return block;
        }

        public string[,] AddBlock(Block block, string[,] currentBoard)
        {
            // Validate block.

            // After validate block, then add the block to the blockchain.
            this._hashofTipBlock = block.Hash();
            _blocks.Add(HashofTipBlock, block);
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
                Difficulty = 
                    DifficultyUpdater.UpdateDifficulty
                    (
                        difficulty: Difficulty, 
                        previouTimeStamp: _genesisBlock.TimeStamp, 
                        currentTimeStamp: block.TimeStamp
                    );
            }
            else
            {
                Difficulty = 
                    DifficultyUpdater.UpdateDifficulty
                    (
                        difficulty: Difficulty, 
                        previouTimeStamp: _blocks[block.PreviousHash].TimeStamp, 
                        currentTimeStamp: block.TimeStamp
                    );
            }
            return updatedBoard;
        }

        public Block GetBlock(byte[] hashValue)
        {
            try
            {
                var returnedBlock = _blocks[hashValue];
                return returnedBlock;
            }
            catch (KeyNotFoundException exception)
            {
                return null;
            }
            catch (ArgumentNullException exception)
            {
                return _genesisBlock;
            }
        }

        public IEnumerable<Block> IterateBlock()
        {
            foreach (Block block in _blocks.Values)
            {
                yield return block;
            }
        }
    }
}
