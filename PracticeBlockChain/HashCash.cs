using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public static class HashCash
    {
        public static KeyValuePair<Nonce, byte[]> CalculateHash(
            Block previousBlock, 
            byte[] previousHash,
            BlockChain blockChain
        )
        {
            SHA256 hashAlgo = SHA256.Create();
            HashDigest hashDigest = new HashDigest();
            Nonce nonce = null;

            do
            {
                if (previousBlock == null)   // It's a genesis block.
                {
                    hashDigest.HashValue = 0;
                    break;
                }
                nonce = new NonceGenerator().GenerateNonce();
                byte[] hashInput =
                    Serialization.SerializeforBlock(
                        previousHash, 
                        nonce, 
                        previousBlock.TimeStamp
                    );
                hashDigest.HashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashDigest.HashValue < blockChain.Difficulty);

            return new KeyValuePair<Nonce, byte[]> 
                (nonce, hashDigest.HashValue.ToByteArray());
        }
    }
}