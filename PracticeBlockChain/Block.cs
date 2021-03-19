using System;
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
            GetAction = action;
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

        public Action GetAction
        {
            get;
        }

        public byte[] Serialize()
        {
            Dictionary<string, object> componentsToSerialize = 
                new Dictionary<string, object>();
            if (!(PreviousHash is null))
            {
                componentsToSerialize.Add("previousHash", PreviousHash);                
            }
            if (!(action is null))
            {
                componentsToSerialize.Add("signature", action.Signature);
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
            // HashCash의 CalculateHash와의 차이점
            // HashCash에서는 랜덤 nonce가 들어가지만, 여기서는 이 블록의 nonce가 들어간다
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