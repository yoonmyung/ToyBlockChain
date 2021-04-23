using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace PracticeBlockChain
{
    [Serializable]
    public class Block
    {
        public Block(BlockHeader blockHeader, Action action) 
        {
            BlockHeader = blockHeader;
            Action = action;
        }

        public BlockHeader BlockHeader
        {
            get;
        }

        public Action Action
        {
            get;
        }

        public byte[] Serialize()
        {
            var componentsToSerialize = new Dictionary<string, object>();
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();

            componentsToSerialize.Add
            (
                "previousHash",
                BlockHeader.PreviousHash is null ? null : BlockHeader.PreviousHash
            );
            componentsToSerialize.Add("nonce", BlockHeader.Nonce.NonceValue);
            componentsToSerialize.Add("timeStamp", BlockHeader.TimeStamp);
            componentsToSerialize.Add("index", BlockHeader.Index);
            componentsToSerialize.Add("difficulty", BlockHeader.Difficulty);
            componentsToSerialize.Add
            (
                "actionId",
                Action.ActionId is null ? null : Action.ActionId
            );
            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }
    }
}