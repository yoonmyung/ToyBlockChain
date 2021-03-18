using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PracticeBlockChain
{
    [Serializable]
    public class Action
    {
        // payload 자료형 변경 예정
        private readonly long txNonce;
        private readonly Address signer;
        private readonly string payload;

        public Action(
            long txNonce,
            Address signer, 
            string payload
        )
        {
            TxNonce = txNonce;
            Signer = signer;
            Payload = payload;
        }

        public long TxNonce
        {
            get;
        }

        public Address Signer
        {
            get;
        }

        public string Payload
        {
            get;
        }

        public byte[] Serialize()
        {
            Dictionary<string, object> componentsToSerialize = new Dictionary<string, object>();
            componentsToSerialize.Add("txNonce", TxNonce);
            componentsToSerialize.Add("signer", Signer.AddressValue);
            componentsToSerialize.Add("payload", Payload);
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, componentsToSerialize);
            return mStream.ToArray();
        }

        public byte[] Hash()
        {
            // Hash the action to sign.
            SHA256 hashAlgo = SHA256.Create();
            byte[] hashInput = Serialize();
            BigInteger hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest.ToByteArray();
        }
    }
}
