using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    public static class DifficultyUpdater
    {
        public static readonly long _minimumDifficulty = 1024;
        public static readonly long _difficultyBoundDivisor = 128;

        public static long UpdateDifficulty(
            long difficulty,
            DateTimeOffset previouTimeStamp,
            DateTimeOffset currentTimeStamp
        )
        {
            //EIP-2
            //Libplanet/Blockchain/Policies/BlockPolicy.cs 참조 
            var prevDifficulty = difficulty;
            TimeSpan timeInterval = currentTimeStamp - previouTimeStamp;
            const long minimumMultiplier = -99;
            var multiplier =
                1 - (timeInterval.TotalMilliseconds / _difficultyBoundDivisor);
            multiplier = Math.Max(multiplier, minimumMultiplier);

            var offset = prevDifficulty / _minimumDifficulty;
            long nextDifficulty = 
                Convert.ToInt64(prevDifficulty + (offset * multiplier));
            nextDifficulty = Math.Max(nextDifficulty, _minimumDifficulty);
            return nextDifficulty;
        }
    }
}