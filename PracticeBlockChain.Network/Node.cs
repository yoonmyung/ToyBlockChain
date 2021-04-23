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
        private TcpListener _listener;
        private string[] _address;

        public Node(bool isSeed, int port)
        {
            string seedNodeAddress = "127.0.0.1:65000";

            SetListener("127.0.0.1", port);
            if (!isSeed)
            {
                _address =
                    new string[]
                    {
                        "127.0.0.1:" + (port + 1).ToString(), "127.0.0.1:" + port.ToString()
                    };
                // It's peer node.
                StartConnection
                (
                    destinationAddress: seedNodeAddress,
                    dataToSend: string.Format(_address[0] + "," + _address[1])
                );
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
        }

        private void Listen()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");
                var node = _listener.AcceptTcpClient();
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
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine($"ArgumentNullException: {e}");
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

        {
        }

        private void PutAddressToRoutingtable(object client)
        {
            var node = (TcpClient)client;

            string nodeAddress = (string)GetData();
            string[] seperatedAddress = nodeAddress.Split(",");
            Console.WriteLine($"Connected client: {seperatedAddress[0]}");
            if (!(_routingTable.ContainsKey(seperatedAddress[1])))
            {
                _routingTable.Add(seperatedAddress[1], seperatedAddress[0]);
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
                if (address.Value.Equals(_address[0]))
                {
                    continue;
                }
                StartConnection
                (
                    destinationAddress: address.Key,
                    dataToSend: string.Format(_address[0] + "," + _address[1])
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
            string[] clientAddress = _address[0].Split(":");

            SetClient(clientAddress[0], int.Parse(clientAddress[1]));
            if (!ConnectToNode(destinationAddress))
            {
                Console.WriteLine("Fail to connect to " + destinationAddress);
            }
            else
            {
                SendData(dataToSend);
                _routingTable = (Dictionary<string, string>)GetData();
            }
            DisconnectToNode();
        }
    }
}