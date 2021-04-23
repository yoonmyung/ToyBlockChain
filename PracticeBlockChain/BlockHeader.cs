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
    public class BlockHeader
    {
        public BlockHeader
        (
            long index,
            byte[] previousHash,
            DateTimeOffset timeStamp,
            Nonce nonce,
            long difficulty
        )
        {
            Index = index;
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
            Difficulty = difficulty;
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

        public long Difficulty
        {
            get;
        }

        public byte[] Serialize()
        {
            var componentsToSerialize = new Dictionary<string, object>();
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            componentsToSerialize.Add
            (
                "previousHash", 
                PreviousHash is null ? null : PreviousHash
            );
            componentsToSerialize.Add("nonce", Nonce.NonceValue);
            componentsToSerialize.Add("timeStamp", TimeStamp);
            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public byte[] Hash()
        {
            // Difference with CalculateHash function in HashCash class.
            // In HashCash class, it uses randomized nonce,
            // But in BlockHeader class, it uses nonce of block.
            var hashAlgorithm = SHA256.Create();
            var concatenated =
                String.Join
                (
                    "",
                    Serialize().Concat(Nonce.NonceValue).ToArray()
                );
            var byteArray = Encoding.GetEncoding("iso-8859-1").GetBytes(concatenated);
            var hashDigest = new BigInteger(hashAlgorithm.ComputeHash(byteArray));
            return hashDigest.ToByteArray();
        }
    }
}
