using PracticeBlockChain.TicTacToeGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    [Serializable]
    public class Block
    {
        // payload 자료형 변경 예정
        private readonly long index;
        private readonly byte[] previousHash;
        private readonly DateTimeOffset timeStamp;
        private readonly Nonce nonce;
        private readonly byte[] signature;
        private readonly string payload;

        public Block(
            long index, 
            byte[] previousHash, 
            DateTimeOffset timeStamp, 
            Nonce nonce, 
            byte[] signature,
            string payload
        ) 
        {
            Index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Signature = signature;
            Payload = payload;      // this.payload = payload; 와 차이점..??
        }

        public long Index
        {
            get;
        }

        public byte[] PreviousHash
        {
            get;
        }

        public DateTimeOffset TimeStamp
        {
            get;
        }

        public Nonce Nonce
        {
            get;
        }

        public byte[] Signature
        {
            get;
        }

        public string Payload
        {
            get;
        }

        public byte[] Serialize()
        {
            Dictionary<string, object> componentsToSerialize = new Dictionary<string, object>();
            if (!(PreviousHash is null))
            {
                componentsToSerialize.Add("previousHash", PreviousHash);                
            }
            componentsToSerialize.Add("nonce", Nonce.NonceValue);
            componentsToSerialize.Add("timeStamp", TimeStamp);
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, componentsToSerialize);
            return mStream.ToArray();
        }

        public byte[] Hash()
        {
            SHA256 hashAlgo = SHA256.Create();
            var concatenated =
                String.Join(
                    "",
                    Serialize().Concat(Nonce.NonceValue).ToArray()
                );
            var byteArray = Encoding.GetEncoding("iso-8859-1").GetBytes(concatenated);
            BigInteger hashDigest = 
                new BigInteger(hashAlgo.ComputeHash(byteArray));
            return hashDigest.ToByteArray();
        }
    }
}