using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace The_Vampire_Server
{
    partial class Server
    {
        static Random rand = new Random();
        public static void PlayerSetting(Dictionary<Socket, Player> users)
        {
            //ClassSetting(users);
        }
        public static void ClassSetting(Dictionary<Socket, Player> users)
        {
            Socket[] players;
            players = users.Keys.ToArray();
            if (GetRandom() > 50)
            {
                users[players[0]].SetJob(PlayerJob.VAMPIRE);
                if (GetRandom() > 50)
                {
                    users[players[1]].SetJob(PlayerJob.VAMPIRE);
                    users[players[2]].SetJob(PlayerJob.HUNTER);
                    users[players[3]].SetJob(PlayerJob.HUNTER);
                }
                else
                {
                    users[players[1]].SetJob(PlayerJob.HUNTER);
                    if (GetRandom() > 50)
                    {
                        users[players[2]].SetJob(PlayerJob.VAMPIRE);
                        users[players[3]].SetJob(PlayerJob.HUNTER);
                    }
                    else
                    {
                        users[players[2]].SetJob(PlayerJob.HUNTER);
                        users[players[3]].SetJob(PlayerJob.VAMPIRE);
                    }
                }
            }
            else
            {
                users[players[0]].SetJob(PlayerJob.HUNTER);
                if (GetRandom() > 50)
                {
                    users[players[1]].SetJob(PlayerJob.VAMPIRE);
                    if (GetRandom() > 50)
                    {
                        users[players[2]].SetJob(PlayerJob.VAMPIRE);
                        users[players[3]].SetJob(PlayerJob.HUNTER);
                    }
                    else
                    {
                        users[players[2]].SetJob(PlayerJob.HUNTER);
                        users[players[3]].SetJob(PlayerJob.VAMPIRE);
                    }
                }
                else
                {
                    users[players[1]].SetJob(PlayerJob.HUNTER);
                    users[players[2]].SetJob(PlayerJob.VAMPIRE);
                    users[players[3]].SetJob(PlayerJob.VAMPIRE);
                }
            }
        }
        public static int GetRandom()
        {
            return rand.Next(0, 100);
        }
        private static double GetRandomDouble()
        {
            return (rand.NextDouble() * 999999999999999);
        }
    }
}
