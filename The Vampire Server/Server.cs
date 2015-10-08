using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;

namespace The_Vampire_Server
{
    partial class Server
    {
        Dictionary<Socket, User> clientSet = new Dictionary<Socket, User>();
        List<RoomInfo> roomSet = new List<RoomInfo>();
        static Item item = new Item();
        string errors = "";

        public Server()
        {
            ReadItemSet();
            Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint _ipep = new IPEndPoint(IPAddress.Any, 8000);
            _server.Bind(_ipep);
            _server.Listen(20);
            Console.WriteLine("Handle of server process: " + _server.Handle.ToInt32());

            SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

            _server.AcceptAsync(_args);

            DataInput();
        }

        static void Main(string[] args)
        {
            Server server = new Server();

        }
    }
}
