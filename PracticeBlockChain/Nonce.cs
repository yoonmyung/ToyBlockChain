using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    public class Nonce
    {
        public Nonce(byte[] nonceValue)
        {
            NonceValue = nonceValue;
        }

        public byte[] NonceValue
        {
            get;
        }

        public static Nonce GenerateNonce()
        {
            var random = new Random();
            var nonceBytes = new byte[10];
            random.NextBytes(nonceBytes);

            return new Nonce(nonceBytes);
        }
    }
}
