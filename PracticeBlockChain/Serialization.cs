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
