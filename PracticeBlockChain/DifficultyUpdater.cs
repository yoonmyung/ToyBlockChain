using System;

namespace PracticeBlockChain
{
    public static class DifficultyUpdater
    {
        public static readonly long _minimumDifficulty = 1024;
        public static readonly long _difficultyBoundDivisor = 128;
        public const long _minimumMultiplier = -99;

        public static long UpdateDifficulty(BlockChain blockChain)
        {
            //EIP-2
            //Libplanet/Blockchain/Policies/BlockPolicy.cs/GetNextBlockDifficulty 함수 참조 
            var previousBlock = blockChain.TipBlock;
            Block prevPreviousBlock = null;
            try
            {
                prevPreviousBlock = blockChain.GetBlock(blockChain.TipBlock.PreviousHash);
            }
            catch (Exception e)
            {
                return _minimumDifficulty;
            }
            TimeSpan timeInterval = previousBlock.TimeStamp - prevPreviousBlock.TimeStamp;
            var multiplier =
                1 - (timeInterval.TotalMilliseconds / _difficultyBoundDivisor);
            var offset = previousBlock.Difficulty / _minimumDifficulty;
            multiplier = Math.Max(multiplier, _minimumMultiplier);
            long nextDifficulty = 
                Convert.ToInt64(previousBlock.Difficulty + (offset * multiplier));
            nextDifficulty = Math.Max(nextDifficulty, _minimumDifficulty);
            
            return nextDifficulty;
        }
    }
}