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
        private Block _tipBlock;
        private Block _genesisBlock;
        private long _difficulty;
        private readonly string _blockStorage;
        private readonly string _actionStorage;
        private readonly string _stateStorage;
        private readonly string _privateKeyStorage;

        public BlockChain()
        {
            Random random = new Random();
            _difficulty = random.Next(10000);
        }

        public long Difficulty
        {
            get 
            {
                return _difficulty;
            }
            set
            {
                _difficulty = value;
            }
        }

        public Block GenesisBlock 
        {
            get
            {
                return _genesisBlock;
            }
        }

        public Block TipBlock
        {
            get
            {
                return _tipBlock;
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

        public string PrivateKeyStorage
        {
            get
            {
                return _privateKeyStorage;
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
            _tipBlock = GenesisBlock;
            File.WriteAllBytes
            (
                Path.Combine(BlockStorage, String.Join("-", GenesisBlock.Hash()) + ".txt"),
                GenesisBlock.SerializeForStorage()
            );
            File.WriteAllBytes
            (
                Path.Combine
                (
                    ActionStorage, 
                    String.Join("-", GenesisBlock.GetAction.ActionId) + ".txt"
                ),
                GenesisBlock.GetAction.SerializeForStorage()
            );
            InitializeState();
        }

        public void LoadTipBlock()
        {
            var files = LoadFilesFromStorage(BlockStorage);
            var numberofStoredGameStates = files.Length;
            long indexofTipBlock = 0;

            if (numberofStoredGameStates <= 0)
            {
                SetGenesisBlock();
                return;
            }
            foreach (var file in files)
            {
                Block block = LoadBlockFromFile(file.Name);
                if (block.Index == 0)
                {
                    _genesisBlock = block;
                }
                else if (block.Index > indexofTipBlock)
                {
                    indexofTipBlock = block.Index;
                }
            }
            _tipBlock = GetBlock(indexofTipBlock);
        }

        private Block LoadBlockFromFile(string file)
        {
            byte[] serializedBlock = LoadFileFromStorage(BlockStorage, file);
            Dictionary<string, object> dataAboutBlock =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedBlock);

            byte[] serializedAction = 
                LoadFileFromStorage
                (
                    ActionStorage, 
                    string.Join("-", (byte[])dataAboutBlock["actionId"]) + ".txt"
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
                Path.Combine(StateStorage, String.Join("-", GenesisBlock.Hash()) + ".txt"),
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
                        previouTimeStamp: GenesisBlock.TimeStamp,
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

        private void UpdateTip(Address address)
        {
            long tipIndex = 0;
            Block block = null;

            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                block = LoadBlockFromFile(file.Name);
                if (block.Index > tipIndex)
                {
                    tipIndex = block.Index;
                }
            }
            _tipBlock = GetBlock(tipIndex);
        }

        public bool AddBlock(Block block)
        {
            // Validate block.
            // After validate block, then execute action with adding a block.
            string[,] updatedBoard = 
                block.GetAction.Execute
                (
                    blockChain: this, 
                    position: block.GetAction.Payload, 
                    address: block.GetAction.Signer
                );
            if (isNotStateChange(GetCurrentState(null), updatedBoard))
            {
                return false;
            }
            // Store current data.
            File.WriteAllBytes
            (
                Path.Combine(BlockStorage, String.Join("-", block.Hash()) + ".txt"),
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
            RefreshStorage();
            UpdateTip(block.GetAction.Signer);
            File.WriteAllBytes
            (
                Path.Combine(StateStorage, String.Join("-", block.Hash()) + ".txt"),
                SerializeState(updatedBoard)
            );
            UpdateDifficulty(block);

            return true;
        }

        private void RefreshStorage()
        {
            var blockDirectory = new DirectoryInfo(BlockStorage);
            var actionDirectory = new DirectoryInfo(ActionStorage);
            var stateDirectory = new DirectoryInfo(StateStorage);

            blockDirectory.Refresh();
            actionDirectory.Refresh();
            stateDirectory.Refresh();
        }

        private bool isNotStateChange(string[,] currentBoard, string[,] updatedBoard)
        {
            var isNotChange = 
                currentBoard.Rank == updatedBoard.Rank &&
                Enumerable.Range(0, currentBoard.Rank).All(dimension => currentBoard.GetLength(dimension) 
                == updatedBoard.GetLength(dimension)) &&
                currentBoard.Cast<string>().SequenceEqual(updatedBoard.Cast<string>());

            return isNotChange;
        }

        public Block GetBlock(long blockIndex)
        {
            Block block = null;

            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                block = LoadBlockFromFile(file.Name);
                if (block.Index == blockIndex)
                {
                    break;
                }
            }

            return block;
        }

        public Block GetBlock(byte[] hashValue)
        {
            Block block = null;

            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                byte[] hashofBlock = 
                    Path.GetFileNameWithoutExtension(file.Name).Split("-")
                    .Select(x => Convert.ToByte(x)).ToArray();
                if (hashofBlock.SequenceEqual(hashValue))
                {
                    block = LoadBlockFromFile(file.Name);
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
                Block block = LoadBlockFromFile(file.Name);
                if (block.Index == blockIndex)
                {
                    byte[] serializedState = LoadFileFromStorage(StateStorage, file.Name);
                    state = (string[,])ByteArrayConverter.DeSerialize(serializedState);
                    break;
                }
            }

            return state;
        }

        public string[,] GetCurrentState(Address address)
        {
            string[,] state = null;

            RefreshStorage();
            UpdateTip(address);
            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                byte[] hashofBlock =
                    Path.GetFileNameWithoutExtension(file.Name).Split("-")
                    .Select(x => Convert.ToByte(x)).ToArray();
                if (hashofBlock.SequenceEqual(TipBlock.Hash()))
                {
                    byte[] serializedState = LoadFileFromStorage(StateStorage, file.Name);
                    state = (string[,])ByteArrayConverter.DeSerialize(serializedState);
                    break;
                }
            }

            return state;
        }

        private byte[] LoadFileFromStorage(string storage, string fileName)
        {
            while (true)
            {
                try
                {
                    return File.ReadAllBytes(Path.Combine(storage, fileName));
                }
                catch (IOException e)
                {
                    continue;
                }
            }
        }

        private FileInfo[] LoadFilesFromStorage(string storage)
        {
            while (true)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(storage);
                    return directoryInfo.GetFiles();
                }
                catch (IOException e)
                {
                    continue;
                }
            }
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
                if (address.AddressValue.SequenceEqual(minerAddress.AddressValue))
                {
                    count++;
                }
            }

            return count;
        }

        public IEnumerable<Block> IterateBlock()
        {
            Block block = null;

            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                block = LoadBlockFromFile(file.Name);
                yield return block;
            }
        }
    }
}
