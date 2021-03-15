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
        private readonly byte[] previousHash;
        private readonly DateTimeOffset timeStamp;
        private readonly Nonce nonce;
        private readonly Action action;

        public Block(
            long index, 
            byte[] previousHash, 
            DateTimeOffset timeStamp, 
            Nonce nonce, 
            Action action
        ) 
        {
            Index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Action = action;
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

        public Action Action
        {
            get; set;
        }
    }
}