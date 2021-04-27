using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace PracticeBlockChain.Cryptography
{
    public class PrivateKey
    {
        // BouncyCastle
        // Library used to generate public key and private key.
        private PublicKey _publicKey;

        public PrivateKey()
            : this(GenerateKeyParam())
        {
        }

        public PrivateKey(byte[] privateKey)
            : this(
                GenerateKeyFromBytes(privateKey)
            )
        {
        }

        private PrivateKey(ECPrivateKeyParameters keyParam)
        {
            KeyParam = keyParam;
        }

        public PublicKey PublicKey
        {
            get
            {
                if (_publicKey is null)
                {
                    ECDomainParameters ecParams = GetECParameters();
                    ECPoint q = ecParams.G.Multiply(KeyParam.D);
                    var kp = new ECPublicKeyParameters("ECDSA", q, ecParams);
                    _publicKey = new PublicKey(kp);
                }

                return _publicKey;
            }
        }

        public byte[] ByteArray => KeyParam.D.ToByteArrayUnsigned();

        internal ECPrivateKeyParameters KeyParam 
        { 
            get; 
        }

        internal static ECDomainParameters GetECParameters()
        {
            return GetECParameters("secp256k1");
        }

        private static ECDomainParameters GetECParameters(string name)
        {
            X9ECParameters ps = SecNamedCurves.GetByName(name);

            return new ECDomainParameters(ps.Curve, ps.G, ps.N, ps.H);
        }

        private static ECPrivateKeyParameters GenerateKeyParam()
        {
            var gen = new ECKeyPairGenerator();
            var secureRandom = new SecureRandom();
            ECDomainParameters ecParams = GetECParameters();
            var keyGenParam =
                new ECKeyGenerationParameters(ecParams, secureRandom);
            gen.Init(keyGenParam);

            return gen.GenerateKeyPair().Private as ECPrivateKeyParameters;
        }

        private static ECPrivateKeyParameters GenerateKeyFromBytes(byte[] privateKey)
        {
            var param = new ECPrivateKeyParameters(
                "ECDSA",
                new BigInteger(1, privateKey),
                GetECParameters()
            );
            var _ = new PrivateKey(param).PublicKey;

            return param;
        }

        public byte[] Sign(byte[] messageHash)
        {
            // Parameter messageHash is the hash value of action to sign.
            // Sign on the hash value of action. 
            // Not on an action itself.
            var h = new Sha256Digest();
            var kCalculator = new HMacDsaKCalculator(h);
            var signer = new ECDsaSigner(kCalculator);

            signer.Init(true, this.KeyParam);
            BigInteger[] rs = signer.GenerateSignature(messageHash);
            var r = rs[0];
            var s = rs[1];
            BigInteger otherS = this.KeyParam.Parameters.N.Subtract(s);
            if (s.CompareTo(otherS).Equals(1))
            {
                s = otherS;
            }
            var bos = new MemoryStream(72);
            var seq = new DerSequenceGenerator(bos);
            seq.AddObject(new DerInteger(r));
            seq.AddObject(new DerInteger(s));
            seq.Close();

            return bos.ToArray();
        }
    }
}
