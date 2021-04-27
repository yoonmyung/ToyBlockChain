using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;

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
            if (Action is null)
            {
                componentsToSerialize.Add("actionId", null);
            }
            else
            {
                componentsToSerialize.Add("actionId", Action.ActionId);
            }
            binFormatter.Serialize(mStream, componentsToSerialize);

            return ByteArrayConverter.Compress(mStream.ToArray());
        }

        public bool IsValid()
        {
            var blockHash = new BigInteger(this.BlockHeader.Hash());

            if (this.BlockHeader.Difficulty < 0)
            {
                Console.WriteLine(1);
                return false;
            }
            else if (!(HashCash.IsValid(this.BlockHeader)))
            {
                Console.WriteLine(3);
                return false;
            }
            else if 
            (
                this.BlockHeader.Index > 0 && 
                this.BlockHeader.PreviousHash is null
            )
            {
                Console.WriteLine(4);
                return false;
            }

            return true;
        }
    }
}