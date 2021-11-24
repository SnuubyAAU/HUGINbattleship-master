using System;
using System.Collections.Generic;

namespace HuginBattleship
{

    public class Game : IGame
    {
        public List<Player> players;
        public string username;
        public Game()
        {

        }
        public void SetUsername(string username)
        {
            this.username = username;
            players = InitializePlayers();
        }
        public List<Player> InitializePlayers()
        {
            List<Player> players = new List<Player>();
            players.Add(new Human(username));
            players.Add(new AI("HUGIN AI"));
            players[1].SetShip(new Ship());
            return players;
        }
        public AI GetAI()
        {
            return (AI)players[1];
        }
        public Human GetHuman()
        {
            return (Human)players[0];
        }
    }
}
