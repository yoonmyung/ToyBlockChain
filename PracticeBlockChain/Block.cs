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
        private readonly long _index;
        private readonly byte[] _previousHash;
        private readonly DateTimeOffset _timeStamp;
        private readonly Nonce _nonce;
        private readonly Action _action;

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

        private Dictionary<string, object> ComposeTuplesToSerialize()
        {
            var componentsToSerialize = new Dictionary<string, object>();
            
            if (!(PreviousHash is null))
            {
                componentsToSerialize.Add("previousHash", PreviousHash);
            }
            else
            {
                componentsToSerialize.Add("previousHash", null);
            }
            componentsToSerialize.Add("nonce", Nonce.NonceValue);
            componentsToSerialize.Add("timeStamp", TimeStamp);

            return componentsToSerialize;
        }

        public byte[] Serialize()
        {
            Dictionary<string, object> componentsToSerialize = ComposeTuplesToSerialize();
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            if (!(GetAction is null))
            {
                componentsToSerialize.Add("signature", GetAction.Signature);
            }
            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public byte[] SerializeForStorage()
        {
            Dictionary<string, object> componentsToSerialize = ComposeTuplesToSerialize();
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            componentsToSerialize.Add("index", Index);
            if (!(GetAction is null))
            {
                componentsToSerialize.Add("actionId", GetAction.ActionId);
            }
            else
            {
                componentsToSerialize.Add("actionId", null);
            }
            binFormatter.Serialize(mStream, componentsToSerialize);
            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public byte[] Hash()
        {
            // HashCash의 CalculateHash와의 차이점
            // HashCash에서는 랜덤 nonce가 들어가지만, 여기서는 이 블록의 nonce가 들어간다
            var hashAlgo = SHA256.Create();
            var concatenated =
                String.Join
                (
                    "",
                    Serialize().Concat(Nonce.NonceValue).ToArray()
                );
            var byteArray = Encoding.GetEncoding("iso-8859-1").GetBytes(concatenated);
            var hashDigest = new BigInteger(hashAlgo.ComputeHash(byteArray));
            return hashDigest.ToByteArray();
        }
    }
}