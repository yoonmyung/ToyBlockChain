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
        static Nonce nonce = new Nonce();

        public Block(byte[] previousHash, DateTimeOffset timeStamp, Nonce nonce) 
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
                //libplanet에서는 Nonce에 매번 랜덤값을 할당하는 것으로 1씩 증가하는 것을 대신함
                nonce.updateNonce();
                byte[] hashInput = MakeHashInput(previousHash, nonce);
                hashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashValue < difficulty);

            return hashValue.ToByteArray();
        }

        private static byte[] MakeHashInput(byte[] previousHash, Nonce nonce)
        {
            return previousHash.Concat(nonce.NonceValue).ToArray();
        }

        public Nonce Nonce
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
