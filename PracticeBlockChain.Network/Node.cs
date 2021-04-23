using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace PracticeBlockChain.Network
{
    public class Node
    {
        private TcpListener _listener;
        // Each node stores other nodes' address which it has connected.
        // Routing table is Dictionary object. And it's composed of client as Key and listener as Value. 
        private Dictionary<int, int> _routingTable;

        public Node(bool isSeed, int port)
        {
            string seedNodeAddress = "127.0.0.1:65000";
            _routingTable = new Dictionary<int, int>();
            Address = (client: port + 1, listener: port);

        public (int client, int listener) Address
        {
            get;
            private set;
        }

            SetListener("127.0.0.1", port);
            if (!isSeed)
        public Dictionary<int, int> RoutingTable
        {
            get
            {
                return _routingTable;
            }
        }

        private void SetListener(string IP, int port)
        {
            _routingTable = new Dictionary<string, string>();
            _listener = new TcpListener(IPAddress.Parse(IP), port);
            _listener.Server.SetSocketOption
            (
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true
            );
            _listener.Start();
            var listeningThread = new Thread(Listen);
            listeningThread.Start();
        }

        {
            (
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true
            );
            var stream = GetStream(Address.listener, client);
            if (dataType.Contains("String"))
            {
                // Gets address from another node.
                PutAddressToRoutingtable((string)data);
                if (Address.listener.Equals(_seedPort))
                {
                    Send
                    (
                        _routingTable[((IPEndPoint)client.Client.RemoteEndPoint).Port], 
                        _routingTable
                    );
                }
            }
            else if (dataType.Contains("Dictionary"))
            {
                // Get routing table from seed.
                _routingTable = (Dictionary<int, int>)data;
                PrintRoutingTable();
            }
        }

        private void Listen()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");
                var node = _listener.AcceptTcpClient();
                PutAddressToRoutingtable(node);
                            new IPEndPoint(IPAddress.Parse(_ip), Address.client)
            }
        }

        private bool ConnectToNode(object destinationAddress)
        {
            var neighborNode = (string)destinationAddress;
            var seperatedAddress = neighborNode.Split(":");

            try
            {
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"ArgumentNullException: {e}");
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode.ToString().Equals("ConnectionRefused"))
                {
                    // Node which you try to connect no longer connects to you.
                    _routingTable.Remove(destinationPort + 1);
                    PrintRoutingTable();
                }
                else
                {
                    Console.WriteLine($"SocketException: {e}");
                }

                return false;
            }
        }

        {
        }

        private void PutAddressToRoutingtable(object client)
        {
            var node = (TcpClient)client;

            string nodeAddress = (string)GetData();
            string[] seperatedAddress = nodeAddress.Split(",");
            Console.WriteLine($"Connected client: {seperatedAddress[0]}");
            if (!(_routingTable.ContainsKey(int.Parse(seperatedAddress[0]))))
            {
                _routingTable.Add
                (
                    int.Parse(seperatedAddress[0]), 
                    int.Parse(seperatedAddress[1])
                );
            }
            PrintRoutingTable();
        }

        private void SendData(object data)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, data);
            var sizeofData = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfData = memoryStream.ToArray();
        }

        public object GetData()
        {
            var binaryFormatter = new BinaryFormatter();
            byte[] sizeofDataAsByte = new byte[4];
            var sizeofData = BitConverter.ToInt32(sizeofDataAsByte, 0);
            var dataAsByte = new byte[sizeofData];
            var memoryStream = new MemoryStream(dataAsByte);
            memoryStream.Position = 0;
            var data = binaryFormatter.Deserialize(memoryStream);

            return data;
        }

        public void RotateRoutingTable()
        {
            foreach (var address in _routingTable)
            {
                if (address.Key.Equals(Address.client))
                {
                    continue;
                }
                StartConnection
                (
                    destinationAddress: address.Key,
                );
            }
        }

        private void PrintRoutingTable()
        {
            Console.WriteLine("\n<Routing table>");
            foreach (var address in _routingTable)
            {
                Console.WriteLine
                (
                    $"Client: {address.Value}, " +
                    $"Listener: {address.Key}"
                );
            }
            Console.WriteLine();
        }

        public void StartConnection(string destinationAddress, object dataToSend)
        {

            SetClient(clientAddress[0], int.Parse(clientAddress[1]));
            if (!ConnectToNode(destinationAddress))
            {
                Console.WriteLine("Fail to connect to " + destinationAddress);
            }
            else
            {
                SendData(dataToSend);
            }
            DisconnectToNode();
        }
    }
}