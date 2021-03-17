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
            BigInteger previousHash,
            BlockChain blockChain
        )
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashDigest;
            Nonce nonce = null;

            do
            {
                // It's a genesis block.
                if (previousBlock == null)
                {
                    hashDigest = 0;
                    break;
                }
                nonce = new NonceGenerator().GenerateNonce();
                byte[] hashInput =
                    // (수정 필요) 이전 블록을 알 수 있어야 함
                    Block.Serialize(
                        previousHash.ToByteArray(),
                        nonce,
                        previousBlock.TimeStamp
                    );
                hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashDigest < blockChain.Difficulty);

            return nonce;
        }
    }
}