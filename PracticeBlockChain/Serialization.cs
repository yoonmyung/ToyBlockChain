using Bencodex;
using Bencodex.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PracticeBlockChain
{
    public class Serialization
    {
        // 1. Hashcash의 input으로 들어가는 Block
        // 2. Sign의 파라미터로 들어가기 위해 Hashing하는 Action

        public static byte[] SerializeforAction(Action action)
        {
            byte[] input = action.Nonce.ToByteArray()
               .Concat(action.Signer.AddressValue)
               .Concat(action.Payload)
               .ToArray();
            return Serialize(input);
        }

        public static byte[] SerializeforBlock(
            byte[] previousHash, 
            Nonce nonce, 
            DateTimeOffset timeStamp
        )
        {
            byte[] input = previousHash
                           .Concat(nonce.NonceValue)
                           .Concat(BitConverter.GetBytes(timeStamp.Offset.TotalMinutes))
                           .ToArray();
            return Serialize(input);
        }

        public static byte[] Serialize(object obj)
        {
            byte[] bytes;
            IFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public static object Deserialize(byte[] serializedInput)
        {
            object obj;
            IFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                obj = formatter.Deserialize(stream);
            }
            return obj;
        }
    }
}
