using PracticeBlockChain.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace PracticeBlockChain.Network
{
    public class Node
    {
        // _routingTable = { peer's address of listener, peer's address of client }
        private Dictionary<string, string> _routingTable;
        private TcpListener _listener;
        private string[] _address;
        private List<byte[]> _stage;

        public Node(bool isSeed, int port)
        {
            string seedNodeAddress = "127.0.0.1:65000";

            SetListener("127.0.0.1", port);
            _stage = new List<byte[]>();
            _address =
                new string[]
                {
                        "127.0.0.1:" + (port + 1).ToString(), "127.0.0.1:" + port.ToString()
                };
            if (!isSeed)
            {
                // It's peer node.
                string[] clientAddress = _address[0].Split(":");
                TcpClient client = SetClient(clientAddress[0], int.Parse(clientAddress[1]));
                NetworkStream stream = GetStream(seedNodeAddress, client);
                if (stream is null)
                {
                    Console.WriteLine("Fail to connect to " + seedNodeAddress);
                }
                else
                {
                    SendData(string.Format(_address[0] + "," + _address[1]), stream);
                    Console.WriteLine($"client sent data {string.Format(_address[0] + "," + _address[1]).GetType().FullName}!");
                }
                _routingTable = (Dictionary<string, string>)GetData(stream).Result;
                PrintRoutingTable();
                DisconnectToNode(client, stream);
            }
        }

        public string[] Address
        {
            get
            {
                return _address;
            }
        }

        public Dictionary<string, string> RoutingTable
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
            listeningThread.Priority = ThreadPriority.Highest;
            listeningThread.Start();
        }

        private TcpClient SetClient(string IP, int port)
        {
            var client = new TcpClient(new IPEndPoint(IPAddress.Parse(IP), port));
            client.Client.SetSocketOption
            (
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true
            );

            return client;
        }

        public void Listen()
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                Task.Factory.StartNew(ListenNodeAsync, client);
            }
        }

        private async void ListenNodeAsync(object obj)
        {
            var client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            var data = await GetData(stream);
            string dataType = data.GetType().FullName;

            if (dataType.Contains("String"))
            {
                // Listener gets address from another node.
                PutAddressToRoutingtable((string)data);
                if (_address[1].Split(":")[1].Contains("65000"))
                {
                    SendData(_routingTable, stream);
                }
            }
            else if (dataType.Contains("ArrayList"))
            {
                // Data from transporting transaction(action)
                var dataArray = (ArrayList)data;
                var publicKey = new PublicKey((byte[])dataArray[0]);

                if (publicKey.Verify((byte[])dataArray[2], (byte[])dataArray[1]))
                {
                    _stage.Add((byte[])data);
                    Console.WriteLine($"Get transaction from 127.0.0.1:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");
                }
            }
            else if (dataType.Contains("Block"))
            {
                // Data from transporting block
            }
            DisconnectToNode(client, stream);
        }

        private NetworkStream GetStream(object destinationAddress, TcpClient client)
        {
            var neighborNode = (string)destinationAddress;
            var seperatedAddress = neighborNode.Split(":");

            try
            {
                client.Connect(seperatedAddress[0], int.Parse(seperatedAddress[1]));
                Console.WriteLine($"Connected to {neighborNode}");
                
                return client.GetStream();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"ArgumentNullException: {e}");
                return null;
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

                return null;
            }
        }

        private void DisconnectToNode(TcpClient client, NetworkStream stream)
        {
            stream.Close();
            client.Close();
            client.Dispose();
        }

        private void PutAddressToRoutingtable(string address)
        {
            string[] seperatedAddress = address.Split(",");
            Console.WriteLine($"Connected client: {seperatedAddress[0]}");
            if (!(_routingTable.ContainsKey(seperatedAddress[1])))
            {
                _routingTable.Add(seperatedAddress[1], seperatedAddress[0]);
            }

            PrintRoutingTable();
        }

        private async void SendData(object data, NetworkStream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, data);
            var sizeofData = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfData = memoryStream.ToArray();
            stream.Write(sizeofData, 0, 4);
            stream.Write(byteArrayOfData, 0, byteArrayOfData.Length);
        }

        public async Task<object> GetData(NetworkStream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            byte[] sizeofDataAsByte = new byte[4];

            stream.Read(sizeofDataAsByte, 0, sizeofDataAsByte.Length);
            var sizeofData = BitConverter.ToInt32(sizeofDataAsByte, 0);
            var dataAsByte = new byte[sizeofData];
            stream.Read(dataAsByte, 0, dataAsByte.Length);

            var memoryStream = new MemoryStream(dataAsByte);
            memoryStream.Position = 0;
            var data = binaryFormatter.Deserialize(memoryStream);

            return data;
        }

        // Transport transaction to all of nodes from routing table.
        public async void RotateRoutingTable(object data)
        {
            string[] clientAddress = _address[0].Split(":");

            foreach (var address in _routingTable)
            {
                if (address.Value.Equals(_address[0]))
                {
                    continue;
                }
                if (data.GetType().FullName.Contains("ArrayList"))
                {
                    await Task.Delay(2000);
                    Console.WriteLine("Send transaction");
                }
                TcpClient client = SetClient(clientAddress[0], int.Parse(clientAddress[1]));
                NetworkStream stream = GetStream(address.Key, client);
                if (stream is null)
                {
                    Console.WriteLine("Fail to connect to " + address.Key);
                }
                else
                {
                    SendData(data, stream);
                    Console.WriteLine($"client sent data {data.GetType().FullName}!");
                }
                DisconnectToNode(client, stream);
            }
        }

        private void PrintRoutingTable()
        {
            Console.WriteLine("\n<Routing table>");
            foreach (var address in _routingTable)
            {
                Console.WriteLine($"Client: {address.Value}, Listener: {address.Key}");
            }
            Console.WriteLine();
        }
    }
}