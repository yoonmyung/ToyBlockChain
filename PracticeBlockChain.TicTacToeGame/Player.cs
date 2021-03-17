using PracticeBlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PracticeBlockChain.Cryptography;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using System.IO;
using Org.BouncyCastle.Asn1;

namespace PracticeBlockChain.TicTacToeGame
{
    public class Player
    {
        private readonly string name;
        private readonly PrivateKey privateKey = new PrivateKey();
        private readonly PublicKey publicKey;
        private readonly Address address;

        public Player(string name) 
        {
            Name = name;
            PublicKey = privateKey.PublicKey;
            Address = new Address(PublicKey);
        }

        public string Name
        {
            get;
        }

        public PublicKey PublicKey
        {
            get;
        }

        public Address Address
        {
            get;
        }

        public byte[] Sign(System.Numerics.BigInteger messageHash)
        {
            // messageHash = 서명할 Action
            var h = new Sha256Digest();
            var kCalculator = new HMacDsaKCalculator(h);
            var signer = new ECDsaSigner(kCalculator);
            signer.Init(true, privateKey.KeyParam);
            Org.BouncyCastle.Math.BigInteger[] rs =
                signer.GenerateSignature(messageHash.ToByteArray());
            var r = rs[0];
            var s = rs[1];

            Org.BouncyCastle.Math.BigInteger otherS =
                privateKey.KeyParam.Parameters.N.Subtract(s);
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

        public bool Verify(
            System.Numerics.BigInteger messageHash,
            byte[] signature,
            PublicKey publicKey
        )
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
