using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public static class HashCash
    {
        public static Nonce CalculateHash(BlockChain blockChain)
        {
            var hashAlgo = SHA256.Create();
            var difficulty = DifficultyUpdater.UpdateDifficulty(blockChain);
            BigInteger hashDigest;
            Nonce nonce = null;

            do
            {
                nonce = new NonceGenerator().GenerateNonce();
                byte[] hashInput =
                    blockChain.TipBlock.Serialize()
                    .Concat(nonce.NonceValue)
                    .ToArray();
                hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } 
            while (hashDigest < difficulty);

            return nonce;
        }
    }
}