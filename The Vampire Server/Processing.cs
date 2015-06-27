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
        private void LoginProc(byte[] data, Socket client)
        {
            DataManager dataManager = DataManager.GetDataManager();
            DataTable dataTable;

            string _tempString = Encoding.Unicode.GetString(data);
            string[] _tempStringArray = _tempString.Split(' ');
            string userid = _tempStringArray[0];
            string userpassword = _tempStringArray[1];

            string _sql = "SELECT userpassword FROM users WHERE userid = '" + userid + "'";
            dataTable = dataManager.ExecuteQuery(_sql);
            DataRow[] row = dataTable.Select("userpassword='" + userpassword + "'");
            if (row.Length == 1)
            {
                // 로그인 성공
                clientSet[client] = new User(userid, ClientState.ONLOGIN);
                Console.WriteLine("Login success: " + clientSet[client].id);
                SendDataToClient((byte)97, Encoding.Unicode.GetBytes("s"), client);
            }
            else
            {
                // 아이디가 없거나 비밀번호를 틀림
                Console.WriteLine("Fail to login");
                SendDataToClient((byte)97, Encoding.Unicode.GetBytes("f"), client);
            }
        }
        /* LobbyInfoProc: 보류 
        private void LobbyInfoProc(Socket client)
        {
            string data = "";
            for(int i=0; i<lobbySet.Count; i++) {
                if (lobbySet[i].isPublic)
                {
                    if (lobbySet[i].totalNumber < lobbySet[i].maximumNumber)
                    {
                        data += lobbySet[i].lobbyNumber.ToString() + " ";
                        data += lobbySet[i].totalNumber.ToString() + " ";
                        data += lobbySet[i].maximumNumber.ToString() + " ";
                    }
                }
            }
            SendDataToClient((byte)98, Encoding.Unicode.GetBytes(data), client);
        }
        */
        private void RoomCreateProc(byte[] data, Socket client)
        {
            string _tempString = Encoding.Unicode.GetString(data);
            string[] _tempStringArray = _tempString.Split(' ');
            int maximumNumber = Int32.Parse(_tempStringArray[0]);
            bool isPublic;
            if(_tempStringArray[1].Equals("t"))
                isPublic = true;
            else
                isPublic = false;

            RoomInfo roomInfo = new RoomInfo(client, clientSet[client].id, maximumNumber, isPublic);
            roomSet.Add(roomInfo);

            string _data = "";
            _data += roomInfo.roomNumber.ToString() + " ";
            _data += roomInfo.maximumNumber.ToString() + " ";
            if (roomInfo.isPublic)
                _data += "t";
            else
                _data += "f";
            SendDataToClient((byte)99, Encoding.Unicode.GetBytes(_data), client);
            clientSet[client] = new User(clientSet[client].id, ClientState.ONROOM);
        }
        private void RoomConfigProc(byte[] data, Socket client)
        {
            string _tempString = Encoding.Unicode.GetString(data);
            string[] _tempStringArray = _tempString.Split(' ');
            int _maximumNumber = Int32.Parse(_tempStringArray[0]);
            bool _isPublic;
            if (_tempStringArray[1].Equals("t"))
                _isPublic = true;
            else
                _isPublic = false;


            RoomInfo roomInfo = roomSet.Find(x => x.users[0].Equals(clientSet[client].id));
            int lobbyIndex = roomSet.IndexOf(roomInfo);
            roomInfo.maximumNumber = _maximumNumber;
            roomInfo.isPublic = _isPublic;
            roomSet[lobbyIndex] = roomInfo;

            RoomUpdateProc(roomInfo);
        }
        private void SpecificRoomEnterProc(byte[] data, Socket client)
        {
            int _roomNumber = Int32.Parse(Encoding.Unicode.GetString(data));
            RoomInfo roomInfo = roomSet.Find(x => x.roomNumber == _roomNumber);
            int lobbyIndex = roomSet.IndexOf(roomInfo);

            bool state = roomInfo.JoinRoom(client);
            string _data = "";
            if (state)
            {
                clientSet[client] = new User(clientSet[client].id, ClientState.ONROOM);
                RoomUpdateProc(roomInfo);
            }
            else
            {
                _data += "f";
                SendDataToClient((byte)100, Encoding.Unicode.GetBytes(_data), client);
            }
        }
        private void RoomEnterProc(Socket client)
        {
            foreach (RoomInfo roomInfo in roomSet)
            {
                if (roomInfo.isPublic && roomInfo.totalNumber < roomInfo.maximumNumber)
                {
                    SpecificRoomEnterProc(Encoding.Unicode.GetBytes(roomInfo.roomNumber.ToString()), client);
                }
            }
        }
        private void RoomUpdateProc(RoomInfo roomInfo)
        {
            string _data = "s ";
            _data += roomInfo.roomNumber.ToString() + " ";
            _data += roomInfo.totalNumber.ToString() + " ";
            _data += roomInfo.maximumNumber.ToString() + " ";
            if (roomInfo.isPublic)
                _data += "t ";
            else
                _data += "f ";
            foreach (string _userId in roomInfo.users)
            {
                if (_userId != null)
                {
                    _data += _userId + " ";
                }
            }
            foreach (Socket _client in roomInfo.clients)
            {
                if (_client != null)
                {
                    SendDataToClient((byte)101, Encoding.Unicode.GetBytes(_data), _client);
                }
            }
        }
    }
}

/* 
인젝션 방어: https://msdn.microsoft.com/ko-kr/library/ms161953.aspx
*/