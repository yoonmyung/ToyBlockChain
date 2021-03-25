using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace PracticeBlockChain.Test
{
    public static class StorageTest
    {
        [Fact]
        public static void StoringBlockTest()
        {
            BlockChain blockChain = new BlockChain();
            var indexTofindBlock = 3;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block
                    = (Dictionary<string, object>)blockChain.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    Assert.Equal(block["index"], (long)3);
                    Assert.Equal
                    (
                        block["previousHash"],
                        new byte[]
                        {
                            35, 0, 144, 168, 40, 203, 208, 148, 218,
                            234, 232, 205, 114, 134, 147, 187, 36, 54,
                            79, 188, 121, 222, 106, 166, 144, 221, 120, 188, 52, 134, 175, 61
                        }
                    );
                    Assert.Equal
                    (
                        block["nonce"],
                        new byte[] { 38, 241, 76, 108, 183, 65, 66, 68, 188, 184 }
                    );
                    Assert.Equal
                    (
                        block["timeStamp"],
                        new DateTimeOffset
                        (
                            new DateTime(2021, 03, 25, 11, 39, 16) + TimeSpan.FromSeconds(0.0205552)
                        )
                    );
                    break;
                }
            }
        }

        [Fact]
        public static void StoringActionTest()
        {
            BlockChain blockChain = new BlockChain();
            var indexTofindBlock = 3;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            object idTofindAction = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block
                    = (Dictionary<string, object>)blockChain.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    idTofindAction = block["actionId"];
                    Assert.Equal
                    (
                        idTofindAction,
                        new byte[]
                        {
                            125, 66, 96, 37, 71, 109, 113, 237, 53, 27,
                            244, 149, 204, 72, 183, 243, 113, 220, 107,
                            225, 242, 154, 114, 91, 79, 101, 100, 58, 126, 87, 42, 123
                        }
                    );
                    break;
                }
            }
            byte[] actionId = (byte[])idTofindAction;
            byte[] serializedAction =
                File.ReadAllBytes(blockChain.ActionStorage + "\\" + string.Join("", actionId) + ".txt");
            Dictionary<string, object> action
                = (Dictionary<string, object>)blockChain.DeSerialize(serializedAction);
            Assert.Equal(action["txNonce"], (long)2);
            Assert.Equal
            (
                action["signer"],
                new byte[]
                {
                    92, 14, 124, 217, 39, 104, 85, 160, 35,
                    214, 106, 90, 85, 253, 86, 50, 13, 37, 197, 211
                }
            );
            Assert.Equal(action["payload_x"], 0);
            Assert.Equal(action["payload_y"], 0);
        }

        [Fact]
        public static void StoringStateTest()
        {
            BlockChain blockChain = new BlockChain();
            var indexTofindBlock = 3;
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            var countOfBlock = 
                Directory.GetFiles(blockChain.BlockStorage, "*", SearchOption.AllDirectories).Length;
            object hashTofindState = null;
            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block
                    = (Dictionary<string, object>)blockChain.DeSerialize(serializedBlock);
                if ((long)block["index"] == indexTofindBlock)
                {
                    hashTofindState = file.Name;
                    break;
                }
            }
            byte[] serializedState = 
                File.ReadAllBytes(blockChain.StateStorage + "\\" + hashTofindState);
            Dictionary<int, string> state
                = (Dictionary<int, string>)blockChain.DeSerialize(serializedState);
            Assert.Equal(state[1], "Kim");
            Assert.Equal(state[2], "");
            Assert.Equal(state[3], "");
            Assert.Equal(state[4], "");
            Assert.Equal(state[5], "Kim");
            Assert.Equal(state[6], "");
            Assert.Equal(state[7], "");
            Assert.Equal(state[8], "Lee");
            Assert.Equal(state[9], "");
        }
    }
}
