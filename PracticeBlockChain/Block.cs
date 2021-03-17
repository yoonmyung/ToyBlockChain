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

        public Block(
            long index, 
            BigInteger previousHash, 
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
            get;
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
            // 기존에는 HashCash에서 찾은 nonce값을 해시함수에 넣어 현재 해시값을 얻는 형식이었는데
            // 똑같은 해시값을 얻으려면 결국 nonce 뿐만 아니라 HashCash와 동일한 input을 넣어야 함
            // 그걸 갖고 있는 게 Serialize 함수가 아닌가 해서 넣었는데.. 맞나
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashDigest = 
                new BigInteger(hashAlgo.ComputeHash(
                    (byte[])Serialize().Concat(Nonce.NonceValue))
                );
            return hashDigest;
        }
    }
}