using PracticeBlockChain;
using System;
using System.Collections.Generic;
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

        public KeyValuePair<string, Position> Put(string name, Position position)
        {
            KeyValuePair<string, Position> input 
                = new KeyValuePair<string, Position>(name, position);
            return input;
        }

        public string Name
        {
            get; set;
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
