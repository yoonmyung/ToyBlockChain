using System;
using System.Collections;
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
        // _routingTable = 
        // {
        //     peer's address of listener, 
        //     [ peer's address of client, is it connected with this node? ] 
        // }
        private Dictionary<string, ArrayList> _routingTable;
        private TcpClient _client;
        private readonly TcpListener _listener;
        private NetworkStream _stream;
        private string[] _address;

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

        public void Listen()
        {
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                var node = _listener.AcceptTcpClient();
                Console.WriteLine($"request connection from {((IPEndPoint)node.Client.RemoteEndPoint).Port}!");
                PutAddressToTable(node);
            }
        }

        private bool ConnectToNode(object destinationAddress)
        {
            var neighborNode = (string)destinationAddress;
            var seperatedAddress = neighborNode.Split(":");

            var clientThread = new Thread(Listen);
            clientThread.Start();
            try
            {
                _client.Connect
                    (seperatedAddress[0], int.Parse(seperatedAddress[1]));
                _stream = _client.GetStream();
                if (!(_routingTable is null))
                {
                    if (_routingTable.ContainsKey(nodeAddress))
                    {
                        _routingTable[nodeAddress][1] = true;
                    }
                }
                _address =
                    new string[]
                    {
                        ((IPEndPoint)_client.Client.LocalEndPoint).Address.MapToIPv4().ToString() +
                        ":" +
                        ((IPEndPoint)_client.Client.LocalEndPoint).Port.ToString(),
                        ((IPEndPoint)_listener.Server.LocalEndPoint).Address.MapToIPv4().ToString() +
                        ":" +
                        ((IPEndPoint)_listener.Server.LocalEndPoint).Port.ToString()
                    };
                Console.WriteLine($"connected to {nodeAddress}");
                SendAddress();
                GetData();
                RotateRoutingTable();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"ArgumentNullException: {e}");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
        }

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
            if (((IPEndPoint)_listener.Server.LocalEndPoint).Port == 8888)
            {
                SendData(_routingTable);
            }
            else
            {
                SendData("I get the connection.");
            }
            PrintRoutingTable();
        }

        private string GetAddress(TcpClient node)
        {
            var bytes = new Byte[256];
            string nodeAddress = null;

            int addressLength = _stream.Read(bytes, 0, bytes.Length);
            nodeAddress = Encoding.ASCII.GetString(bytes, 0, addressLength);

            return nodeAddress;
        }

        public void SendData(object data)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, data);
            var sizeofData = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfData = memoryStream.ToArray();
            _stream.Write(sizeofData, 0, 4);
            _stream.Write(byteArrayOfData, 0, byteArrayOfData.Length);
        }

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

        private void RotateRoutingTable()
        {
            foreach (var address in _routingTable)
            {
                if ((bool)address.Value[1])
                {
                    continue;
                }
                ReloadPort((string)address.Key);
            }
        }

        private void ReloadPort(string destinationAddress)
        {
            string[] clientAddress = _address[0].Split(":");
            IPAddress bindingIP = IPAddress.Parse(clientAddress[0]);
            _client.Close();
            _client.Dispose();
            Thread.Sleep(1000);
            Console.WriteLine("Refresh client node " + bindingIP + ":" + clientAddress[1]);
            _client = 
            new TcpClient
                (
                    new IPEndPoint(bindingIP, int.Parse(clientAddress[1]))
                );
            _client.Client.SetSocketOption
            (
                SocketOptionLevel.Socket, 
                SocketOptionName.ReuseAddress, 
                true
            );
            var clientThread = new Thread(ConnectToNode);
            clientThread.Start(destinationAddress);
        }

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