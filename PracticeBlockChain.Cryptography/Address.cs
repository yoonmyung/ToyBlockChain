using Org.BouncyCastle.Crypto.Digests;
using System.Linq;

namespace PracticeBlockChain.Cryptography
{
    public class Address
    {
        private const int _addressSize = 20;

        public Address(PublicKey publicKey)
        {
            AddressValue = DeriveAddress(publicKey);
        }

        public Address(byte[] addressValue)
        {
            AddressValue = addressValue;
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

            return output.Skip(output.Length - _addressSize).ToArray();
        }
    }
}
