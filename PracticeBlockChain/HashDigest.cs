using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PracticeBlockChain
{
    public class HashDigest
    {
        BigInteger _hashValue;

        public HashDigest()
        {
            HashValue = -111;
        }

        public HashDigest(BigInteger hashValue)
        {
            HashValue = hashValue;
        }

        public BigInteger HashValue
        {
            get; set;
        }

        public byte[] ToByteArray()
        {
            return HashValue.ToByteArray();
        }
    }
}
