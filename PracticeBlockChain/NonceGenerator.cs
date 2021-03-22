using System;

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
            var random = new Random();
            var nonceBytes = new byte[10];
            random.NextBytes(nonceBytes);

            return new Nonce(nonceBytes);
        }
    }
}