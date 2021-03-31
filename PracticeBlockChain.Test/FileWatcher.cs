using System;
using System.IO;

namespace PracticeBlockChain.Test
{
    public static class FileWatcher
    {
        public static void RunWatcher(BlockChain blockChain)
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
            var handler = new FileSystemEventHandler((s, e) => WatcherOnChanged(s, e, blockChain));
            directoryWatcher.Created += handler;
            directoryWatcher.EnableRaisingEvents = true;
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e, BlockChain blockChain)
        {
            Console.WriteLine();
            TictactoeGameTest.PrintCurrentState(blockChain);
        }
    }
}
