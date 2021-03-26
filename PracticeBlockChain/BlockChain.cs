using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<byte[], Block> _blocks;
        private readonly Dictionary<long, string[,]> _states;
        private byte[] _hashofTipBlock;
        private Block _genesisBlock;
        private long _difficulty;
        private readonly string _blockStorage;
        private readonly string _actionStorage;
        private readonly string _stateStorage;

        public BlockChain()
        {
            Random random = new Random();
            _blocks = new Dictionary<byte[], Block>();
            _states = new Dictionary<long, string[,]>();
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

        public string BlockStorage
        {
            get
            {
                return _blockStorage;
            }
        }

        public string ActionStorage
        {
            get
            {
                return _actionStorage;
            }
        }

        public string StateStorage
        {
            get
            {
                return _stateStorage;
            }
        }

        public void SetGenesisBlock()
        {
            // Add an empty block.
            _genesisBlock =
                new Block
                (
                    index: 0,
                    previousHash: null,
                    timeStamp: DateTimeOffset.Now,
                    nonce: new NonceGenerator().GenerateNonce(),
                    action:
                    new Action(
                        txNonce: 0,
                        signer: new Address(new PrivateKey().PublicKey),
                        payload: null,
                        signature: null
                    )
                );
            this._hashofTipBlock = _genesisBlock.Hash();
            _blocks.Add(_hashofTipBlock, _genesisBlock);
            InitializeState();
            StoreData(_genesisBlock);
            StoreData(_genesisBlock.GetAction);
            StoreData(GetCurrentState());
        }

        private void InitializeState()
        {
            var board = new string[3, 3];
            for (var row = 0; row < 3; row++)
            {
                for (var calmn = 0; calmn < 3; calmn++)
                {
                    board[row, calmn] = "";
                }
            }
            AddState(0, board);
        }

        private void UpdateDifficulty(Block block)
        {
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
            AddState(block.Index, updatedBoard);
            // Store current data.
            StoreData(block);
            StoreData(block.GetAction);
            StoreData(GetCurrentState());
            // Update Difficulty.
            UpdateDifficulty(block);
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

        public void AddState(long blockIndex, string[,] state)
        {
            _states.Add(blockIndex, state);
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

        private byte[] SerializeState()
        {
            var componentsToSerialize = new Dictionary<int, string>();
            var index = 1;
            foreach(string tuple in GetCurrentState())
            {
                componentsToSerialize.Add(index++, tuple);
            }
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, componentsToSerialize);
            byte[] result = ByteArrayConverter.Compress(mStream.ToArray());
            return result;
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

        public void StoreData(object data)
        {
            StreamWriter streamWriter;

            if (data.GetType().Name == "Block")
            {
                Block block = (Block)data;

                File.WriteAllBytes(
                    _blockStorage + "\\" + String.Join("", block.Hash()) + ".txt", 
                    block.SerializeForStorage()
                );
            }
            else if (data.GetType().Name == "Action")
            {
                Action action = (Action)data;

                File.WriteAllBytes(
                    _actionStorage + "\\" + String.Join("", action.ActionId) + ".txt",
                    action.Serialize()
                );
            }
            else if (data.GetType().Name == "String[,]")
            {
                File.WriteAllBytes(
                    _stateStorage + "\\" + String.Join("", HashofTipBlock) + ".txt",
                    SerializeState()
                );
            }
        }
    }
}
