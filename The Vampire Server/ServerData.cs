using System;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    public struct Packet
    {
        private byte[] data;
        private int length;

        public byte Type { get { return data[0]; } }
        public byte[] Data
        {
            get
            {
                byte[] temp = new byte[Length - 1];
                for (int i = 0; i < Length - 1; i++) { temp[i] = data[i + 1]; }
                return temp;
            }
        }
        public int Length { get { return length; } }
        public void InitSendPacket(byte type, byte[] _data)
        {
            length = _data.Length + sizeof(byte);
            data = new byte[Length];
            data[0] = type;
            for (int i = 0; i < _data.Length; i++) { data[i + 1] = _data[i]; }
        }
        public void InitRecvPacket(byte[] _data)
        {
            if (_data.Length < 4)
                return;
            length = BitConverter.ToInt32(_data, 0);
            data = new byte[Length + 1];
        }
        public byte[] DataBuffer { get { return data; } }
        public byte[] GetBuffer()
        {
            return new byte[4];
        }
    }

    public enum ClientState { ONACCESS, ONLOGIN, ONLOBBY, ONROOM, ONGAME };
    public struct User
    {
        public string id;
        public ClientState state;

        public User(string _id, ClientState _state)
        {
            id = _id;
            state = _state;
        }
    }

    public struct RoomInfo
    {
        public int roomNumber;
        private static int nextRoomNumber = 0;
        public int totalNumber;
        public int maximumNumber;
        public bool isPublic;
        public string[] users;
        public Socket[] clients;

        public RoomInfo(Socket director, string directoId, int _maximumNumber, bool _isPublic)
        {
            roomNumber = nextRoomNumber;
            nextRoomNumber++;
            clients = new Socket[_maximumNumber];
            clients[0] = director;
            users = new string[_maximumNumber];
            users[0] = directoId;
            totalNumber = 1;
            maximumNumber = _maximumNumber;
            isPublic = _isPublic;

            PrintLobbyState();
        }

        public bool JoinRoom(Socket client)
        {
            if (totalNumber < maximumNumber)
            {
                for (int i = 0; i < maximumNumber; i++)
                {
                    if (clients[i] == null)
                    {
                        clients[i] = client;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ExitRoom(Socket client)
        {
            for (int i = 0; i < totalNumber; i++)
            {
                if (clients[i] == client)
                {
                    clients[i] = null;
                    return true;
                }
            }
                return false;
        }

        private void PrintLobbyState()
        {
            Console.WriteLine("no." + roomNumber + ", next: " + nextRoomNumber + ", director: " + clients[0].RemoteEndPoint);
        }
    }
}
