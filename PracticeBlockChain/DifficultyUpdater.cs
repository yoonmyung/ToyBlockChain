using System;

namespace PracticeBlockChain
{
    public static class DifficultyUpdater
    {
        public static readonly long _minimumDifficulty = 1024;
        public static readonly TimeSpan _blockInterval = TimeSpan.FromMilliseconds(10000);
        public static readonly long _difficultyBoundDivisor = 128;
        public const long _minimumMultiplier = -99;

        public static long UpdateDifficulty(BlockChain blockChain)
        {
            //EIP-2
            //Libplanet/Blockchain/Policies/BlockPolicy.cs/GetNextBlockDifficulty 함수 참조 
            var previousBlock = blockChain.TipBlock;
            Block prevPreviousBlock = null;

            if (blockChain.TipBlock.BlockHeader.PreviousHash is null)
                var timeDiff =
                    previousBlock.BlockHeader.TimeStamp
                    - prevPreviousBlock.BlockHeader.TimeStamp;
                var timeDiffMilliseconds = (int)timeDiff.TotalMilliseconds;
                var multiplier =
                    1 - (timeDiffMilliseconds / (long)_blockInterval.TotalMilliseconds);
            {
                // It's genesis block.
                return _minimumDifficulty;
            }
            prevPreviousBlock =
                blockChain.GetBlock(blockChain.TipBlock.BlockHeader.PreviousHash);
            var offset = previousBlock.BlockHeader.Difficulty / _minimumDifficulty;
            multiplier = Math.Max(multiplier, _minimumMultiplier);
            long nextDifficulty = 
                Convert.ToInt64
                (
                    previousBlock.BlockHeader.Difficulty + (offset * multiplier)
                );
            nextDifficulty = Math.Max(nextDifficulty, _minimumDifficulty);
            
            return nextDifficulty;
        }
    }
}