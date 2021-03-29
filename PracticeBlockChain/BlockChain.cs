using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace PracticeBlockChain
{
    public class BlockChain
    {
        private byte[] _hashofTipBlock;
        private Block _genesisBlock;
        private long _difficulty;
        private readonly string _blockStorage;
        private readonly string _actionStorage;
        private readonly string _stateStorage;

        public BlockChain()
        {
            Random random = new Random();
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

        private void SetGenesisBlock()
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
            InitializeState();
            File.WriteAllBytes
            (
                Path.Combine(BlockStorage, String.Join("-", _hashofTipBlock) + ".txt"),
                _genesisBlock.SerializeForStorage()
            );
            File.WriteAllBytes
            (
                Path.Combine
                (
                    ActionStorage, 
                    String.Join("-", _genesisBlock.GetAction.ActionId) + ".txt"
                ),
                _genesisBlock.GetAction.SerializeForStorage()
            );
        }

        public void LoadGenesisBlock()
        {
            var directoryInfo = new DirectoryInfo(BlockStorage);
            var numberofStoredGameStates = directoryInfo.GetFiles().Length;
            long indexofTipBlock = 0;

            if (numberofStoredGameStates <= 0)
            {
                SetGenesisBlock();
                return;
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                Block block = LoadBlockFromStorage(file.Name);
                if (block.Index > indexofTipBlock)
                {
                    indexofTipBlock = block.Index;
                }
            }
            _genesisBlock = GetBlock(indexofTipBlock);
            this._hashofTipBlock = _genesisBlock.Hash();
        }

        private Block LoadBlockFromStorage(string file)
        {
            byte[] serializedBlock =
                File.ReadAllBytes(Path.Combine(BlockStorage, file));
            Dictionary<string, object> dataAboutBlock =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedBlock);
            byte[] serializedAction =
                File.ReadAllBytes(
                    Path.Combine
                    (
                        ActionStorage,
                        string.Join("-", (byte[])dataAboutBlock["actionId"]) +
                        ".txt"
                    )
                );
            Dictionary<string, object> dataAboutAction =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedAction);
            TicTacToeGame.Position position = null;
            if (!(dataAboutAction["payload_x"] is null))
            {
                position =
                    new TicTacToeGame.Position
                    (
                        (int)dataAboutAction["payload_x"],
                        (int)dataAboutAction["payload_y"]
                    );
            }
            byte[] previousHash = null;
            if (!(dataAboutBlock["previousHash"] is null))
            {
                previousHash = (byte[])dataAboutBlock["previousHash"];
            }
            Action action =
                new Action
                (
                    txNonce: (long)dataAboutAction["txNonce"],
                    signer: new Address((byte[])dataAboutAction["signer"]),
                    payload: position,
                    signature: (byte[])dataAboutAction["signature"]
                );
            Block block =
                new Block
                (
                    index: (long)dataAboutBlock["index"],
                    previousHash: previousHash,
                    timeStamp: (DateTimeOffset)dataAboutBlock["timeStamp"],
                    nonce: new Nonce((byte[])dataAboutBlock["nonce"]),
                    action: action
                );

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
            File.WriteAllBytes
            (
                Path.Combine(StateStorage, String.Join("-", HashofTipBlock) + ".txt"),
                SerializeState(board)
            );
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
                        previouTimeStamp: GetBlock(block.PreviousHash).TimeStamp,
                        currentTimeStamp: block.TimeStamp
                    );
            }
        }

        public void AddBlock(Block block)
        {
            // Validate block.
            // After validate block, then execute action with adding a block.
            string[,] updatedBoard = 
                block.GetAction.Execute(
                    currentState: GetCurrentState(), 
                    position: block.GetAction.Payload, 
                    address: block.GetAction.Signer
                );
            this._hashofTipBlock = block.Hash();
            // Store current data.
            File.WriteAllBytes
            (
                Path.Combine(BlockStorage, String.Join("-", _hashofTipBlock) + ".txt"),
                block.SerializeForStorage()
            );
            File.WriteAllBytes
            (
                Path.Combine
                (
                    ActionStorage,
                    String.Join("-", block.GetAction.ActionId) + ".txt"
                ),
                block.GetAction.SerializeForStorage()
            );
            File.WriteAllBytes
            (
                Path.Combine(StateStorage, String.Join("-", _hashofTipBlock) + ".txt"),
                SerializeState(updatedBoard)
            );
            // Update Difficulty.
            UpdateDifficulty(block);
        }

        public Block GetBlock(long blockIndex)
        {
            var directoryInfo = new DirectoryInfo(BlockStorage);
            Block block = null;

            foreach (var file in directoryInfo.GetFiles())
            {
                block = LoadBlockFromStorage(file.Name);
                if (block.Index == blockIndex)
                {
                    break;
                }
            }
            return block;
        }

        public Block GetBlock(byte[] hashValue)
        {
            var directoryInfo = new DirectoryInfo(BlockStorage);
            Block block = null;

            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] hashofBlock = 
                    Path.GetFileNameWithoutExtension(file.Name).Split("-")
                    .Select(x => Convert.ToByte(x)).ToArray();
                if (hashofBlock.SequenceEqual(hashValue))
                {
                    block = LoadBlockFromStorage(file.Name);
                    break;
                }
            }

            return block;
        }

        public string[,] GetState(long blockIndex)
        {
            var directoryInfo = new DirectoryInfo(BlockStorage);
            string[,] state = null;

            foreach (var file in directoryInfo.GetFiles())
            {
                Block block = LoadBlockFromStorage(file.Name);
                if (block.Index == blockIndex)
                {
                    byte[] serializedState =
                        File.ReadAllBytes(Path.Combine(StateStorage, file.Name));
                    state = (string[,])ByteArrayConverter.DeSerialize(serializedState);
                    break;
                }
            }

            return state;
        }

        public string[,] GetCurrentState()
        {
            var directoryInfo = new DirectoryInfo(StateStorage);
            string[,] state = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] hashofBlock =
                    Path.GetFileNameWithoutExtension(file.Name).Split("-")
                    .Select(x => Convert.ToByte(x)).ToArray();
                if (hashofBlock.SequenceEqual(HashofTipBlock))
                {
                    byte[] serializedState =
                        File.ReadAllBytes(Path.Combine(StateStorage, file.Name));
                    state = (string[,])ByteArrayConverter.DeSerialize(serializedState);
                    break;
                }
            }

            return state;
        }

        private byte[] SerializeState(string[,] state)
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            binFormatter.Serialize(mStream, state);
            byte[] serializedState = ByteArrayConverter.Compress(mStream.ToArray());

            return serializedState;
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
            var directoryInfo = new DirectoryInfo(BlockStorage);
            Block block = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                block = LoadBlockFromStorage(file.Name);
                yield return block;
            }
        }
    }
}
