using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PracticeBlockChain.Cryptography
{
    public class Address
    {
        public const int _Size = 20;
        private readonly byte[] _addressValue;

        public Address(PublicKey publicKey)
        {
            AddressValue = DeriveAddress(publicKey);
        }

        public byte[] AddressValue
        {
            get;
        }

        private byte[] CalculateHash(byte[] value)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        private byte[] DeriveAddress(PublicKey key)
        {
            byte[] hashPayload = key.Format(false).Skip(1).ToArray();
            var output = CalculateHash(hashPayload);
            return output.Skip(output.Length - _Size).ToArray();
        }
    }
}
