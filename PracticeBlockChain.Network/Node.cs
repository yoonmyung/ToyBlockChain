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
        // _routingTable = 
        // {
        //     peer's address of listener, 
        //     [ peer's address of client, is it connected with this node? ] 
        // }
        private Dictionary<string, ArrayList> _routingTable;
        private readonly TcpListener _listener;
        private NetworkStream _stream;

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
                var node = _listener.AcceptTcpClient();
                Console.WriteLine($"request connection from {((IPEndPoint)node.Client.RemoteEndPoint).Port}!");
            }
        }

                _stream = _client.GetStream();

        private void SendAddress()
        {
            // Node makes a client stream for reading and writing.
            // Seed node and peer node communicate through stream.
            Thread.Sleep(1000);
            var byteAddress =
                Encoding.ASCII.GetBytes(string.Format(_address[0] + "," + _address[1]));
            _stream.Write(byteAddress, 0, byteAddress.Length);
        }

        private void PutAddressToTable(object client)
        {
            var node = (TcpClient)client;
            _stream = node.GetStream();
            string address = GetAddress(node);
            string[] addresses = address.Split(",");
            ArrayList arrayList = new ArrayList();
            arrayList.Add(addresses[0]);
            arrayList.Add(false);
            _routingTable.Add(addresses[1], arrayList);
        }

        private string GetAddress(TcpClient node)
        {
            var bytes = new Byte[256];
            string nodeAddress = null;

            int addressLength = _stream.Read(bytes, 0, bytes.Length);
            nodeAddress = Encoding.ASCII.GetString(bytes, 0, addressLength);
            Console.WriteLine($"SeedNode Received: {nodeAddress}");

            return nodeAddress;
        }

        private void SendRoutingTable(TcpClient node)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, data);
            var sizeofData = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfData = memoryStream.ToArray();
            _stream.Write(sizeofData, 0, 4);
            _stream.Write(byteArrayOfData, 0, byteArrayOfData.Length);
        }

        // Methods which node uses.
        public void ConnectToSeedNode(TcpClient client)
        private void GetData()
        {
            var binaryFormatter = new BinaryFormatter();
            byte[] sizeofDataAsByte = new byte[4];

            _stream.Read(sizeofDataAsByte, 0, sizeofDataAsByte.Length);
            var sizeofData = BitConverter.ToInt32(sizeofDataAsByte, 0);
            var dataAsByte = new byte[sizeofData];
            _stream.Read(dataAsByte, 0, dataAsByte.Length);

            var memoryStream = new MemoryStream(dataAsByte);
            memoryStream.Position = 0;
            var data = binaryFormatter.Deserialize(memoryStream);
            if (data.GetType().FullName.Contains("System.Collections.Generic.Dictionary"))
            {
                _routingTable = (Dictionary<string, ArrayList>)data;
                _routingTable[_address[1]][1] = true;
                PrintRoutingTable();
            }
            else
            {
                Console.WriteLine($"received from other node: {data}");
            }
        }
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        private void SendAddress(TcpClient node, String address)
        {
        }

        private void GetRoutingTable(TcpClient node)
        private void PrintRoutingTable()
        {
            Console.WriteLine("\n<Routing table>");
            foreach (var address in _routingTable)
            {
                Console.WriteLine
                (
                    $"Client: {address.Value[0]}, " +
                    $"Listener: {address.Key}\n"
                );
            }
        }
    }
}