using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;

namespace The_Vampire_Server
{
    partial class Server
    {
        string path = System.IO.Directory.GetCurrentDirectory() + "/itemset";
        void ReadItemSet(Item item)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("not exist!");
                return;
            }
            try
            {
                string _cardSet = File.ReadAllText(path);
                string[] cardSet = _cardSet.Split('\n');
                Dictionary<int, Ability> abilitySet = new Dictionary<int, Ability>();
                Dictionary<int, InfoCard> infoCardSet = new Dictionary<int, InfoCard>();
                Dictionary<int, BattleCard> battleCardSet = new Dictionary<int, BattleCard>();

                Int32.TryParse(cardSet[0], out item.itemVersion);
                int i = 2;
                for (; i < cardSet.Length; i++)
                {
                    string[] tmp = cardSet[i].Split('\t');
                    Ability _ability = new Ability();
                    int number;
                    bool check = Int32.TryParse(tmp[0], out number);
                    if(!check)
                    {
                        i++;
                        break;
                    }
                    float.TryParse(tmp[3], out _ability.effect);
                    float.TryParse(tmp[4], out _ability.effectFactor);
                    abilitySet.Add(number, _ability);
                }
                for (; i < cardSet.Length; i++)
                {
                    if (!cardSet[i].Equals("$battle"))
                    {
                        string[] tmp = cardSet[i].Split('\t');
                        InfoCard _info = new InfoCard();
                        int number;
                        bool check = Int32.TryParse(tmp[0], out number);
                        if (!check)
                        {
                            i++;
                            break;
                        }
                        float.TryParse(tmp[3], out _info.pickRate);
                        float.TryParse(tmp[4], out _info.cuccessRate);
                        infoCardSet.Add(number, _info);
                    }
                    else
                    {
                        i++;
                        break;
                    }
                }
                for (; i < cardSet.Length; i++)
                {
                    string[] tmp = cardSet[i].Split('\t');
                    BattleCard _battle = new BattleCard();
                    int number;
                    bool check = Int32.TryParse(tmp[0], out number);
                    if (!check)
                    {
                        i++;
                        break;
                    }
                    float.TryParse(tmp[3], out _battle.effect);
                    float.TryParse(tmp[4], out _battle.effectFactor);
                    float.TryParse(tmp[5], out _battle.pickRate);
                    float.TryParse(tmp[6], out _battle.cuccessRate);
                    battleCardSet.Add(number, _battle);
                }

                item.SetItem(abilitySet, infoCardSet, battleCardSet);

            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Read item list (Data Version: " + item.itemVersion + ")");
        }
    }
}
