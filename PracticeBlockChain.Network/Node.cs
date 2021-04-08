using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PracticeBlockChain.Network
{
    public class Node
    {
        public readonly string _bindIP = "127.0.0.1";
        private readonly int _bindPort = 8888;
        private readonly TcpListener _listner;
        private readonly Queue<string> _rountingTable;

        public int BindPort
        {
            get
            {
                return _bindPort;
            }
        }

        // It's Node.
        public Node(int port)
        {
            _bindPort = port;
        }

        // It's Seed node.
        public Node()
        {
            var localAddress = IPAddress.Parse(_bindIP);
            _listner = new TcpListener(localAddress, _bindPort);
            _listner.Start();
            Listen();
        }

        // Methods which Seed node uses.
        private void Listen()
        {
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                var node = _listner.AcceptTcpClient();
                Console.WriteLine("Connected!");
                PutAddressToTable(node);
            }
        }

        private void PutAddressToTable(TcpClient node)
        {
            int eachByte;
            var bytes = new Byte[256];

            NetworkStream stream = node.GetStream();
            while ((eachByte = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // In this part seed node stores address of node to Rounting table.
                var nodeAddress = Encoding.ASCII.GetString(bytes, 0, eachByte);
                Console.WriteLine($"SeedNode Received: {nodeAddress}");
                var address = Encoding.ASCII.GetBytes(nodeAddress);

                // In this part seed node sends Rounting table to the last connected node.
                stream.Write(address, 0, address.Length);
                Console.WriteLine($"SeedNode Sent: {address}");
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