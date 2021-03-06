using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace practiceBlockchain
{
    public class BlockChain
    {
        static readonly Dictionary<byte[], Block> blocks 
            = new Dictionary<byte[], Block>(new ByteArrayComparer());
        private static long difficulty = 0;
        private readonly static long minimumDifficulty = 1024;
        private readonly static long difficultyBoundDivisor = 128;

        //minimumMultiplier, minimumDifficulty는 난이도 조절(difficulty 값 조절)을 위해

        public static Block MakeBlock(
            byte[] previousHash, 
            byte[] hashValue, 
            DateTimeOffset timeStamp, 
            int nonce
        )
        {
            blocks.Add(hashValue, new Block(previousHash, timeStamp, nonce));

            return GetBlock(hashValue);
        }

        public static int Difficulty
        {
            get;
        }

        public static long UpdateDifficulty (
            DateTimeOffset previouTimeStamp, 
            DateTimeOffset currentTimeStamp
        )
        {
            //EIP-2
            //Libplanet/Blockchain/Policies/BlockPolicy.cs 참조 
            long prevDifficulty = difficulty;
            TimeSpan timeInterval = currentTimeStamp - previouTimeStamp;
            const long minimumMultiplier = -99;
            double multiplier = 
                1 - timeInterval.TotalMilliseconds / difficultyBoundDivisor;
            multiplier = Math.Max(multiplier, minimumMultiplier);

            var offset = prevDifficulty / minimumDifficulty;
            long nextDifficulty = Convert.ToInt64(prevDifficulty + (offset * multiplier));
            difficulty = Math.Max(nextDifficulty, minimumDifficulty);

            return difficulty;
        }

        public static Block GetBlock(byte[] hashValue)
        {
            Block returnedBlock = null;

            try
            {
                returnedBlock = blocks[hashValue];
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return returnedBlock;
        }

        public static byte[] GetHash(Block blockToGetHash)
        {
            var returnedHash = 
                blocks.FirstOrDefault(block => block.Value == blockToGetHash).Key;

            return returnedHash;
        }

        public static IEnumerable<byte[]> IterateBlock()
        {
            foreach(byte[] hashofBlock in blocks.Keys)
            {
                yield return hashofBlock;
            }
        }
    }
}
