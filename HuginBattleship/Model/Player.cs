using System.Collections.Generic;
using System.Drawing;

namespace HuginBattleship
{
    public abstract class Player
    {
        public Board board;
        public string name;
        public List<Point> shots = new List<Point>();
        public Player(string name)
        {
            board = new Board();
            this.name = name;
        }
        public abstract void Shoot(Point point);
        public abstract bool SetShip(Ship ship);
        public bool AllShipsSunken()
        {
            int sunkenShips = 0;
            foreach (Ship ship in board.ships)
            {
                if (ship.IsSunken())
                {
                    sunkenShips++;
                }
                if (sunkenShips == board.ships.Count)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
