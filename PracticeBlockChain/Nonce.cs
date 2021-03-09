using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    public class Nonce
    {
        private readonly byte[] nonceValue = null;

        public Nonce(byte[] nonceValue)
        {
            NonceValue = nonceValue;
        }

        public byte[] NonceValue
        {
            get;
        }
    }
}
