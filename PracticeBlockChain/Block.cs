using PracticeBlockChain.TicTacToeGame;
using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    [Serializable]
    public class Block
    {
        private readonly long index;
        private readonly BigInteger previousHash;
        private readonly DateTimeOffset timeStamp;
        private readonly Nonce nonce;
        private readonly byte[] signature;
        private readonly Board state;

        public Block(
            long index, 
            BigInteger previousHash, 
            DateTimeOffset timeStamp, 
            Nonce nonce, 
            byte[] signature,
            Board state
        ) 
        {
            this.index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Signature = signature;
            State = state;
        }

        public Nonce Nonce
        {
            get;
        }

        public BigInteger PreviousHash
        {
            get;
        }

        public DateTimeOffset TimeStamp
        {
            get;
        }

        public byte[] Signature
        {
            get;
        }

        public Board State
        {
            get;
        }

        public byte[] Serialize()
        {
            byte[] input = this.previousHash.ToByteArray()
                           .Concat(this.nonce.NonceValue)
                           .Concat(BitConverter.GetBytes(this.timeStamp.Offset.TotalMinutes))
                           .ToArray();
            return Serialization.Serialize(input);
        }

        public BigInteger Hash()
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashDigest = 
                new BigInteger(hashAlgo.ComputeHash(
                    (byte[])Serialize().Concat(Nonce.NonceValue))
                );
            return hashDigest;
        }
    }
}