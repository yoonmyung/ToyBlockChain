using System;
using System.Collections.Generic;
using System.Numerics;
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
            PrivateKey = privateKey;
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

        public PrivateKey PrivateKey
        {
            get;  set;
        }

        public byte[] Payload
        {
            get; set;
        }
    }
}
