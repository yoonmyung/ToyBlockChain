using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<byte[], Block> _blocks;
        private readonly Dictionary<long, string[,]> _states;
        private byte[] _hashofTipBlock;
        private readonly Block _genesisBlock;
        private long _difficulty;

        public BlockChain()
        {
            Random random = new Random();
            _blocks = new Dictionary<byte[], Block>();
            _states = new Dictionary<long, string[,]>();
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
            InitializeBoard();
            return block;
        }

        public void AddBlock(Block block)
        {
            // Validate block.
            // After validate block, then add the block to the blockchain.
            this._hashofTipBlock = block.Hash();
            _blocks.Add(HashofTipBlock, block);
            // Execute action.
            string[,] updatedBoard = 
                block.GetAction.Execute(
                    currentState: GetState(block.Index - 1), 
                    position: block.GetAction.Payload, 
                    address: block.GetAction.Signer
                );
            _states.Add(block.Index, updatedBoard);
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

        public string[,] GetState(long blockIndex)
        {
            try
            {
                var returnedState = _states[blockIndex];
                return returnedState;
            }
            catch (KeyNotFoundException exception)
            {
                return null;
            }
        }

        public string[,] GetCurrentState()
        {
            return _states[_states.Count - 1];
        }

        public long GetHowmanyBlocksMinermade(Address minerAddress)
        {
            var count = 0;
            foreach(Block block in IterateBlock())
            {
                if (block.GetAction is null)
                {
                    continue;
                }
                Address address = block.GetAction.Signer;
                if (address == minerAddress)
                {
                    count++;
                }
            }
            return count;
        }

        public IEnumerable<Block> IterateBlock()
        {
            foreach (Block block in _blocks.Values)
            {
                yield return block;
            }
        }

        private void InitializeBoard()
        {
            var board = new string[3, 3];
            for (var row = 0; row < 3; row++)
            {
                for (var calmn = 0; calmn < 3; calmn++)
                {
                    board[row, calmn] = "";
                }
            }
            _states.Add(0, board);
        }
    }
}
