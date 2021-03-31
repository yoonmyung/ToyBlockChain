using PracticeBlockChain.Cryptography;
using PracticeBlockChain.TicTacToeGame;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace PracticeBlockChain.Test
{
    public static class FileWatcher
    {
        public static void RunWatcher(BlockChain blockChain, Address address)
        {
            var directoryWatcher = new FileSystemWatcher();

            directoryWatcher.Filter = "*.*";
            directoryWatcher.Path = blockChain.StateStorage;
            directoryWatcher.IncludeSubdirectories = true;
            directoryWatcher.NotifyFilter = 
                NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Security
                | NotifyFilters.Size;
            var handler = 
                new FileSystemEventHandler((s, e) => WatcherOnChanged(s, e, blockChain, address));
            directoryWatcher.Created += handler;
            directoryWatcher.EnableRaisingEvents = true;
        }

        private static void WatcherOnChanged
        (object sender, FileSystemEventArgs e, BlockChain blockChain, Address address)
        {
            if 
            (
                blockChain.TipBlock.GetAction.Signer.AddressValue.SequenceEqual
                (
                    blockChain.GenesisBlock.GetAction.Signer.AddressValue
                )
            )
            {
                // Genesis block 상태에서는 항상 Lee가 먼저 두는 상태
                if (AddressPlayerMappingAttribute.GetPlayer(address) == "Kim")
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            else if
            (
                address.AddressValue.SequenceEqual(blockChain.TipBlock.GetAction.Signer.AddressValue)
            )
            {
                // Tip이 현재 생성된 블록으로 갱신되기 전의 블록(이전 턴에 말을 둔 플레이어)
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(100);
            }
            Console.WriteLine();
            TictactoeGameTest.PrintCurrentState(blockChain);
        }
    }
}
