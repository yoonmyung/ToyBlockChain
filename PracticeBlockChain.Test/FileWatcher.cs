using PracticeBlockChain.Cryptography;
using System;
using System.IO;

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
            GC.KeepAlive(directoryWatcher);
            directoryWatcher.EnableRaisingEvents = true;
        }

        private static void WatcherOnChanged
        (object sender, FileSystemEventArgs e, BlockChain blockChain, Address address)
        {
            TictactoeGameTest.PrintCurrentState(blockChain, address);
        }
    }
}
