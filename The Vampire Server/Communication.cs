using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    partial class Server {
        public Server()
        {
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
        public void DataInput() {
            String _data;
            Packet packet = new Packet();
            Console.WriteLine("Start up server");
            while (true) {
                _data = Console.ReadLine();
                if (_data.CompareTo("exit") == 0) { break; }
                else {
                    foreach (Socket _client in clientSet.Keys)
                    {
                        if (!_client.Connected)
                        {
                            DisconnectProc(_client);
                        }
                        else
                        {
                            packet.InitSendPacket((byte)67, Encoding.Unicode.GetBytes(_data));
                            SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
                            _sendArgs.SetBuffer(BitConverter.GetBytes(packet.Length), 0, 4);
                            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                            _sendArgs.UserToken = packet;

                            _client.SendAsync(_sendArgs);
                        }
                    }
                }
            }
        }

        public void SendDataToClient(byte type, string data, Socket client)
        {
            SendDataToClient(type, Encoding.Unicode.GetBytes(data), client);
        }
        private void SendDataToClient(byte type, byte[] data, Socket client)
        {
            Packet packet = new Packet();
            if (!client.Connected)
            {
                DisconnectProc(client);
            }
            else
            {
                packet.InitSendPacket(type, data);
                SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
                _sendArgs.SetBuffer(BitConverter.GetBytes(packet.Length), 0, 4);
                _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                _sendArgs.UserToken = packet;

                client.SendAsync(_sendArgs);
            }
        }
        private void DisconnectProc(Socket client)
        {
            if (clientSet[client].state == ClientState.ONROOM || clientSet[client].state == ClientState.ONGAME)
            {
                RoomInfo roomInfo = roomSet.Find(x => x.users.ContainsKey(client));
                RoomExitProc(Encoding.Unicode.GetBytes(roomInfo.roomNumber.ToString()), client);
                foreach (Socket _user in roomInfo.users.Keys)
                {
                    clientSet[_user] = new User(clientSet[_user].id, ClientState.ONLOBBY);
                }
            }
            clientSet.Remove(client);

        }
        private void Accept_Completed(object sender, SocketAsyncEventArgs e) {
            Socket _client = e.AcceptSocket;

            Packet packet = new Packet();
            SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.UserToken = packet;
            _receiveArgs.SetBuffer(packet.GetBuffer(), 0, 4);
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Recieve_Completed);
            _client.ReceiveAsync(_receiveArgs);
            clientSet.Add(_client, new User("", ClientState.ONACCESS));

            IPEndPoint clientIP = _client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("{0} is connected", clientIP);

            Socket _server = (Socket)sender;
            e.AcceptSocket = null;
            _server.AcceptAsync(e);
        }
        private void Recieve_Completed(object sender, SocketAsyncEventArgs e) {
            Socket _client = (Socket)sender;
            Packet packet = (Packet)e.UserToken;
            packet.InitRecvPacket(e.Buffer);
            IPEndPoint clientIP = _client.RemoteEndPoint as IPEndPoint;
            if (_client.Connected) {
                _client.Receive(packet.DataBuffer, packet.Length, SocketFlags.None);

                /* Classify Packet Type */
                try
                {
                    switch ((char)packet.Type)
                    {
                        case 'A':
                            LoginProc(packet.Data, _client);
                            break;
                        case 'B':
                            //LobbyInfoProc();
                            break;
                        case 'C':
                            RoomCreateProc(packet.Data, _client);
                            break;
                        case 'D':
                            RoomConfigProc(packet.Data, _client);
                            break;
                        case 'E':
                            SpecificRoomEnterProc(packet.Data, _client);
                            break;
                        case 'F':
                            RoomEnterProc(_client);
                            break;
                        case 'G':
                            RoomChatProc(packet.Data, _client);
                            break;
                        case 'H':
                            RoomExitProc(packet.Data, _client);
                            break;
                        case 'I':
                            SignUpProc(packet.Data, _client);
                            break;
                        case 'J':
                            ShowFriendsProc(_client);
                            break;
                        case 'K':
                            AddFriendProc(packet.Data, _client);
                            break;
                        case 'L':
                            DeleteFriendProc(packet.Data, _client);
                            break;
                    }
                }
                catch (Exception _e)
                {
                    Console.WriteLine(_e.Message);
                    Console.WriteLine(_e.StackTrace);
                    SendDataToClient((byte)122, new byte[0], _client);
                }

                Console.WriteLine("Recv type: {0}, from: {1}", (char)packet.Type, clientIP);
                Console.WriteLine("Data: {0}", Encoding.Unicode.GetString(packet.Data));
            }
            else
            {
                Console.WriteLine("{0} is disconnected", clientIP);
            }
            if (_client.Connected) {
                _client.ReceiveAsync(e);
            }
            else {
                DisconnectProc(_client);
            }
        }
        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket _client = (Socket)sender;
            Packet packet = (Packet)e.UserToken;
            _client.Send(packet.DataBuffer);

            IPEndPoint clientIP = _client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("Send: {0}, to: {1}", Encoding.Unicode.GetString(packet.Data), clientIP);
        }
    }
}
