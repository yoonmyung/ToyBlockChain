using PracticeBlockChain.Cryptography;
using System;
using System.Collections.Generic;

namespace PracticeBlockChain.TicTacToeGame
{
    public static class AddressPlayerMappingAttribute
    {
        private static readonly Dictionary<Address, string> _listofPlayers
             = new Dictionary<Address, string>();

        public static void AddPlayer(Address address, string playerName)
        {
            _listofPlayers.Add(address, playerName);
        }

        public static string GetPlayer(Address address)
        {
            try
            {
                return _listofPlayers[address];
            }
            catch (ArgumentNullException exception)
            {
                return null;
            }
        } 
    }
}
