using PracticeBlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Player
    {
        private readonly string name;
        private readonly PrivateKey privateKey;
        private readonly PublicKey publicKey;
        private readonly Address address;

        public Player(
            string name, 
            PrivateKey privateKey, 
            PublicKey publicKey, 
            Address address
        ) 
        {
            Name = name;
            this.privateKey = privateKey;
            PublicKey = publicKey;
            Address = address;
        }

        public Position Put(Position position)
        {
            return new Position(position.X, position.Y);
        }

        public string Name
        {
            get; set;
        }

        public PrivateKey PrivateKey
        {
            get;
        }

        public PublicKey PublicKey
        {
            get; set;
        }

        public Address Address
        {
            get; set;
        }
    }
}
