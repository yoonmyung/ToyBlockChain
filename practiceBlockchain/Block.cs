using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace practiceBlockchain
{
    public class Block
    {
        readonly static byte[] previousHash;
        static readonly DateTimeOffset timeStamp;
        static int nonce;

        public Block(byte[] previousHash, DateTimeOffset timeStamp, int nonce) 
        {
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
        }

        public static byte[] CalculateHash(byte[] previousHash)
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashValue;
            long difficulty = BlockChain.Difficulty;

            do
            {
                if (previousHash == null)   //genesis block
                {
                    hashValue = 0;
                    break;
                }
                byte[] nonceByte = BitConverter.GetBytes(++nonce);
                byte[] hashInput = MakeHashInput(previousHash, nonceByte);
                hashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashValue < difficulty);

            return hashValue.ToByteArray();
        }

        private static byte[] MakeHashInput(byte[] previousHash, byte[] nonce)
        {
            return previousHash.Concat(nonce).ToArray();
        }

        public int Nonce
        {
            get; set;
        }

        public byte[] PreviousHash
        {
            get; set;
        }

        public DateTimeOffset TimeStamp
        {
            get; set;
        }
    }
}
