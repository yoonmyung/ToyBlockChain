using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public static class HashCash
    {
        public static (Nonce nonce, long difficulty) CalculateBlockHash
        (
            BlockChain blockChain
        )
        {
            var hashAlgorithm = SHA256.Create();
            var difficulty = DifficultyUpdater.UpdateDifficulty(blockChain);
            BigInteger result;
            BigInteger target;
            Nonce nonce = null;

            do
            {
                nonce = Nonce.GenerateNonce();
                byte[] hashInput =
                    blockChain.TipBlock.BlockHeader.Serialize()
                    .Concat(nonce.NonceValue)
                    .ToArray();
                byte[] hash = hashAlgorithm.ComputeHash(hashInput);

                var maxTargetBytes = new byte[hash.Length + 1];
                maxTargetBytes[hash.Length] = 0x01;
                var maxTarget = new BigInteger(maxTargetBytes);
                target = maxTarget / difficulty;

                var hashInputBytes = new byte[hash.Length + 1];
                Buffer.BlockCopy(hash, 0, hashInputBytes, 0, hash.Length);
                Buffer.BlockCopy(new byte[] { 0 }, 0, hashInputBytes, hash.Length, 1);
                result = new BigInteger(hashInputBytes);
            } 
            while (result > target);

            return (nonce, difficulty);
        }
    }
}