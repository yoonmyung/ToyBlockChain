using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public static class HashCash
    {
        public static Nonce CalculateHash(
            Block previousBlock, 
            HashDigest previousHash,
            BlockChain blockChain
        )
        {
            SHA256 hashAlgo = SHA256.Create();
            HashDigest hashDigest = new HashDigest();
            Nonce nonce = null;

            do
            {
                // It's a genesis block.
                if (previousBlock == null)
                {
                    hashDigest.HashValue = 0;
                    break;
                }
                nonce = new NonceGenerator().GenerateNonce();
                byte[] hashInput =
                    Block.Serialize(
                        previousHash.ToByteArray(),
                        nonce,
                        previousBlock.TimeStamp
                    );
                hashDigest.HashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashDigest.HashValue < blockChain.Difficulty);

            return nonce;
        }
    }
}