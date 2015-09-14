using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
    class Server
    {

        static Socket listenerSocket;
        static List<ClientData> _Clients;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on " + Packet.GetIP4Address());

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 1994);
            listenerSocket.Bind(ip);

            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();
        }

        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                _Clients.Add(new ClientData(listenerSocket.Accept()));
            }
        }

        public static void DATA_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] buffer;
            int readBytes;

            try
            {
                while (true)
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(buffer);

                    if (readBytes > 0)
                    {
                        Packet packet = new Packet(buffer);
                        DataManager(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("A client has disconnected");
            }
        }

        public static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case Packet.PacketType.Chat:
                    foreach (ClientData c in _Clients)
                    {
                        c.ClientSocket.Send(p.ToBytes());
                    }
                    break;
            }
        }
    }

    class ClientData
    {
        public Socket ClientSocket;
        public Thread ClientThread;
        public string id;

        public ClientData()
        {

            id = Guid.NewGuid().ToString();

            ClientThread = new Thread(Server.DATA_IN);
            ClientThread.Start(ClientSocket);

            SendRegPacket();
        }
        public ClientData(Socket ClientSocket)
        {

            this.ClientSocket = ClientSocket;

            id = Guid.NewGuid().ToString();

            ClientThread = new Thread(Server.DATA_IN);
            ClientThread.Start(ClientSocket);

            SendRegPacket();

        }

        public void SendRegPacket()
        {
            Packet p = new Packet(Packet.PacketType.Registration, "Server");
            p.GData.Add(id);
            ClientSocket.Send(p.ToBytes());
        }
    }
}
