using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;
using System.IO;

namespace PracticeBlockChain
{
    [Serializable]
    public class Action
    {
        private readonly BigInteger txNonce;
        private readonly Address signer;
        private readonly (Player player, Position position) payload;

        public Action(
            BigInteger txNonce,
            Address signer, 
            (Player player, Position position) payload
        )
        {
            TxNonce = txNonce;
            Signer = signer;
            Payload = payload;
        }

        public BigInteger TxNonce
        {
            get;
        }

        public Address Signer
        {
            get;
        }

        public (Player player, Position position) Payload
        {
            get;
        }

        public byte[] Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(this.TxNonce.ToByteArray());
                    writer.Write(this.Signer.AddressValue);
                    writer.Write(this.Payload.player.Name);
                    writer.Write(this.Payload.position.X);
                    writer.Write(this.Payload.position.Y);
                }
                return memoryStream.ToArray();
            }
        }

        public BigInteger Hash()
        {
            // Hash the action to sign.
            SHA256 hashAlgo = SHA256.Create();
            byte[] hashInput = Serialize();
            BigInteger hashDigest = new BigInteger(hashAlgo.ComputeHash(hashInput));
            return hashDigest;
        }
    }
}
