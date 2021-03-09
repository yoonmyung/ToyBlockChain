using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain
{
    class NonceGenerator
    {
        public Nonce GenerateNonce()
        {
            return MakeNewNonce();
        }

        private Nonce MakeNewNonce()
        {
            byte[] nonceBytes = new byte[10];
            Random random = new Random();
            random.NextBytes(nonceBytes);

            return new Nonce(nonceBytes);
        }
    }
}
