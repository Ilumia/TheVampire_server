using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Data;

namespace The_Vampire_Server
{
    partial class Server
    {
        static Server server = null;
        Timer timer = new System.Timers.Timer();
        Dictionary<Socket, User> clientSet = new Dictionary<Socket, User>();
        List<RoomInfo> roomSet = new List<RoomInfo>();
        static Item item = new Item();
        string errors = "";

        static void Main(string[] args)
        {
            server = new Server();
        }

        public Server()
        {
            server = this;
            ReadItemSet();
            Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint _ipep = new IPEndPoint(IPAddress.Any, 50005);
            _server.Bind(_ipep);
            _server.Listen(20);
            Console.WriteLine("Handle of server process: " + _server.Handle.ToInt32());

            SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);

            _server.AcceptAsync(_args);
            SetTimer();
            DataInput();
        }
        public static Server GetInstance()
        {
            return server;
        }

        void SetTimer()
        {
            timer.Interval = 100;
            timer.Elapsed += new ElapsedEventHandler(Timer);
            timer.Start();
        }

        void Timer(object sender, EventArgs e)
        {
            foreach(RoomInfo roomInfo in roomSet)
            {
                roomInfo.TimerUpdate();
            }
        }

    }
}
