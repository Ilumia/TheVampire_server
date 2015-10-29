using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    class RoomInfo
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
            if (timer > 0.0f)
            {
                timer -= 0.1f;
            }
            else
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
                            Console.WriteLine("gameStart message!!");
                        }
                        isReadyToStart = false;
                    }
                }
                else if (roomState == 0)
                {
                    // Game is over
                }
                else
                {
                    // Now playing
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

        // PreProcessing
        public void CardSubmitted(string data, Socket client)
        {
            string[] tmp = data.Split(' ');
            Player player = users[client];
            Player target = null;
            int cardNo = Int32.Parse(tmp[0]);
            if(tmp.Length == 2) {
                target = FindPlayerFromID(tmp[1]);
            }
            switch (cardNo)
            {
                case 30:    // 조사
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, target));
                    break;
                case 31:    // 은폐
                    users[client].isHiding = true;
                    break;
                case 32:    // 통신

                    break;
                case 33:    // 위장
                    users[client].isCamouflaging = true;
                    break;
                case 34:    // 염탐
                    target.isObserved.Add(player);
                    break;

                /* 사용 안함
                case 35:    // 도청
                    break;
                */
                case 36:    // 파악
                    users[client].isRealizing = true;
                    break;
                case 37:    // 입막음
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, target));
                    break;
                /* 클라이언트에서만 사용
                case 38:    // 행동 재개
                    break;
                */
                case 60:    // 저격
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, target));
                    break;
                case 61:    // 강타
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, target));
                    break;
                case 62:    // 엄폐
                    users[client].isConcealing = true;
                    break;
                case 63:    // 방어
                    users[client].isDefencing = true;
                    break;
                case 64:    // 응급치료
                    
                    break;
                case 65:    // 교란
                    users[client].isConfusing = true;
                    break;
                case 66:    // 반격
                    users[client].isCounting = true;
                    break;
                case 67:    // 함정
                    users[client].isTrapping = true;
                    break;
                case 68:    // 빠른 몸놀림
                    users[client].isReattacking = true;
                    break;
                case 69:    // 금제
                    target.isDisabled = true;
                    break;
                /* 클라이언트에서만 사용
                case 70:    // 행동 재개
                    break;
                */
            }
        }
        public void PostProcessing()
        {
            while(processQueue.Count > 0) {
                ProcessQueue process = processQueue.Dequeue();
                int randomNumber = Server.GetRandom();
                switch (process.cardNo)
                {
                    case 30:    // 조사
                        if (process.target.isHiding)
                        {
                            // 은폐상태
                        }
                        else if (process.target.isCamouflaging)
                        {
                            // 위장상태
                        }
                        else if (process.user.isObserved.Count > 0)
                        {
                            // 염탐상태
                            process.user.isObserved.Clear();
                        }
                        else
                        {

                        }
                        break;
                    case 37:    // 입막음
                        break;
                    case 60:    // 저격
                        break;
                    case 61:    // 강타
                        break;
                }
            }
            // 일괄수행 (상태변화)
        }


    }
    class ProcessQueue {
        public int cardNo;
        public Player user;
        public Player target;
        public ProcessQueue(int _cardNo, Player _user, Player _target) {
            cardNo = _cardNo;
            user = _user;
            target = _target;
        }
        public ProcessQueue(int _cardNo, Player _user) {
            cardNo = _cardNo;
            user = _user;
        }
    }
}
