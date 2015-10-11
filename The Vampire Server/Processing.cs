using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.IO;

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
                SendDataToClient((byte)97, Encoding.Unicode.GetBytes(item.itemVersion.ToString()), client);
            }
            else
            {
                // 아이디가 없거나 비밀번호를 틀림
                SendDataToClient((byte)97, Encoding.Unicode.GetBytes("f"), client);
            }
        }
        private void LobbyInfoProc(Socket client)
        {
            string data = "";
            for(int i=0; i<roomSet.Count; i++) {
                if (roomSet[i].isPublic)
                {
                    if (roomSet[i].totalNumber < roomSet[i].maximumNumber)
                    {
                        data += roomSet[i].roomNumber.ToString() + " ";
                        data += roomSet[i].totalNumber.ToString() + " ";
                        data += roomSet[i].maximumNumber.ToString() + " ";
                    }
                }
            }
            SendDataToClient((byte)98, Encoding.Unicode.GetBytes(data), client);
        }
        private void RoomCreateProc(byte[] data, Socket client)
        {
            string _tempString = Encoding.Unicode.GetString(data);
            int maximumNumber = 4;
            bool isPublic;
            if (_tempString.Equals("t"))
                isPublic = true;
            else
                isPublic = false;

            RoomInfo roomInfo = new RoomInfo(client, clientSet[client].id, maximumNumber, isPublic);
            roomSet.Add(roomInfo);

            string _data = "s ";
            if (roomInfo.isPublic)
                _data += "t";
            else
                _data += "f";
            SendDataToClient((byte)99, Encoding.Unicode.GetBytes(_data), client);
            clientSet[client] = new User(clientSet[client].id, ClientState.ONROOM);
            RoomUpdateProc(roomInfo);
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

            RoomInfo roomInfo = roomSet.Find(x => x.owner.Equals(client));
            roomInfo.ConfigRoom(_maximumNumber, _isPublic);

            RoomUpdateProc(roomInfo);
        }
        private void SpecificRoomEnterProc(byte[] data, Socket client)
        {
            int _roomNumber = Int32.Parse(Encoding.Unicode.GetString(data));
            RoomInfo roomInfo = roomSet.Find(x => x.roomNumber == _roomNumber);

            bool state = roomInfo.JoinRoom(client, clientSet[client].id);
            
            if (state)
            {
                clientSet[client] = new User(clientSet[client].id, ClientState.ONROOM);
                RoomUpdateProc(roomInfo);
            }
            else
            {
                SendDataToClient((byte)100, new byte[0], client);
            }
        }
        private void RoomEnterProc(Socket client)
        {
            foreach (RoomInfo roomInfo in roomSet)
            {
                if (roomInfo.isPublic && roomInfo.totalNumber < roomInfo.maximumNumber)
                {
                    SpecificRoomEnterProc(Encoding.Unicode.GetBytes(roomInfo.roomNumber.ToString()), client);
                    return;
                }
            }
            SendDataToClient((byte)100, new byte[0], client);
        }
        private void RoomUpdateProc(RoomInfo roomInfo)
        {
            string _data = "";
            _data += roomInfo.roomNumber.ToString() + " ";
            _data += roomInfo.totalNumber.ToString() + " ";
            _data += roomInfo.maximumNumber.ToString() + " ";
            if (roomInfo.isPublic)
                _data += "t ";
            else
                _data += "f ";
            foreach (Player _player in roomInfo.users.Values)
            {
                if (true /*_player.id != null*/)
                {
                    _data += _player.id + " ";
                }
            }
            Console.WriteLine(_data);
            foreach (Socket _client in roomInfo.users.Keys)
            {
                if (_client != null)
                {
                    SendDataToClient((byte)101, Encoding.Unicode.GetBytes(_data), _client);
                }
            }
        }
        private void RoomChatProc(byte[] data, Socket client) {
            string chat = Encoding.Unicode.GetString(data);
            string separator = "\r\n";
            string _data = clientSet[client].id + separator + chat;

            RoomInfo roomInfo = roomSet.Find(x => x.users.ContainsKey(client));

            foreach (Socket _client in roomInfo.users.Keys)
            {
                if (_client != null)
                {
                    SendDataToClient((byte)102, Encoding.Unicode.GetBytes(_data), _client);
                }
            }
        }
        private void RoomExitProc(byte[] data, Socket client)
        {
            Console.WriteLine(clientSet[client].id + " is exit");
            int _roomNumber = Int32.Parse(Encoding.Unicode.GetString(data));
            RoomInfo roomInfo = roomSet.Find(x => x.roomNumber == _roomNumber);
            roomInfo = roomSet.Find(x => x.users.ContainsKey(client));
            bool state = roomInfo.ExitRoom(client);
            if (roomInfo.owner == client)
            {
                foreach (Socket _client in roomInfo.users.Keys)
                {
                    SendDataToClient((byte)101, Encoding.Unicode.GetBytes("f"), _client);
                    clientSet[_client] = new User(clientSet[_client].id, ClientState.ONLOBBY);
                }
                for(int i=0; i < roomSet.Count; i++)
                {
                    if(roomSet[i].roomNumber == roomInfo.roomNumber)
                    {
                        roomSet.RemoveAt(i);
                    }
                }
                return;
            }
            if (state)
            {
                clientSet[client] = new User(clientSet[client].id, ClientState.ONLOBBY);
                RoomUpdateProc(roomInfo);
            }
        }
        private void SignUpProc(byte[] data, Socket client)
        {
            DataManager dataManager = DataManager.GetDataManager();

            string _tempString = Encoding.Unicode.GetString(data);
            string[] _tempStringArray = _tempString.Split(' ');
            string userid = _tempStringArray[0];
            string userpassword = _tempStringArray[1];

            string _sql = "INSERT INTO users VALUES ('" + userid + "', '" + userpassword + "', NULL)";
            int result = dataManager.ExecuteUpdate(_sql);
            if (result == 1)
                SendDataToClient((byte)103, Encoding.Unicode.GetBytes("s"), client);
            else
                SendDataToClient((byte)103, Encoding.Unicode.GetBytes("f"), client);
        }
        private void ShowFriendsProc(Socket client)
        {
            DataManager dataManager = DataManager.GetDataManager();
            DataTable dataTable;

            string _sql = "SELECT friendid FROM friends WHERE userid = '" + clientSet[client].id + "'";
            dataTable = dataManager.ExecuteQuery(_sql);

            string _data = "";
            foreach (DataRow item in dataTable.Rows)
            {
                _data += item.ItemArray[0].ToString() + " ";
            }
            SendDataToClient((byte)104, Encoding.Unicode.GetBytes(_data), client);
        }
        private void AddFriendProc(byte[] data, Socket client)
        {
            DataManager dataManager = DataManager.GetDataManager();
            string friendid = Encoding.Unicode.GetString(data);
            if (friendid.Equals(clientSet[client].id))
            {
                SendDataToClient((byte)105, Encoding.Unicode.GetBytes("f 1"), client);
                return;
            }

            string _sql = "INSERT INTO friends VALUES ('" + clientSet[client].id + "', '" + friendid + "')";

            int result = dataManager.ExecuteUpdate(_sql);
            if (result == 1)
            {
                SendDataToClient((byte)105, Encoding.Unicode.GetBytes("t"), client);
            }
            else
            {
                SendDataToClient((byte)105, Encoding.Unicode.GetBytes("f"), client);
            }
        }
        private void DeleteFriendProc(byte[] data, Socket client)
        {
            DataManager dataManager = DataManager.GetDataManager();
            string friendid = Encoding.Unicode.GetString(data);

            string _sql = "DELETE FROM friends WHERE userid = '" + clientSet[client].id + "' and friendid = '" + friendid + "'";

            int result = dataManager.ExecuteUpdate(_sql);
        }

        //Disconnect 시 종료처리

        private void ItemListProc(Socket client)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "/itemset";
            string data = File.ReadAllText(path, Encoding.Default);
            SendDataToClient((byte)119, data, client);
        }
        private void MonitorProc(byte[] data, Socket client)
        {
            char request = (char)data[0];
            string _data = "";
            switch (request)
            {
                case 'a':
                    _data += "a" + "$sep$";
                    _data += clientSet.Count.ToString() + "$sep$";
                    _data += roomSet.Count.ToString() + "$sep$";
                    _data += errors;
                    errors = "";
                    break;
                case 'c':
                    _data += "c" + " ";
                    _data += clientSet.Count.ToString() + " ";
                    break;
                case 'r':
                    _data += "r" + " ";
                    _data += roomSet.Count.ToString() + " ";
                    break;
                case 'e':
                    _data += "e" + "$sep$";
                    _data += errors;
                    errors = "";
                    break;
            }
            SendDataToClient((byte)120, Encoding.Unicode.GetBytes(_data), client);
        }
        private void NoticeProc(byte[] data, Socket client)
        {
            foreach (Socket _client in clientSet.Keys)
            {
                SendDataToClient((byte)121, data, _client);
            }
        }
    }
}

/* 
인젝션 방어: https://msdn.microsoft.com/ko-kr/library/ms161953.aspx
*/