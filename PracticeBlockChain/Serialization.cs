using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    public static class Serialization
    {
        public static byte[] SerializeHashInput
            (byte[] previousHash, Nonce nonce, DateTimeOffset timeStamp)
        {
            NonceGenerator nonceGenerator = new NonceGenerator();

            return previousHash.Concat(nonceGenerator.GenerateNonce().NonceValue)
                               .Concat(BitConverter.GetBytes(timeStamp.Offset.TotalMinutes))
                               .ToArray();
        }
    }
}
