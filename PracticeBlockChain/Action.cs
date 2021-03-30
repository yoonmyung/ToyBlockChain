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
        private readonly byte[] _actionId;
        private readonly long _txNonce;
        private readonly Address _signer;
        private readonly Position _payload;
        private readonly byte[] _signature;

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
            ActionId = Hash();
        }

        public byte[] ActionId
        {
            get;
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

        private Dictionary<string, object> ComposeTuplesToSerialize()
        {
            var componentsToSerialize = new Dictionary<string, object>();

            componentsToSerialize.Add("txNonce", TxNonce);
            componentsToSerialize.Add("signer", Signer.AddressValue);
            if (!(Payload is null))
            {
                componentsToSerialize.Add("payload_x", Payload.X);
                componentsToSerialize.Add("payload_y", Payload.Y);
            }
            else
            {
                componentsToSerialize.Add("payload_x", null);
                componentsToSerialize.Add("payload_y", null);
            }

            return componentsToSerialize;
        }

        public byte[] Serialize()
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            var componentsToSerialize = ComposeTuplesToSerialize();

            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public byte[] SerializeForStorage()
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            var componentsToSerialize = ComposeTuplesToSerialize();

            componentsToSerialize.Add("signature", Signature);
            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public byte[] Hash()
        {
            // Hash the action to sign.
            var hashAlgo = SHA256.Create();
            byte[] hashInput = Serialize();
            var hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest.ToByteArray();
        }

        public string[,] Execute(
            BlockChain blockChain,
            Position position,
            Address address
        )
        {
            string[,] updatedState = blockChain.GetCurrentState();

            if (GameStateController.IsAbletoPut(updatedState, position))
            {
                updatedState[position.X, position.Y] =
                    AddressPlayerMappingAttribute.GetPlayer(address);
            }

            return updatedState;
        }
    }
}
