using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    public class Hashing
    {
        // Sign를 하기 위해 Action을 Hashing하는 과정
        public static HashDigest Hash(Action action)
        {
            SHA256 hashAlgo = SHA256.Create();
            HashDigest hashDigest = new HashDigest();

            byte[] hashInput = Serialization.SerializeforAction(action);
            hashDigest.HashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest;
        }
    }
}
