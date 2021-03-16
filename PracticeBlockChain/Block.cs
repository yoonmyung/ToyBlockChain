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
        private readonly HashDigest previousHash;
        private readonly DateTimeOffset timeStamp;
        private readonly Nonce nonce;
        private readonly byte[] signature;

        public Block(
            long index, 
            HashDigest previousHash, 
            DateTimeOffset timeStamp, 
            Nonce nonce, 
            byte[] signature
        ) 
        {
            Index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Signature = signature;
        }

        public long Index
        {
            get; set;
        }

        public Nonce Nonce
        {
            get; set;
        }

        public HashDigest PreviousHash
        {
            get; set;
        }

        public DateTimeOffset TimeStamp
        {
            get; set;
        }

        public byte[] Signature
        {
            get; set;
        }

        public static byte[] Serialize(
            byte[] previousHash,
            Nonce nonce,
            DateTimeOffset timeStamp
        )
        {
            byte[] input = previousHash
                           .Concat(nonce.NonceValue)
                           .Concat(BitConverter.GetBytes(timeStamp.Offset.TotalMinutes))
                           .ToArray();
            return Serialization.Serialize(input);
        }

        public static HashDigest Hash(Nonce nonce)
        {
            SHA256 hashAlgo = SHA256.Create();
            HashDigest hashDigest = new HashDigest();
            hashDigest.HashValue = new BigInteger(hashAlgo.ComputeHash(nonce.NonceValue));
            return hashDigest;
        }
    }
}