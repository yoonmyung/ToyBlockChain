using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    public class Block
    {
        private readonly long index;
        private readonly byte[] previousHash;
        private readonly DateTimeOffset timeStamp;
        private readonly Nonce nonce;
        private readonly long difficulty;

        public Block(
            long index, 
            byte[] previousHash, 
            DateTimeOffset timeStamp, 
            Nonce nonce, 
            long difficulty) 
        {
            Index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Difficulty = difficulty;
        }

        public long Index
        {
            get; set;
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

        public long Difficulty
        {
            get; set;
        }
    }
}
