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
        // _routingTable = 
        // {
        //     peer's address of listener, 
        //     [ peer's address of client, is it connected with this node? ] 
        // }
        private Dictionary<string, string> _routingTable;
        private TcpClient _client;
        private TcpListener _listener;
        private NetworkStream _stream;
        private string[] _address;

        public Node(bool isSeed, int port)
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

        private void Listen()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");
                var node = _listener.AcceptTcpClient();
                _stream = node.GetStream();
                PutAddressToRoutingtable(node);
                SendData(_routingTable);
            }
        }

        private bool ConnectToNode(object destinationAddress)
        {
            var neighborNode = (string)destinationAddress;
            var seperatedAddress = neighborNode.Split(":");

            try
            {
                _stream = _client.GetStream();
                return true;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"ArgumentNullException: {e}");
                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode.ToString().Equals("ConnectionRefused"))
                {
                    // Node you try to connect no longer connected to you.
                    _routingTable.Remove(neighborNode);
                    PrintRoutingTable();
                }
                else
                {
                    Console.WriteLine($"SocketException: {e}");
                }

                return false;
            }
        }

        public void DisconnectToNode()
        {
            _client.Close();
            _client.Dispose();
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

        private string GetAddress(TcpClient node)
        {
            var bytes = new Byte[256];
            string nodeAddress = null;

            int addressLength = _stream.Read(bytes, 0, bytes.Length);
            nodeAddress = Encoding.ASCII.GetString(bytes, 0, addressLength);

            return nodeAddress;
        }

        private void PutAddressToRoutingtable(object client)
        {
            var node = (TcpClient)client;

            string nodeAddress = GetAddress(node);
            string[] seperatedAddress = nodeAddress.Split(",");
            Console.WriteLine($"Connected client: {seperatedAddress[0]}");
            if (!(_routingTable.ContainsKey(seperatedAddress[1])))
            {
                _routingTable.Add(seperatedAddress[1], seperatedAddress[0]);
            }

            PrintRoutingTable();
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
                _routingTable = (Dictionary<string, string>)data;
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
                if (address.Value.Equals(_address[0]))
                {
                    continue;
                }
                ReloadPort((string)address.Key);
            }
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
        public void StartConnection(string destinationAddress)
        {
            string[] clientAddress = _address[0].Split(":");

            SetClient(clientAddress[0], int.Parse(clientAddress[1]));
            if (!ConnectToNode(destinationAddress))
            {
                Console.WriteLine("Fail to connect to " + destinationAddress);
            }
            else
            {
                SendAddress();
                GetData();
            }
            DisconnectToNode();
        }
    }
}