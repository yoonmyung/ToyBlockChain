using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    [Serializable]
    public class Action
    {
        private readonly BigInteger nonce;
        private readonly Address signer;
        private readonly PublicKey publicKey;
        private readonly PrivateKey privateKey;
        private readonly byte[] payload;

        public Action(
            BigInteger nonce, 
            Address signer, 
            PublicKey publicKey,
            PrivateKey privateKey,
            byte[] payload
        )
        {
            Nonce = nonce;
            Signer = signer;
            PublicKey = publicKey;
            this.privateKey = privateKey;
            Payload = payload;
        }

        public BigInteger Nonce
        {
            get; set;
        }

        public Address Signer
        {
            get; set;
        }

        public PublicKey PublicKey 
        { 
            get; set;
        }

        public byte[] Payload
        {
            get; set;
        }

        public static byte[] Serialize(Action action)
        {
            byte[] input = action.Nonce.ToByteArray()
                           .Concat(action.Signer.AddressValue)
                           .Concat(action.Payload)
                           .ToArray();
            return Serialization.Serialize(input);
        }

        public static HashDigest Hash(Action action)
        {
            // Hash the action to sign.
            SHA256 hashAlgo = SHA256.Create();
            HashDigest hashDigest = new HashDigest();
            byte[] hashInput = Action.Serialize(action);
            hashDigest.HashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest;
        }
    }
}
