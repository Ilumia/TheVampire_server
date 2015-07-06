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
        string errors = "";

        static void Main(string[] args)
        {
            new Server();
        }
    }
}
