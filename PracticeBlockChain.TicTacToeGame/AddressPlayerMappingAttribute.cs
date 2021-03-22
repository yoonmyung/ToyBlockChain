using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;

namespace PracticeBlockChain.TicTacToeGame
{
    public static class AddressPlayerMappingAttribute
    {
        private static readonly Dictionary<Address, string> listofPlayers;

        static AddressPlayerMappingAttribute() {
            listofPlayers = new Dictionary<Address, string>();
        }

        public static void AddPlayer(Address address, string playerName)
        {
            listofPlayers.Add(address, playerName);
        }

        public static string GetPlayer(Address address)
        {
            try
            {
                return listofPlayers[address];
            }
            catch (ArgumentNullException exception)
            {
                return null;
            }
        } 
    }
}
