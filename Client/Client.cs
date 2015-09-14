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

namespace Client
{
    class Client
    {
        public static Socket master;
        public static string name;
        public static string id;

        static void Main(string[] args)
        {
            Console.Write("Enter your name: ");
            name = Console.ReadLine();

            A: Console.Clear();
            Console.Write("Enter Host IP Address: ");
            string ip = Console.ReadLine();

            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 1994);

            try
            {
                master.Connect(ipe);
            }
            catch
            {
                Console.WriteLine("Could not connect to host!");
                Thread.Sleep(1000);
                goto A;
            }

            Thread t = new Thread(DATA_IN);
            t.Start();

            while (true)
            {
                Console.Write("::>");
                string input = Console.ReadLine();

                Packet p = new Packet(Packet.PacketType.Chat, id);

                p.GData.Add(name);
                p.GData.Add(input);
                master.Send(p.ToBytes());
            }
        }

        static void DATA_IN()
        {
            byte[] buffer;
            int readBytes;

            try
            {
                while (true)
                {
                    buffer = new byte[master.SendBufferSize];
                    readBytes = master.Receive(buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection to server lost!");
                Console.Read();
                Environment.Exit(0);
            }
        }

        static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case Packet.PacketType.Registration:
                    id = p.GData[0];
                    break;

                case Packet.PacketType.Chat:
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Blue;

                    Console.WriteLine(p.GData[0] + ": " + p.GData[1]);
                    Console.ForegroundColor = c;
                    break;
            }
        }
    }
}
