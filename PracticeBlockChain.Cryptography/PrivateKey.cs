using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace PracticeBlockChain.Cryptography
{
    public class PrivateKey
    {
        // BouncyCastle
        // : C#에서 bitcoin에 사용되는 public key와 private key 생성에 쓰이는 라이브러리
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

        internal ECPrivateKeyParameters KeyParam { get; }

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

        private ECPoint CalculatePoint(ECPublicKeyParameters pubKeyParams)
        {
            ECDomainParameters dp = KeyParam.Parameters;
            if (!dp.Equals(pubKeyParams.Parameters))
            {
                throw new InvalidOperationException(
                    "ECDH public key has wrong domain parameters"
                );
            }

            BigInteger d = KeyParam.D;

            ECPoint q = dp.Curve.DecodePoint(pubKeyParams.Q.GetEncoded(true));
            if (q.IsInfinity)
            {
                throw new InvalidOperationException(
                    "Infinity is not a valid public key for ECDH"
                );
            }

            BigInteger h = dp.H;
            if (!h.Equals(BigInteger.One))
            {
                d = dp.H.ModInverse(dp.N).Multiply(d).Mod(dp.N);
                q = ECAlgorithms.ReferenceMultiply(q, h);
            }

            ECPoint p = q.Multiply(d).Normalize();
            if (p.IsInfinity)
            {
                throw new InvalidOperationException(
                    "Infinity is not a valid agreement value for ECDH"
                );
            }

            return p;
        }

        public byte[] Sign(byte[] messageHash)
        {
            // messageHash = 서명할 Action
            var h = new Sha256Digest();
            var kCalculator = new HMacDsaKCalculator(h);
            var signer = new ECDsaSigner(kCalculator);
            signer.Init(true, this.KeyParam);
            BigInteger[] rs =
                signer.GenerateSignature(messageHash);
            var r = rs[0];
            var s = rs[1];

            BigInteger otherS =
                this.KeyParam.Parameters.N.Subtract(s);
            if (s.CompareTo(otherS) == 1)
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
