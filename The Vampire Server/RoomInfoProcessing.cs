using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    partial class RoomInfo
    {
        Dictionary<Socket, string> dataStack = new Dictionary<Socket, string>();
        public void StackUpData(Socket client, string data)
        {
            if(!dataStack.ContainsKey(client))
            {
                dataStack.Add(client, "");
            }
            dataStack[client] += data + "\n";
        }
        public void GenerateStackWithTurnInfo(string data)
        {
            Dictionary<Socket, string> tmpStack = new Dictionary<Socket, string>();
            string separator = "\r";
            string additionalData = separator + data;
            foreach (KeyValuePair<Socket, string> stack in dataStack)
            {
                string tmp = stack.Value + additionalData;
                tmpStack.Add(stack.Key, tmp);
            }
            foreach (KeyValuePair<Socket, string> stack in tmpStack)
            {
                Server.GetInstance().SendDataToClient((byte)117, stack.Value, stack.Key);
            }
            dataStack.Clear();
        }

        // PreProcessing
        public void CardSubmitted(string data, Socket client)
        {
            string[] tmp = data.Split(' ');
            Player player = users[client];
            Player target = null;
            string message;
            int cardNo = Int32.Parse(tmp[0]);
            if (tmp.Length == 2)
            {
                target = FindPlayerFromID(tmp[1]);
                Console.WriteLine("//////////////////////\ncardSubmit\nplayer: {0}, target: {1}, cardNo: {2}\n//////////////////",
                    player.id, target.id, cardNo);
            }
            else
            {
                Console.WriteLine("//////////////////////\ncardSubmit\nplayer: {0}, cardNo: {1}\n//////////////////", 
                    player.id, cardNo);
            }
            switch (cardNo)
            {
                case 30:    // 조사
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, client, target));
                    break;
                case 31:    // 은폐
                    users[client].isHiding = true;
                    break;
                case 32:    // 통신
                    Player tmpPlayer = users.Where(x => x.Value.job == player.job).Select(x => x.Value).First();
                    message = player.id + "가 " + TranslateJob(player.job) + "만의 통신채널로 자신을 알렸습니다.";
                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(tmpPlayer.id));
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
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, client, target));
                    break;
                /* 클라이언트에서만 사용
                case 38:    // 행동 재개
                    break;
                */
                case 60:    // 저격
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, client, target));
                    break;
                case 61:    // 강타
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, client, target));
                    break;
                case 62:    // 엄폐
                    users[client].isConcealing = true;
                    break;
                case 63:    // 방어
                    users[client].isDefencing = true;
                    break;
                case 64:    // 응급치료
                    users[client].hp += 10;
                    if (users[client].hp > 100)  //추후 생명력 변경에 따른 조정 필요
                    {
                        users[client].hp = 100;
                    }
                    users[client].isTrapped = 0;
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
                    processQueue.Enqueue(new ProcessQueue(cardNo, player, client, target));
                    target.isDisabled = true;
                    break;
                /* 클라이언트에서만 사용
                case 70:    // 행동 재개
                    break;
                */
            }
        }
        void PostProcessing()
        {
            PreStateClear();
            while (processQueue.Count > 0)
            {
                ProcessQueue p = processQueue.Dequeue();
                int randomNumber = Server.GetRandom();
                switch (p.cardNo)
                {
                    // 조사
                    case 30: 
                        {
                            bool isCanceled = false;
                            string message;
                            if (p.target.isHiding) // 은폐상태
                            {
                                message = p.target.id + "를 조사했으나 실패했습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                isCanceled = true;
                            }
                            if (p.target.isCamouflaging) // 위장상태
                            {
                                message = p.target.id + "를 조사한 결과 그는 " + TranslateJob(p.user.job) + "입니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                isCanceled = true;
                            }
                            if (!isCanceled)
                            {
                                if (Server.GetRandom() > 50)
                                {
                                    message = p.target.id + "를 조사한 결과 그는 " + TranslateJob(p.target.job) + "입니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    if (p.user.isObserved.Count > 0) // 염탐상태
                                    {
                                        foreach (Player observer in p.user.isObserved)
                                        {
                                            message = p.user.id + "를 염탐한 결과 그는 " + TranslateJob(p.user.job) +
                                                "이며 그가 조사에 성공한 " + p.target.id + "는 " + TranslateJob(p.target.job) + "입니다.";
                                            Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(observer.id));
                                        }
                                        p.user.isObserved.Clear();
                                        isCanceled = true;
                                    }
                                }
                                else
                                {
                                    message = p.target.id + "를 조사하는데에 실패했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                }
                            }
                        }
                        break;
                    // 입막음
                    case 37: 
                        {
                            p.target.isMuted = true;
                            string message = "당신은 입막음당했습니다.\n한 턴간 채팅할 수 없습니다.";
                            Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                        }
                        break;
                    // 저격
                    case 60: 
                        {
                            int damage = 6;
                            bool isCanceled = false;
                            string message;
                            if(p.user.isReattacking) // 빠른몸놀림상태(공격자)
                            {
                                damage *= 2;
                                message = "빠른 몸놀림의 효과로 당신의 공격은 두배의 피해를 입힙니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                            }
                            if (p.target.isConcealing) // 엄폐상태
                            {
                                if(Server.GetRandom() > 50)
                                {
                                    message = p.target.id + "는 엄폐해 당신의 저격을 피했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = "누군가가 저격을 시도했지만 엄폐해 무사히 피했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                    isCanceled = true;
                                }
                            }
                            if (p.target.isConfusing) // 교란상태
                            {
                                List<Player> tmpPlayerList = new List<Player>();
                                Player newTarget;
                                foreach (Player tmpPlayer in users.Values)
                                {
                                    if(!tmpPlayer.id.Equals(p.user.id) && !tmpPlayer.id.Equals(p.target.id))
                                    {
                                        tmpPlayerList.Add(tmpPlayer);
                                    }
                                }
                                if(Server.GetRandom() > 50)
                                {
                                    newTarget = tmpPlayerList[0];
                                } else
                                {
                                    newTarget = tmpPlayerList[1];
                                }
                                newTarget.hp -= damage;
                                message = p.target.id + "를 저격하고자 하였으나 교란당해 " + newTarget.id + "를 공격하였습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                message = "누군가로부터 저격당했습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(newTarget.id));
                                isCanceled = true;
                            }
                            if (p.target.isTrapping) // 함정상태
                            {
                                p.user.isTrapped = 3;
                                message = p.target.id + "를 저격하던 중 함정에 당했습니다. 3턴간 지속피해를 입습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                            }
                            if (!isCanceled)
                            {
                                if (p.target.isDefencing) // 방어상태
                                {
                                    damage /= 2;
                                    message = p.target.id + "는 당신의 저격을 방어하고 절반의 피해를 입었습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = "누군가로부터 저격당했지만 방어해 절반의 피해만을 입었습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                } else
                                {
                                    message = p.target.id + "를 성공적으로 저격했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = "누군가로부터 저격당했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                }
                                if (p.target.isRealizing) // 파악상태
                                {
                                    message = "파악 결과 " + p.user.id + "가 당신을 저격했음을 알아냈습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                }
                                p.target.hp -= damage;
                            }
                        }
                        break;
                    // 강타
                    case 61: 
                        {
                            int damage = 10;
                            bool isCanceled = false;
                            string message;
                            if (p.user.isReattacking) // 빠른몸놀림상태(공격자)
                            {
                                damage *= 2;
                                message = "빠른 몸놀림의 효과로 당신의 공격은 두배의 피해를 입힙니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                            }
                            if (p.target.isConcealing) // 엄폐상태
                            {
                                if (Server.GetRandom() > 50)
                                {
                                    message = p.target.id + "는 엄폐해 당신의 공격을 피했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = p.user.id + "가 공격해왔으나 엄폐해 무사히 피했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                    isCanceled = true;
                                }
                            }
                            if (p.target.isConfusing) // 교란상태
                            {
                                List<Player> tmpPlayerList = new List<Player>();
                                Player newTarget;
                                foreach (Player tmpPlayer in users.Values)
                                {
                                    if (!tmpPlayer.id.Equals(p.user.id) && !tmpPlayer.id.Equals(p.target.id))
                                    {
                                        tmpPlayerList.Add(tmpPlayer);
                                    }
                                }
                                if (Server.GetRandom() > 50)
                                {
                                    newTarget = tmpPlayerList[0];
                                }
                                else
                                {
                                    newTarget = tmpPlayerList[1];
                                }
                                newTarget.hp -= damage;
                                message = p.target.id + "를 공격하고자 하였으나 교란당해 " + newTarget.id + "를 공격하였습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                message = p.user.id + "로부터 공격당했습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(newTarget.id));
                                isCanceled = true;
                            }
                            if (p.target.isCounting) // 반격상태
                            {
                                p.user.hp -= 10;
                                message = p.target.id + "를 공격하였으나 반격당해 큰 피해를 입었습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                message = p.user.id + "가 공격해왔지만 반격했습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                isCanceled = true;
                            }
                            if (p.target.isTrapping) // 함정상태
                            {
                                p.user.isTrapped = 3;
                                message = p.target.id + "를 공격하던 중 함정에 당했습니다. 3턴간 지속피해를 입습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                            }
                            if (!isCanceled)
                            {
                                if (p.target.isDefencing) // 방어상태
                                {
                                    damage /= 2;
                                    message = p.target.id + "는 당신의 공격을 방어하고 절반의 피해를 입었습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = p.user.id + "로부터 공격당했지만 방어해 절반의 피해만을 입었습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                }
                                else
                                {
                                    message = p.target.id + "를 성공적으로 공격했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                                    message = p.user.id + "로부터 공격당했습니다.";
                                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(p.target.id));
                                }
                                p.target.hp -= damage;
                            }
                        }
                        break;
                    // 금제
                    case 69:
                        {
                            {
                                p.target.isMuted = true;
                                string message = "당신은 금제당했습니다.\n한 턴간 행동할 수 없습니다.";
                                Server.GetInstance().SendDataToClient((byte)108, message, p.userSocket);
                            }
                        }
                        break;
                }
            }
            PostStateClear();
            PostGameCheck();
        }
        void PreStateClear()
        {
            foreach (Player player in users.Values)
            {
                if (player.isMuted)
                {
                    string message = "상태이상 [입막음]이 해제되었습니다.";
                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(player.id));
                    player.isMuted = false;
                }
                if (player.isTrapped > 0)
                {
                    player.isTrapped--;
                    player.hp -= 3;
                }
                if (player.isDisabled)
                {
                    string message = "상태이상 [금제]가 해제되었습니다.";
                    Server.GetInstance().SendDataToClient((byte)108, message, FindSocketFromID(player.id));
                    player.isDisabled = false;
                }
            }
        }
        void PostStateClear()
        {
            foreach(Player player in users.Values)
            {
                player.isHiding = false;
                player.isCamouflaging = false;
                player.isConcealing = false;
                player.isDefencing = false;
                player.isConfusing = false;
                player.isCounting = false;
            }
        }
        void PostGameCheck()
        {
            Player[] players = users.Values.ToArray();
            int v = 2;
            int h = 2;
            bool isGameOver = false;
            for(int i=0; i<4; i++)
            {
                if (players[i].hp < 0)
                {
                    if(players[i].job == PlayerJob.VAMPIRE) { v--; }
                    else if (players[i].job == PlayerJob.HUNTER) { h--; }
                }
            }
            string msg = "";
            if (h == 0 && v == 0) {
                msg = "d";
                isGameOver = true;
            }
            else if (v == 0) {
                msg = "h";
                isGameOver = true;
            }
            else if (h == 0) {
                msg = "v";
                isGameOver = true;
            }
            if(isGameOver)
            {
                roomState = 0;
                foreach (Socket _client in users.Keys)
                {
                    Server.GetInstance().SendDataToClient((byte)112, msg, _client);
                }
            }
        }
    }

    class ProcessQueue
    {
        public int cardNo;
        public Player user;
        public Socket userSocket;
        public Player target;
        public ProcessQueue(int _cardNo, Player _user, Socket _userSocket, Player _target)
        {
            cardNo = _cardNo;
            user = _user;
            target = _target;
            userSocket = _userSocket;
        }
        public ProcessQueue(int _cardNo, Player _user, Socket _userSocket)
        {
            cardNo = _cardNo;
            user = _user;
            userSocket = _userSocket;
        }
    }
}
