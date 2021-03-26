using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace PracticeBlockChain.Test
{
    public static class StorageTest
    {
        public static BlockChain blockChain = new BlockChain();

        [Fact]
        public static void StoringBlockTest()
        {
            var indexTofindBlock = 0;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = 
                    File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block = 
                    (Dictionary<string, object>)
                    ByteArrayConverter.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    Assert.Equal(block["index"], (long)0);
                    Assert.Equal
                    (
                        block["previousHash"],
                        null
                    );
                    Assert.Equal
                    (
                        block["nonce"],
                        new byte[] { 179, 34, 184, 195, 60, 46, 201, 167, 171, 237 }
                    );
                    Assert.Equal
                    (
                        block["timeStamp"],
                        new DateTimeOffset
                        (
                            new DateTime(2021, 03, 25, 17, 38, 1) 
                            + TimeSpan.FromSeconds(0.1832255)
                        )
                    );
                    break;
                }
            }
        }

        [Fact]
        public static void StoringActionTest()
        {
            var indexTofindBlock = 0;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            object idTofindAction = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = 
                    File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block =
                    (Dictionary<string, object>)
                    ByteArrayConverter.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    idTofindAction = block["actionId"];
                    Assert.Equal
                    (
                        idTofindAction,
                        new byte[]
                        {
                            57, 79, 79, 66, 27, 212, 39, 150,
                            38, 84, 40, 209, 232, 217, 192, 129,
                            141, 132, 33, 195, 168, 191, 249, 46,
                            52, 123, 84, 195, 133, 124, 195, 7
                        }
                    );
                    break;
                }
            }
            byte[] actionId = (byte[])idTofindAction;
            byte[] serializedAction =
                File.ReadAllBytes
                (
                    blockChain.ActionStorage + "\\" + string.Join("", actionId) + ".txt"
                );
            Dictionary<string, object> action =
                (Dictionary<string, object>)
                ByteArrayConverter.DeSerialize(serializedAction);
            Assert.Equal(action["txNonce"], (long)0);
            Assert.Equal
            (
                action["signer"],
                new byte[]
                {
                    116, 51, 244, 149, 150, 239, 184, 168,
                    60, 135, 8, 52, 29, 131, 123, 76,
                    245, 132, 86, 37
                }
            );
            Assert.Equal(action["payload_x"], null);
            Assert.Equal(action["payload_y"], null);
        }

        [Fact]
        public static void StoringStateTest()
        {
            var indexTofindBlock = 0;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            var countOfBlock = 
                Directory.GetFiles
                (
                    blockChain.BlockStorage, "*", SearchOption.AllDirectories
                ).Length;
            object hashTofindState = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock 
                    = File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block = 
                    (Dictionary<string, object>)
                    ByteArrayConverter.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    hashTofindState = file.Name;
                    break;
                }
            }
            byte[] serializedState = 
                File.ReadAllBytes(blockChain.StateStorage + "\\" + hashTofindState);
            Dictionary<int, string> state = 
                (Dictionary<int, string>)
                ByteArrayConverter.DeSerialize(serializedState);
            Assert.Equal(state[1], "");
            Assert.Equal(state[2], "");
            Assert.Equal(state[3], "");
            Assert.Equal(state[4], "");
            Assert.Equal(state[5], "");
            Assert.Equal(state[6], "");
            Assert.Equal(state[7], "");
            Assert.Equal(state[8], "");
            Assert.Equal(state[9], "");
        }
    }
}
