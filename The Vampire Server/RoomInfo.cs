using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    partial class RoomInfo
    {
        public int roomNumber;
        private static int nextRoomNumber = 0;
        public int totalNumber;
        public int maximumNumber;
        public bool isPublic;
        public Dictionary<Socket, Player> users;
        public Socket owner;
        public int roomState;   // -1: 시작 전, 0: 게임종료, 1이상의 양수: 진행회차
        public float timer;
        public float nextNoticeTimer;
        public bool isReadyToStart;
        public Queue<ProcessQueue> processQueue;

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
            timer = 0.0f;
            nextNoticeTimer = 0.0f;
            isReadyToStart = false;
            processQueue = new Queue<ProcessQueue>();
        }
        public bool JoinRoom(Socket client, string clientid)
        {
            Console.WriteLine(totalNumber);
            if (totalNumber < maximumNumber)
            {
                users.Add(client, new Player(clientid));
                this.totalNumber++;

                // Countdown of Game start
                if (this.totalNumber == 2)
                {
                    timer = 10.0f;
                    nextNoticeTimer = timer - 1.0f; ;
                    isReadyToStart = true;
                    Console.WriteLine("isReadyToStart: " + isReadyToStart);
                }
            }
            RoomInOutNotice(this, clientid, true);
            return true;
        }
        public bool ExitRoom(Socket client, string clientid)
        {
            bool tt = users.Remove(client);
            totalNumber--;
            timer = 0.0f;
            nextNoticeTimer = 0.0f;
            isReadyToStart = false;
            RoomInOutNotice(this, clientid, false);
            return true;
        }
        private void RoomInOutNotice(RoomInfo roomInfo, string userID, bool isEnter)
        {
            string tmp = "";
            if (isEnter)
            {
                tmp = "i " + userID;
            }
            else
            {
                tmp = "o " + userID;
            }
            foreach (Socket client in roomInfo.users.Keys)
            {
                Server.GetInstance().SendDataToClient((byte)110, tmp, client);
            }
        }
        public void TimerUpdate()
        {
            bool isReady = false;
            if (timer > 0.0f)
            {
                timer -= 0.1f;
                if (timer > 0.0f)
                {
                    if (nextNoticeTimer >= timer)
                    {
                        nextNoticeTimer = timer - 1.0f;
                        double _tmpTimer = Math.Round(nextNoticeTimer);
                        foreach(Socket _client in users.Keys)
                        {
                            Server.GetInstance().SendDataToClient((byte)111, _tmpTimer.ToString(), _client);
                        }
                    }
                } else
                {
                    isReady = true;
                }
            }
            if (isReady)
            {
                if (roomState == -1)
                {
                    if (isReadyToStart)
                    {
                        Server.ClassSetting(users);
                        // Send gameStart message
                        foreach (KeyValuePair<Socket, Player> pair in users)
                        {
                            String _t = "";
                            if (pair.Value.job == PlayerJob.HUNTER)
                            {
                                _t = "h";
                            }
                            else
                            {
                                _t = "v";
                            }
                            Server.GetInstance().SendDataToClient((byte)106, _t, pair.Key);
                        }
                        isReadyToStart = false;
                        roomState = 1;
                        timer = 15.0f;
                        nextNoticeTimer = timer - 1.0f;
                    }
                }
                else if (roomState == 0)
                {
                    // Game is over
                }
                else
                {
                    PostProcessing();
                    roomState++;
                    timer = 15.0f;
                    nextNoticeTimer = timer - 1.0f;
                    string tmpMessage = roomState.ToString();
                    foreach (Player _player in users.Values)
                    {
                        tmpMessage += " " + _player.id;
                        tmpMessage += " " + _player.hp;
                    }
                    foreach (Socket _client in users.Keys)
                    {
                        Server.GetInstance().SendDataToClient((109), tmpMessage, _client);
                    }
                }
            }
        }
        // No use
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

        private Player FindPlayerFromID(string id)
        {
            foreach (Player player in users.Values)
            {
                if (player.id.Equals(id))
                {
                    return player;
                }
            }
            return null;
        }
        private Socket FindSocketFromID(string id)
        {
            return users.Where(x => x.Value.id.Equals(id)).Select(x => x.Key).First();
        }
        private string TranslateJob(PlayerJob job)
        {
            if (job == PlayerJob.HUNTER)
            {
                return "헌터";
            }else
            {
                return "뱀파이어";
            }
        }
    }
}
