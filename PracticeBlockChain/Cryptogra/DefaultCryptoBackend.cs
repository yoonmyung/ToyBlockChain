using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

namespace PracticeBlockChain.Crypto
{
    // Libplanet/DefaultCryptoBackend.cs 참고
    public class DefaultCryptoBackend
    {
        public static byte[] Sign(HashDigest messageHash, PrivateKey privateKey)
        {
            // messageHash가 서명할 action같음

            var h = new Sha256Digest();
            var kCalculator = new HMacDsaKCalculator(h);
            var signer = new ECDsaSigner(kCalculator);
            signer.Init(true, privateKey.KeyParam);
            BigInteger[] rs = signer.GenerateSignature(messageHash.ToByteArray());
            var r = rs[0];
            var s = rs[1];

            BigInteger otherS = privateKey.KeyParam.Parameters.N.Subtract(s);
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

        public static bool Verify(
            HashDigest messageHash,
            byte[] signature,
            PublicKey publicKey)
        {
            // public key로 signature를 verify함. 
            // messageHash는 private key 서명을 풀었을 때 원본 대조를 위해서인듯
            try
            {
                Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1Object.FromByteArray(signature);

                var rs = new[]
                {
                    ((DerInteger)asn1Sequence[0]).Value,
                    ((DerInteger)asn1Sequence[1]).Value,
                };
                var verifier = new ECDsaSigner();
                verifier.Init(false, publicKey.KeyParam);

                return verifier.VerifySignature(messageHash.ToByteArray(), rs[0], rs[1]);
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
