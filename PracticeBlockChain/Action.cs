using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PracticeBlockChain
{
    [Serializable]
    public class Action
    {
        private readonly long txNonce;
        private readonly Address signer;
        private readonly Position payload;
        private readonly byte[] signature;

        public Action(
            long txNonce,
            Address signer, 
            Position payload,
            byte[] signature
        )
        {
            TxNonce = txNonce;
            Signer = signer;
            Payload = payload;
            Signature = signature;
        }

        public long TxNonce
        {
            get;
        }

        public Address Signer
        {
            get;
        }

        public Position Payload
        {
            get;
        }

        public byte[] Signature
        {
            get;
        }

        public byte[] Serialize()
        {
            var componentsToSerialize = new Dictionary<string, object>();
            componentsToSerialize.Add("txNonce", TxNonce);
            componentsToSerialize.Add("signer", Signer.AddressValue);
            componentsToSerialize.Add("payload_x", Payload.X);
            componentsToSerialize.Add("payload_y", Payload.Y);
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, componentsToSerialize);
            return mStream.ToArray();
        }

        public byte[] Hash()
        {
            // Hash the action to sign.
            var hashAlgo = SHA256.Create();
            byte[] hashInput = Serialize();
            var hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest.ToByteArray();
        }
    }
}
