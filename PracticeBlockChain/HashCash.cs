using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace PracticeBlockChain
{
    public class HashCash
    {
        public byte[] CalculateHash(Block previousBlock, byte[] previousHash)
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashValue;

            do
            {
                if (previousBlock == null)   // It's a genesis block.
                {
                    hashValue = 0;
                    break;
                }
                byte[] hashInput = 
                    Serialization.SerializeHashInput(
                        previousHash, 
                        new NonceGenerator().GenerateNonce(), 
                        previousBlock.TimeStamp
                    );
                hashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashValue < previousBlock.Difficulty);

            // In here, need to implement a validation.

            return hashValue.ToByteArray();
        }
    }
}