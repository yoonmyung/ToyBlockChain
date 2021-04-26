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
        private readonly string _genesisBlockPath = "C:\\Users\\1229k\\Documents\\BlockChain\\_Genesisblock";

        public BlockChain(string playerStorage)
        {
            BlockStorage = 
                Directory.CreateDirectory
                (
                    Path.Combine(playerStorage, "_BlockStorage")
                ).FullName;
            ActionStorage = 
                Directory.CreateDirectory
                (
                    Path.Combine(playerStorage, "_ActionStorage")
                ).FullName;
            StateStorage = 
                Directory.CreateDirectory
                (
                    Path.Combine(playerStorage, "_StateStorage")
                ).FullName;
            LoadTipBlock();
        }

        public Block GenesisBlock
        {
            get;
            private set;
        }

        public Block TipBlock
        {
            get;
            private set;
        }

        public string BlockStorage
        {
            get; private set;
        }

        public string ActionStorage
        {
            get; private set;
        }

        public string StateStorage
        {
            get; private set;
        }

        public void LoadTipBlock()
        {
            var files = LoadFilesFromStorage(BlockStorage);
            long indexofTipBlock = 0;

            if (!(files.Any()))
            {
                // Make genesis block.
                files = LoadFilesFromStorage(_genesisBlockPath);
                foreach(var genesisBlock in files)
                {
                    GenesisBlock = LoadBlock(genesisBlock.Name, _genesisBlockPath);
                    File.WriteAllBytes
                    (
                        Path.Combine
                        (
                            BlockStorage,
                            String.Join("-", GenesisBlock.BlockHeader.Hash()) + ".txt"
                        ),
                        GenesisBlock.Serialize()
                    );
                }
            }
            else
            {
                foreach (var file in files)
                {
                    var block = LoadBlock(file.Name, BlockStorage);

                    if
                    (
                        indexofTipBlock == 0 &&
                        block.BlockHeader.Index.Equals(indexofTipBlock)
                    )
                    {
                        GenesisBlock = block;
                    }
                    else if (block.BlockHeader.Index > indexofTipBlock)
                    {
                        indexofTipBlock = block.BlockHeader.Index;
                    }
                }
            }
            TipBlock = GetBlock(indexofTipBlock);
        }

        private Block LoadBlock(string file, string storage)
        {
            byte[] serializedBlock = LoadFileFromStorage(storage, file);
            var componentsofBlock =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedBlock);
            byte[] serializedAction =
                LoadFileFromStorage(
                    ActionStorage,
                    string.Join("-", (byte[])componentsofBlock["actionId"]) + ".txt"
                );
            var componentsofAction =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedAction);
            TicTacToeGame.Position position = 
                componentsofAction["payload_x"] is null ? 
                null :
                new TicTacToeGame.Position
                (
                    (int)componentsofAction["payload_x"],
                    (int)componentsofAction["payload_y"]
                );
            byte[] previousHash =
                componentsofBlock["previousHash"] is null ?
                null :
                (byte[])componentsofBlock["previousHash"];
            var action =
                new Action
                (
                    txNonce: (long)componentsofAction["txNonce"],
                    signer: new Address((byte[])componentsofAction["signer"]),
                    payload: position,
                    signature: (byte[])componentsofAction["signature"]
                );
            var blockHeader =
                new BlockHeader
                (
                    index: (long)componentsofBlock["index"],
                    previousHash: previousHash,
                    timeStamp: (DateTimeOffset)componentsofBlock["timeStamp"],
                    nonce: new Nonce((byte[])componentsofBlock["nonce"]),
                    difficulty: (long)componentsofBlock["difficulty"]
                );
            var block = new Block(blockHeader: blockHeader, action: null);

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
                Path.Combine
                (
                    StateStorage,
                    String.Join("-", GenesisBlock.BlockHeader.Hash()) + ".txt"
                ),
                SerializeState(board)
            );
        }

        private void UpdateTip()
        {
            long tipIndex = 0;
            Block block = null;

            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                block = LoadBlock(file.Name, BlockStorage);
                if (block.BlockHeader.Index > tipIndex)
                {
                    tipIndex = block.BlockHeader.Index;
                }
            }
            TipBlock = GetBlock(tipIndex);
        }

        public bool AddBlock(Block block)
        {
            // Validate block.
            // After validate block, then execute action with adding a block.
            string[,] updatedBoard =
                block.Action.Execute
                (
                    blockChain: this,
                    position: block.Action.Payload,
                    address: block.Action.Signer
                );
            if (isNotStateChange(GetCurrentState(), updatedBoard))
            {
                return false;
            }
            // Store current data.
            File.WriteAllBytes
            (
                Path.Combine
                (
                    BlockStorage,
                    String.Join("-", block.BlockHeader.Hash()) + ".txt"
                ),
                block.Serialize()
            );
            if (!(block.Action is null))
            {
                File.WriteAllBytes
                (
                    Path.Combine
                    (
                        ActionStorage,
                        String.Join("-", block.Action.ActionId) + ".txt"
                    ),
                    block.Action.SerializeForStorage()
                );
            }
            RefreshStorage();
            UpdateTip();
            File.WriteAllBytes
            (
                Path.Combine
                (
                    StateStorage,
                    String.Join("-", block.BlockHeader.Hash()) + ".txt"
                ),
                SerializeState(updatedBoard)
            );
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
                block = LoadBlock(file.Name, BlockStorage);
                if (block.BlockHeader.Index == blockIndex)
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
                if (hashValue is null)
                {
                    break;
                }
                else if (hashofBlock.SequenceEqual(hashValue))
                {
                    block = LoadBlock(file.Name, BlockStorage);
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
                Block block = LoadBlock(file.Name, BlockStorage);
                if (block.BlockHeader.Index == blockIndex)
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
            string[,] state = null;

            RefreshStorage();
            UpdateTip();
            foreach (var file in LoadFilesFromStorage(BlockStorage))
            {
                byte[] hashofBlock =
                    Path.GetFileNameWithoutExtension(file.Name).Split("-")
                    .Select(x => Convert.ToByte(x)).ToArray();
                if (hashofBlock.SequenceEqual(TipBlock.BlockHeader.Hash()))
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
                    var directoryInfo = new DirectoryInfo(storage);

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

            foreach (Block block in IterateBlock())
            {
                if (block.Action is null)
                {
                    continue;
                }
                Address address = block.Action.Signer;
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
                block = LoadBlock(file.Name, BlockStorage);
                yield return block;
            }
        }
    }
}