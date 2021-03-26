using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace PracticeBlockChain.Test
{
    public class StoringFullGameTest
    {
        public static BlockChain blockChain = new BlockChain();

        [Fact]
        public static void LoadStates()
        {
            var directoryInfo = new DirectoryInfo(blockChain.BlockStorage);
            var countOfBlock =
                Directory.GetFiles
                (
                    blockChain.BlockStorage, "*", SearchOption.AllDirectories
                ).Length;
            SortedDictionary<long, string> states =
                new SortedDictionary<long, string>();

            foreach (var file in directoryInfo.GetFiles())
            {
                byte[] serializedBlock = 
                    File.ReadAllBytes(blockChain.BlockStorage + "\\" + file.Name);
                Dictionary<string, object> block = 
                    (Dictionary<string, object>)
                    ByteArrayConverter.DeSerialize(serializedBlock);
                states.Add((long)block["index"], file.Name);
            }
            PrintStates(states);
        }

        private static void PrintStates(SortedDictionary<long, string> states)
        {
            foreach (long index in states.Keys)
            {
                byte[] serializedState =
                    File.ReadAllBytes(blockChain.StateStorage + "\\" + states[index]);
                Dictionary<int, string> state = 
                    (Dictionary<int, string>)
                    ByteArrayConverter.DeSerialize(serializedState);
                Debug.WriteLine("---------------------------");
                foreach (int tuple in state.Keys)
                {
                    Debug.Write
                    (
                        "   " +
                        (state[tuple].Length == 0 ? "   " : state[tuple]) +
                        "   "
                    );
                    if (tuple % 3 == 0)
                    {
                        Debug.WriteLine("\n---------------------------\n");
                    }
                }
            }
        }
    }
}
