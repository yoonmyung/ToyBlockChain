using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private readonly Dictionary<byte[], Block> _blocks;
        private readonly Dictionary<long, string[,]> _states;
        private byte[] _hashofTipBlock;
        private readonly Block _genesisBlock;
        private long _difficulty;
        private readonly string _blockStorage = 
            "C:\\Users\\1229k\\Desktop\\Planetarium\\BlockChain\\_BlockStorage";
        private readonly string _actionStorage =
            "C:\\Users\\1229k\\Desktop\\Planetarium\\BlockChain\\_ActionStorage";
        private readonly string _stateStorage =
            "C:\\Users\\1229k\\Desktop\\Planetarium\\BlockChain\\_StateStorage";

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

        public string BlockStorage
        {
            get
            {
                return this._blockStorage;
            }
        }

        public string ActionStorage
        {
            get
            {
                return this._actionStorage;
            }
        }

        public string StateStorage
        {
            get
            {
                return this._stateStorage;
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
                    action:
                    new Action(
                        txNonce: 0,
                        signer: new Address(new PrivateKey().PublicKey),
                        payload: null,
                        signature: null
                    )
                );
            this._hashofTipBlock = block.Hash();
            _blocks.Add(_hashofTipBlock, block);
            InitializeState();
            StoreData(block);
            StoreData(block.GetAction);
            StoreData(GetCurrentState());
            return block;
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
            byte[] result = Compress(mStream.ToArray());
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

        public Object DeSerialize(byte[] arrBytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                var decompressed = Decompress(arrBytes);

                memoryStream.Write(decompressed, 0, decompressed.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        public static byte[] Compress(byte[] input)
        {
            byte[] compressesData;

            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zip.Write(input, 0, input.Length);
                }

                compressesData = outputStream.ToArray();
            }

            return compressesData;
        }

        public static byte[] Decompress(byte[] input)
        {
            byte[] decompressedData;

            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(input))
                {
                    using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outputStream);
                    }
                }

                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }
    }
}
