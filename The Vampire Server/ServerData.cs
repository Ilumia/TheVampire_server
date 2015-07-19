using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    public struct Message
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
            set { this.data = value; }
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
    public enum PlayerState { UNSET, TURN_ON, TURN_OFF, DROPPED };
    public enum PlayerJob { UNSET, VAMPIRE };
    public struct Player
    {
        public string id;
        public PlayerJob job;
        public PlayerState state;
        public int item1;
        public int item2;
        public int item3;
        public bool isAI;
        public Player(string id)
        {
            this.id = id;
            job = PlayerJob.UNSET;
            state = PlayerState.UNSET;
            item1 = 0;
            item2 = 0;
            item3 = 0;
            isAI = false;
        }
        public void InitPlayer(PlayerJob job, bool isAI) {
            this.job = job;
            this.isAI = isAI;
            /*  초기아이템 및 상태정의
            switch (job)
            {
                case PlayerJob.:
                    break;
                case PlayerJob.:
                    break;
                case PlayerJob.:
                    break;
            }
            */
        }
    }

    public struct RoomInfo
    {
        public int roomNumber;
        private static int nextRoomNumber = 0;
        public int totalNumber;
        public int maximumNumber;
        public bool isPublic;
        public Dictionary<Socket, Player> users;
        public Socket owner;
        public int roomState;   // -1: 시작 전, 0: 게임종료, 1이상의 양수: 진행회차

        public RoomInfo(Socket _owner, string _ownerId, int _maximumNumber, bool _isPublic)
        {
            roomNumber = nextRoomNumber;
            nextRoomNumber++;
            users = new Dictionary<Socket, Player>();
            users.Add(_owner, new Player(_ownerId));
            owner = _owner;
            totalNumber = 1;
            maximumNumber = _maximumNumber;
            isPublic = _isPublic;
            roomState = -1;
        }

        public bool ConfigRoom(int _maximumNumber, bool _isPublic)
        {
            if (totalNumber > _maximumNumber)
            {
                return false;
            }
            maximumNumber = _maximumNumber;
            isPublic = _isPublic;
            return true;
        }

        public bool JoinRoom(Socket client, string clientid)
        {
            if (totalNumber < maximumNumber)
            {
                users.Add(client, new Player(clientid));
                totalNumber++;
            }
            return true;
        }
        public bool ExitRoom(Socket client)
        {
            users.Remove(client);
            totalNumber--;
            return true;
        }
    }
}
