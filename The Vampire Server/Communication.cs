using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    partial class Server {

        public void DataInput() {
            String _data;
            Console.WriteLine("Start up server");

            while (true) {
                _data = Console.ReadLine();
                if (_data.CompareTo("exit") == 0) { break; }
            }
        }

        public void SendDataToClient(byte type, string data, Socket client)
        {
            SendDataToClient(type, Encoding.Unicode.GetBytes(data), client);
        }
        public void SendDataToClient(byte type, byte[] data, Socket client)
        {
            Message packet = new Message();
            if (!client.Connected)
            {
                Console.WriteLine(client.Handle + " is disconnected");
                DisconnectProc(client);
            }
            else
            {
                //byte[] compressedData = CompressToBytes(data);
                packet.InitSendPacket(type, data);
                SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
                _sendArgs.SetBuffer(BitConverter.GetBytes(packet.Length), 0, 4);
                _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                _sendArgs.UserToken = packet;

                client.SendAsync(_sendArgs);
            }
        }
        private byte[] CompressToBytes(string data)
        {
            byte[] _data = Encoding.Unicode.GetBytes(data);
            return _data;
        }
        private byte[] CompressToBytes(byte[] data)
        {
            string tData = Encoding.Unicode.GetString(data);
            byte[] _data = Encoding.Unicode.GetBytes(tData);
            return _data;
        }
        private string DecompressToUnicode(byte[] data)
        {
            string _data = Encoding.Unicode.GetString(data);
            return _data;
        }
        private byte[] DecompressToBytes(byte[] data)
        {
            string __data = Encoding.Unicode.GetString(data);
            byte[] _data = Encoding.Unicode.GetBytes(__data);
            return _data;
        }
        private RoomInfo FindRoomFromSocket(Socket client)
        {
            return roomSet.Find(x => x.users.ContainsKey(client));
        }
        private void DisconnectProc(Socket client)
        {
            if (clientSet[client].state == ClientState.ONROOM || clientSet[client].state == ClientState.ONGAME)
            {
                RoomInfo roomInfo = roomSet.Find(x => x.users.ContainsKey(client));
                try {
                    RoomExitProc(Encoding.Unicode.GetBytes(roomInfo.roomNumber.ToString()), client);
                } catch(Exception e)
                {
                    errors += e.Message + "\n";
                    errors += e.StackTrace + "\n\n";
                }
                clientSet[client] = new User(clientSet[client].id, ClientState.ONLOBBY);
                Console.WriteLine(clientSet[client].id + " is out of room!!!");
            }
            Console.WriteLine(clientSet[client].id + " is disconnected!!!");
            clientSet.Remove(client);
            
        }
        private void Accept_Completed(object sender, SocketAsyncEventArgs e) {
            Socket _client = e.AcceptSocket;

            Message packet = new Message();
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
            Message packet = (Message)e.UserToken;
            packet.InitRecvPacket(e.Buffer);
            IPEndPoint clientIP = _client.RemoteEndPoint as IPEndPoint;
            if (_client.Connected) {
                _client.Receive(packet.DataBuffer, packet.Length, SocketFlags.None);

                byte[] data = null;
                if (packet.Length > 0)
                {
                    if(packet.Data == null)
                    {
                        return;
                    }
                    data = DecompressToBytes(packet.Data);
                }

                /* Classify Packet Type */
                try
                {
                    switch ((char)packet.Type)
                    {
                        case 'A':
                            LoginProc(data, _client);
                            break;
                        case 'B':
                            LobbyInfoProc(_client);
                            break;
                        case 'C':
                            RoomCreateProc(data, _client);
                            break;
                        case 'D':
                            RoomConfigProc(data, _client);
                            break;
                        case 'E':
                            SpecificRoomEnterProc(data, _client);
                            break;
                        case 'F':
                            RoomEnterProc(_client);
                            break;
                        case 'G':
                            RoomChatProc(data, _client);
                            break;
                        case 'H':
                            RoomExitProc(data, _client);
                            break;
                        case 'I':
                            SignUpProc(data, _client);
                            break;
                        case 'J':
                            ShowFriendsProc(_client);
                            break;
                        case 'K':
                            AddFriendProc(data, _client);
                            break;
                        case 'L':
                            DeleteFriendProc(data, _client);
                            break;
                        case 'M':
                            CardSubmitProc(data, _client);
                            break;

                        case 'V':
                            ItemListProc(_client);
                            break;
                        case 'Y':
                            MonitorProc(data, _client);
                            break;
                        case 'Z':
                            NoticeProc(data, _client);
                            break;
                        default:
                            return;
                            break;
                    }
                }
                catch (Exception _e)
                {
                    errors += _e.Message + "\n";
                    errors += _e.StackTrace + "\n\n";
                    Console.WriteLine(_e.Message);
                    Console.WriteLine(_e.StackTrace);
                    SendDataToClient((byte)122, new byte[0], _client);
                }

                Console.WriteLine("===================================");
                Console.WriteLine("Recv Type: {0}, from: {1}", (char)packet.Type, clientIP);
                Console.WriteLine("Recv Data: {0}", Encoding.Unicode.GetString(data));
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
            Message packet = (Message)e.UserToken;
            //packet.Data = CompressToBytes(packet.Data);
            _client.Send(packet.DataBuffer);

            IPEndPoint clientIP = _client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("Send Type: {0}, to: {1}", (char)packet.Type, clientIP);
            Console.WriteLine("Send Data: {0}", Encoding.Unicode.GetString(packet.Data));
        }
    }
}
