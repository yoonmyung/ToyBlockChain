using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    class DifficultyUpdater
    {
        private readonly long minimumDifficulty = 1024;
        private readonly long difficultyBoundDivisor = 128;

        public long UpdateDifficulty(
            Block prevBlock,
            DateTimeOffset previouTimeStamp,
            DateTimeOffset currentTimeStamp
        )
        {
            //EIP-2
            //Libplanet/Blockchain/Policies/BlockPolicy.cs 참조 
            long prevDifficulty = prevBlock.Difficulty;
            TimeSpan timeInterval = currentTimeStamp - previouTimeStamp;
            const long minimumMultiplier = -99;
            double multiplier =
                1 - timeInterval.TotalMilliseconds / difficultyBoundDivisor;
            multiplier = Math.Max(multiplier, minimumMultiplier);

            var offset = prevDifficulty / minimumDifficulty;
            long nextDifficulty = Convert.ToInt64(prevDifficulty + (offset * multiplier));
            nextDifficulty = Math.Max(nextDifficulty, minimumDifficulty);

            return nextDifficulty;
        }
    }
}
