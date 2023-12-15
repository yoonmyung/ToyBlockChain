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
        // Each node has a listener object.
        // Whenever another node sends connection, a node takes it by its listener object.
        // Also node can send connection by making client object
        // and connect it to destination node's listener object.
        // Each node stores other nodes' address which it has connected.
        // Routing table is Dictionary object. And it's composed of client as Key and listener as Value. 
        private Dictionary<int, int> _routingTable;
        private List<KeyValuePair<byte[], Action>> _stage;
        private BlockChain _blockChain;

        private const string _ip = "127.0.0.1";
        private const int _seedPort = 65000;

        public Node(int port, BlockChain blockChain)
        {
            SetListener(port);
            _stage = new List<KeyValuePair<byte[], Action>>();
            _routingTable = new Dictionary<int, int>();
            Address = (client: port + 1, listener: port);
            _blockChain = blockChain;
        }

        public (int client, int listener) Address
        {
            get;
            private set;
        }

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
                    Console.WriteLine($"Send routing table to {_routingTable[((IPEndPoint)client.Client.RemoteEndPoint).Port]}");
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
            else if (dataType.Contains("ArrayList"))
            {
                // Get data from transporting transaction(action).
                var dataArray = (ArrayList)data;
                var publicKey = new PublicKey((byte[])dataArray[0]);

                Console.WriteLine
                (
                    $"Get transaction from " +
                    $"{((IPEndPoint)client.Client.RemoteEndPoint).Port}"
                );
                if (publicKey.Verify((byte[])dataArray[2], (byte[])dataArray[1]))
                {
                    var serializedAction = (byte[])dataArray[3];
                    var componentsofAction =
                        (Dictionary<string, object>)
                        ByteArrayConverter.DeSerialize(serializedAction);
                    var position =
                        componentsofAction["payload_x"] is null ?
                        null :
                        new TicTacToeGame.Position
                        (
                            (int)componentsofAction["payload_x"],
                            (int)componentsofAction["payload_y"]
                        );
                    _stage.Add
                    (
                        new KeyValuePair<byte[], Action>
                        (
                            (byte[])dataArray[2],
                            new Action
                            (
                                txNonce: (long)componentsofAction["txNonce"],
                                signer: new Address((byte[])componentsofAction["signer"]),
                                payload: position,
                                signature: (byte[])dataArray[1]
                            )
                        )
                    );
                }
                else
                {
                    Console.WriteLine("But fail to verify transaction");
                }
            }
            else if (dataType.Contains("KeyValuePair"))
            {
                // Get data from transporting block.
                Console.WriteLine
                (
                    $"Get block from " +
                    $"{((IPEndPoint)client.Client.RemoteEndPoint).Port}"
                );
                var blockPair = (KeyValuePair<byte[], byte[]>)data;
                var componentsofBlock =
                    (Dictionary<string, object>)
                    ByteArrayConverter.DeSerialize(blockPair.Value);
                byte[] previousHash =
                    componentsofBlock["previousHash"] is null ?
                    null :
                    (byte[])componentsofBlock["previousHash"];
                var blockHeader =
                    new BlockHeader
                    (
                        index: (long)componentsofBlock["index"],
                        previousHash: previousHash,
                        timeStamp: (DateTimeOffset)componentsofBlock["timeStamp"],
                        nonce: new Nonce((byte[])componentsofBlock["nonce"]),
                        difficulty: (long)componentsofBlock["difficulty"]
                    );
                var emptyBlock = new Block(blockHeader: blockHeader, action: null);
                if (emptyBlock.IsValid())
                {
                    var action = _stage.Count > 0 ? _stage[0].Value : null;
                    if (!(action is null)) 
                    {
                        _stage.RemoveAt(0);
                    }
                    var block =
                        new Block
                        (
                            blockHeader: emptyBlock.BlockHeader,
                            action: action
                        );
                    _blockChain.AddBlock(block);
                }
                else
                {
                    Console.WriteLine("But fail to validate block");
                }
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

        public void Send(int destinationPort, object data)
        {
            var client = SetClient();

            try
            {
                client.Connect(_ip, destinationPort);
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

                return;
            }
            NetworkStream stream = GetStream(destinationPort, client);
            if (stream is null)
            {
                Console.WriteLine("Fail to connect to " + destinationPort);
            }
            else
            {
                Console.WriteLine($"Client: Connected to {destinationPort}");
                SendData(data, stream);
            }
        }

        private NetworkStream GetStream
            (object destinationAddress, TcpClient client)
        {
            var destinationPort = (int)destinationAddress;

            try
            {
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
                    // Node which you try to connect no longer connects to you.
                    _routingTable.Remove(destinationPort + 1);
                    PrintRoutingTable();
                }
                else
                {
                    Console.WriteLine($"SocketException: {e}");
                }

                return null;
            }
        }

        private void DisconnectClient(TcpClient client, NetworkStream stream)
        {
            if (!(stream is null))
            {
                stream.Flush();
                stream.Close();
            }
            client.Close();
            client.Dispose();
        }

        private void PutAddressToRoutingtable(string address)
        {
            string[] seperatedAddress = address.Split(",");

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

        private void SendData(object data, NetworkStream stream)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, data);
            var sizeofData = BitConverter.GetBytes(memoryStream.Length);
            byte[] byteArrayOfData = memoryStream.ToArray();
            stream.Write(sizeofData, 0, 4);
            stream.Write(byteArrayOfData, 0, byteArrayOfData.Length);
        }

        public object GetData(NetworkStream stream)
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

        public void RotateRoutingTable(object data)
        {
            while (_routingTable.Count < 1) ;
            foreach (var address in _routingTable)
            {
                if (address.Key.Equals(Address.client))
                {
                    if (!(data.GetType().FullName.Contains("KeyValuePair")))
                    {
                        continue;
                    }
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
                    ($"Client: {address.Key}, Listener: {address.Value}");
            }
            Console.WriteLine();
        }
    }
}