using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    public class Message
    {
        private byte[] data;
        private int length;

        public byte Type { get { return data[0]; } }
        public byte[] Data
        {
            get
            {
                byte[] temp = new byte[0];
                temp = new byte[Length - 1];
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

    public class User
    {
        public string id;
        public ClientState state;
        public UserItem userItem;

        public User(string _id, ClientState _state)
        {
            id = _id;
            state = _state;
            userItem = new UserItem();
        }
        public void SetAllItems()
        {
            UserAbility tmpAb = new UserAbility();
            tmpAb.effectUpgrade = 0;
            UserInfoCard tmpIn = new UserInfoCard();
            tmpIn.pickRateUpgrade = 0;
            UserBattleCard tmpBa = new UserBattleCard();
            tmpBa.effectUpgrade = 0;
            tmpBa.pickRateUpgrade = 0;
            for (int i = 0; i < 7; i++)
            {
                userItem.abilities.Add(i, tmpAb);
            }
            for (int i = 30; i < 39; i++)
            {
                userItem.infoCards.Add(i, tmpIn);
            }
            for (int i = 60; i < 71; i++)
            {
                userItem.battleCards.Add(i, tmpBa);
            }
        }
    }

    public class UserItem
    {
        public Dictionary<int, UserAbility> abilities;
        public Dictionary<int, UserInfoCard> infoCards;
        public Dictionary<int, UserBattleCard> battleCards;
        public UserItem()
        {
            abilities = new Dictionary<int, UserAbility>();
            infoCards = new Dictionary<int, UserInfoCard>();
            battleCards = new Dictionary<int, UserBattleCard>();
        }
    }
    public class UserAbility
    {
        public int effectUpgrade;
    }
    public class UserInfoCard
    {
        public int pickRateUpgrade;
    }
    public class UserBattleCard
    {
        public int effectUpgrade;
        public int pickRateUpgrade;
    }

    public class RoomInfo
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
                    if(isReadyToStart)
                    {
                        Server.ClassSetting(users);
                        // Send gameStart message
                        foreach (KeyValuePair<Socket, Player> pair in users)
                        {
                            String _t = "";
                            if (pair.Value.job == PlayerJob.HUNTER)
                            {
                                _t = "h";
                            } else
                            {
                                _t = "v";
                            }
                            Server.GetInstance().SendDataToClient((byte)106, _t, pair.Key);
                            Console.WriteLine("gameStart message!!");
                        }
                        isReadyToStart = false;
                    }
                } else if(roomState == 0)
                {
                    // Game is over
                } else
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

        public bool JoinRoom(Socket client, string clientid)
        {
            Console.WriteLine(totalNumber);
            if (totalNumber < maximumNumber)
            {
                users.Add(client, new Player(clientid));
                this.totalNumber++;

                // Countdown of Game start
                if(this.totalNumber == 2)
                {
                    timer = 10.0f;
                    isReadyToStart = true;
                    Console.WriteLine("isReadyToStart: " + isReadyToStart);
                }
            }
            return true;
        }
        public bool ExitRoom(Socket client)
        {
            bool tt = users.Remove(client);
            totalNumber--;
            timer = 0.0f;
            isReadyToStart = false;
            return true;
        }
    }

    public enum ClientState { ONACCESS, ONLOGIN, ONLOBBY, ONROOM, ONGAME };
    public enum PlayerJob { VAMPIRE, HUNTER };
    public class Player
    {
        public string id;
        public ClientState state;
        public UserItem item;
        public bool isAI;
        public PlayerJob job;
        public int hp;
        public Player(string id)
        {
            this.id = id;
            this.state = ClientState.ONACCESS;
            item = new UserItem();
            SetAllItems();
            hp = 100;
        }
        public Player(string id, ClientState state)
        {
            this.id = id;
            this.state = state;
            item = new UserItem();
            SetAllItems();
            hp = 100;
        }
        public void SetState(ClientState state) { this.state = state; }
        public void SetJob(PlayerJob job) { this.job = job; }

        public void SetAllItems()
        {
            UserAbility tmpAb = new UserAbility();
            tmpAb.effectUpgrade = 0;
            UserInfoCard tmpIn = new UserInfoCard();
            tmpIn.pickRateUpgrade = 0;
            UserBattleCard tmpBa = new UserBattleCard();
            tmpBa.effectUpgrade = 0;
            tmpBa.pickRateUpgrade = 0;
            for (int i = 0; i < 7; i++)
            {
                item.abilities.Add(i, tmpAb);
            }
            for (int i = 30; i < 39; i++)
            {
                item.infoCards.Add(i, tmpIn);
            }
            for (int i = 60; i < 71; i++)
            {
                item.battleCards.Add(i, tmpBa);
            }
        }
    }

    public class Item
    {
        public Dictionary<int, Ability> abilitySet;
        public Dictionary<int, InfoCard> infoCardSet;
        public Dictionary<int, BattleCard> battleCardSet;
        public int itemVersion;
        public void SetItem(Dictionary<int, Ability> _abilitySet)
        {
            abilitySet = _abilitySet;
        }
        public void SetItem(Dictionary<int, InfoCard> _infoCardSet)
        {
            infoCardSet = _infoCardSet;
        }
        public void SetItem(Dictionary<int, BattleCard> _battleCardSet)
        {
            battleCardSet = _battleCardSet;
        }
        public void SetItem(Dictionary<int, Ability> _abilitySet, Dictionary<int, InfoCard> _infoCardSet, Dictionary<int, BattleCard> _battleCardSet)
        {
            this.abilitySet = _abilitySet;
            this.infoCardSet = _infoCardSet;
            this.battleCardSet = _battleCardSet;
        }
    }
    public class Ability
    {
        public float effect;
        public float effectFactor;
    }
    public class InfoCard
    {
        public float pickRate;
        public float cuccessRate;
    }
    public class BattleCard
    {
        public float effect;
        public float effectFactor;
        public float pickRate;
        public float cuccessRate;
    }
}
