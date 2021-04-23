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
        // Each node has a listener object.
        // Whenever another node sends connection, a node takes it by its listener object.
        // Each node stores other nodes' address which it has connected.
        // Routing table is Dictionary object. And it's composed of client as Key and listener as Value. 
        private Dictionary<int, int> _routingTable;

        private const string _ip = "127.0.0.1";
        private const int _seedPort = 65000;

        public Node(int port)
        {
            SetListener(port);
            _routingTable = new Dictionary<int, int>();
            Address = (client: port + 1, listener: port);
        }

        public (int client, int listener) Address
        {
            get;
            private set;
        }

            if (!isSeed)
        public Dictionary<int, int> RoutingTable
        {
            get
            {
                return _routingTable;
            }
        }

        private TcpListener Listener
        {
            get;
            set;
        }

        private void SetListener(int port)
        {
            Listener = new TcpListener(IPAddress.Parse(_ip), port);
            Listener.Server.SetSocketOption
            (
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true
            );
        }

        public void StartListener()
        {
            Listener.Start();
            var listeningThread = 
                new Thread
                (
                    () =>
                    {
                        while (true)
                        {
                            Listen();
                        }
                    }
                 );
            listeningThread.Start();
        }

        public void StopListener()
        {
            Listener.Stop();
        }

        public void Listen()
        {
            Console.WriteLine("Waiting for a connection. . .");
            var client = Listener.AcceptTcpClient();
            Console.WriteLine
            (
                "Listener: Connected to " +
                ((IPEndPoint)client.Client.RemoteEndPoint).Port
            );
            Receive(client);
            DisconnectClient(client, client.GetStream());
        }

        private void Receive(object obj)
        {
            var client = (TcpClient)obj;
            var stream = GetStream(Address.listener, client);

            var data = GetData(stream);
            string dataType = data.GetType().FullName;

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

        private TcpClient SetClient()
        {
            TcpClient client = null;
            while (true)
            {
                try
                {
                    client =
                        new TcpClient
                        (
                            new IPEndPoint(IPAddress.Parse(_ip), Address.client)
                        );
                    client.Client.SetSocketOption
                    (
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress,
                        true
                    );
                    break;
                }
                catch (SocketException e) 
                { 
                }
            }

            return client;
        }

            var client = SetClient();
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

        private void DisconnectClient(TcpClient client, NetworkStream stream)
        {
            client.Close();
            client.Dispose();
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

        public void RotateRoutingTable(object data)
        {
            while (_routingTable.Count < 1) ;
            foreach (var address in _routingTable)
            {
                if (address.Key.Equals(Address.client))
                {
                    continue;
                }
                Send(address.Value, data);
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
    }
}