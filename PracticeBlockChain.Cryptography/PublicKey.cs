using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace PracticeBlockChain.Cryptography
{
    [Serializable]
    public class PublicKey
    {
        public PublicKey(byte[] publicKey)
            : this(GetECPublicKeyParameters(publicKey))
        {
        }

        internal PublicKey(ECPublicKeyParameters keyParam)
        {
            KeyParam = keyParam;
        }

        internal ECPublicKeyParameters KeyParam 
        {
            get; 
        }

        public byte[] Format(bool compress)
        {
            return KeyParam.Q.GetEncoded(compress);
        }

        private static ECPublicKeyParameters GetECPublicKeyParameters(byte[] bs)
        {
            var ecParams = PrivateKey.GetECParameters();
            return new ECPublicKeyParameters(
                "ECDSA",
                ecParams.Curve.DecodePoint(bs),
                ecParams
            );
        }

        public bool Verify(byte[] messageHash, byte[] signature)
        {
            // Parameter messageHash is the original data  
            // used to compare to signed data by private key.
            try
            {
                var asn1Sequence = (Asn1Sequence)Asn1Object.FromByteArray(signature);
                var rs = new[]
                {
                    ((DerInteger)asn1Sequence[0]).Value,
                    ((DerInteger)asn1Sequence[1]).Value,
                };
                var verifier = new ECDsaSigner();

                verifier.Init(false, this.KeyParam);

                return verifier.VerifySignature(messageHash, rs[0], rs[1]);
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
