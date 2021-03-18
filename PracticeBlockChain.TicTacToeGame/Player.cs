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
    }
}
