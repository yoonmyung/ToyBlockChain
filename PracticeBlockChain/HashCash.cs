using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public static class HashCash
    {
        public static Nonce CalculateHash(Block previousBlock, BlockChain blockChain)
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashDigest;
            Nonce nonce = null;

            do
            {
                nonce = new NonceGenerator().GenerateNonce();
                byte[] hashInput =
                    previousBlock.Serialize()
                    .Concat(nonce.NonceValue)
                    .ToArray();
                hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashDigest < blockChain.Difficulty);

            return nonce;
        }
    }
}