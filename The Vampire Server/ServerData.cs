using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace The_Vampire_Server
{
    public class User
    {
        public string id;
        public ClientState state;
        public UserItem userItem;
        public Dictionary<int, socketMessage> bufferSerial;
        public int nextBufferSerial = 0;
        public int nextBufferDeleteSerial = 0;
        
        public class socketMessage
        {
            public byte type;
            public byte[] data;
            public socketMessage(byte _type, byte[] _data)
            {
                type = _type;
                data = _data;
            }
        }
        public User(string _id, ClientState _state)
        {
            id = _id;
            state = _state;
            userItem = new UserItem();
            bufferSerial = new Dictionary<int, socketMessage>();
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
        public bool isHiding;           // 은폐
        public bool isCamouflaging;     // 위장
        public List<Player> isObserved; // 염탐(피)
        public bool isRealizing;        // 파악
        public bool isMuted;            // 입막음
        public bool isConcealing;       // 엄폐
        public bool isDefencing;        // 방어
        public bool isConfusing;        // 교란
        public bool isCounting;         // 반격
        public bool isTrapping;         // 함정
        public int isTrapped;           // 함정(피)
        public bool isReattacking;      // 빠른 몸놀림
        public bool isDisabled;         // 금제(피)
        public Player(string id)
        {
            this.id = id;
            this.state = ClientState.ONACCESS;
            item = new UserItem();
            SetAllItems();
            hp = 10;
            isHiding = false;
            isCamouflaging = false;
            isObserved = new List<Player>();
            isRealizing = false;
            isMuted = false;
            isConcealing = false;
            isDefencing = false;
            isConfusing = false;
            isCounting = false;
            isTrapping = false;
            isTrapped = 0;
            isReattacking = false;
            isDisabled = false;
        }
        public Player(string id, ClientState state)
        {
            this.id = id;
            this.state = state;
            item = new UserItem();
            SetAllItems();
            hp = 10;
            isHiding = false;
            isCamouflaging = false;
            isObserved = new List<Player>();
            isRealizing = false;
            isMuted = false;
            isConcealing = false;
            isDefencing = false;
            isConfusing = false;
            isCounting = false;
            isTrapping = false;
            isTrapped = 0;
            isReattacking = false;
            isDisabled = false;
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
