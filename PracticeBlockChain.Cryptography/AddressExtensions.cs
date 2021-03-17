using System;
using System.Collections.Generic;
using System.Text;

namespace PracticeBlockChain.Cryptography
{
    public static class AddressExtensions
    {
        public static Address ToAddress(this PublicKey publicKey)
        {
            return new Address(publicKey);
        }

        public static Address ToAddress(this PrivateKey privateKey)
        {
            return new Address(privateKey.PublicKey);
        }
    }
}
