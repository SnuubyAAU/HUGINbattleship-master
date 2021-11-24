using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuginBattleship
{
    public interface IGame
    {
        public void SetUsername(string username);
        public List<Player> InitializePlayers();
        public AI GetAI();
        public Human GetHuman();
    }
}
