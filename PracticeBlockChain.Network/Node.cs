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
        private readonly string _bindIP = "127.0.0.1";
        private readonly int _bindPort = 8888;
        private Stack<string> _routingTable;

        public Node(bool isSeed)
        {
            if (isSeed)
            {
                _routingTable = new Dictionary<string, ArrayList>();
                _client = null;
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                _listener.Server.SetSocketOption
                (
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress,
                    true
                );
                _listener.Start();
            }
            else
            {
                // It's peer node.
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
                _listener.Server.SetSocketOption
                (
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress,
                    true
                );
                _listener.Start();

                _client = new TcpClient();
                _client.Client.SetSocketOption
                (
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress,
                    true
                );
            }
        }

        // Methods which Seed node uses.
        private void Listen(TcpListener listener)
        {
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                var node = listener.AcceptTcpClient();
                Console.WriteLine("Connected!");
                var thread = new Thread(PutAddressToTable);
                thread.Start(node);
            }
        }

        private void PutAddressToTable(object client)
        {
            var node = (TcpClient)client;
            _routingTable.Push(GetAddress(node));
            SendRoutingTable(node);
        }

        private string GetAddress(TcpClient node)
        {
            var bytes = new Byte[256];
            string nodeAddress = null;

            NetworkStream stream = node.GetStream();
            int addressLength = stream.Read(bytes, 0, bytes.Length);
            nodeAddress = Encoding.ASCII.GetString(bytes, 0, addressLength);
            Console.WriteLine($"SeedNode Received: {nodeAddress}");

            return nodeAddress;
        }

        private void SendRoutingTable(TcpClient node)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            NetworkStream stream = node.GetStream();
            binaryFormatter.Serialize(memoryStream, _routingTable);
            var sizeofRoutingTable = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfRoutingTable = memoryStream.ToArray();
            stream.Write(sizeofRoutingTable, 0, 4);
            stream.Write(byteArrayOfRoutingTable, 0, byteArrayOfRoutingTable.Length);
        }

        // Methods which node uses.
        public void ConnectToSeedNode(TcpClient client)
        {
            try
            {
                SendAddress
                (
                    client,
                    ((IPEndPoint)client.Client.LocalEndPoint).Address.MapToIPv4().ToString()
                    + ":" +
                    _bindPort.ToString()
                );
                GetRoutingTable(client);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        private void SendAddress(TcpClient node, String address)
        {
            Byte[] byteAddress = Encoding.ASCII.GetBytes(address);

            // In this part, node gets(makes?) a client stream for reading and writing.
            // Seed node and node communicate through stream.
            NetworkStream stream = node.GetStream();
            stream.Write(byteAddress, 0, byteAddress.Length);
            Console.WriteLine($"Node Sent: {address}");
        }

        private void GetRoutingTable(TcpClient node)
        {
            var binaryFormatter = new BinaryFormatter();
            byte[] sizeofRoutingTableAsByte = new byte[4];

            NetworkStream stream = node.GetStream();
            stream.Read(sizeofRoutingTableAsByte, 0, sizeofRoutingTableAsByte.Length); 
            var sizeofRoutingTable = BitConverter.ToInt32(sizeofRoutingTableAsByte, 0); 
            var routingTableAsByte = new byte[sizeofRoutingTable]; 
            stream.Read(routingTableAsByte, 0, routingTableAsByte.Length); 

            var memoryStream = new MemoryStream(routingTableAsByte); 
            memoryStream.Position = 0; 
            _routingTable = (Stack<string>)binaryFormatter.Deserialize(memoryStream);
            Console.WriteLine("-----------Routing table----------");
            foreach(string address in _routingTable)
            {
                Console.WriteLine(address);
            }
        }
    }
}