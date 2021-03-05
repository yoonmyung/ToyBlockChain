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
        byte[] previousHash;
        DateTimeOffset timeStamp;
        int nonce;

        public Block(byte[] previousHash, DateTimeOffset timeStamp, int nonce) 
        {
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
        }

        public byte[] calculateHash(int difficulty, byte[] previousHash)
        {
            using SHA256 hashAlgo = SHA256.Create();
            BigInteger hashValue;

            do
            {
                if (previousHash == null)   //genesis block
                {
                    previousHash = new byte[] {};
                    hashValue = 0;
                    break;
                }
                byte[] nonceByte = BitConverter.GetBytes(++nonce);
                byte[] hashInput = makeHashInput(previousHash, nonceByte);
                hashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashValue < difficulty);

            previousHash = 
                BlockChain.makeBlock(
                    previousHash, 
                    hashValue.ToByteArray(), 
                    DateTimeOffset.Now, 
                    nonce
                );

            return previousHash;
        }

        private byte[] makeHashInput(byte[] previousHash, byte[] nonce)
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
