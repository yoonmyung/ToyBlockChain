using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace practiceBlockchain
{
    public class BlockChain
    {
        static readonly Dictionary<byte[], Block> blocks 
            = new Dictionary<byte[], Block>(new ByteArrayComparer());
        private static long difficulty = 0;
        private static long minimumDifficulty = 1024;

        //minimumMultiplier, minimumDifficulty는 난이도 조절(difficulty 값 조절)을 위해

        private static long difficultyBoundDivisor = 128;

        public static byte[] makeBlock(
            byte[] previousHash, 
            byte[] hashValue, 
            DateTimeOffset timeStamp, 
            int nonce
        )
        {
            if (previousHash is null)   //genesis block
            {
                previousHash = new byte[] { };
                blocks.Add(hashValue, new Block(previousHash, timeStamp, nonce));
            }
            else
            {
                blocks.Add(hashValue, new Block(previousHash, timeStamp, nonce));
                updateDifficulty(blocks[previousHash].TimeStamp, timeStamp);
            }

            return hashValue;
        }

        public static int Difficulty
        {
            get;
        }

        public static void updateDifficulty (
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

            Console.WriteLine($"difficulty changed: {prevDifficulty} -> {difficulty}");
        }

        public static Block getBlock(byte[] hashValue)
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

        public static IEnumerable<byte[]> iterateBlock()
        {
            foreach(byte[] hashofBlock in blocks.Keys)
            {
                yield return hashofBlock;
            }
        }
    }
}
