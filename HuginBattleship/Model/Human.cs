using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace HuginBattleship
{
    public class Human : Player
    {
        public Human(string name) : base(name)
        {
        }
        public override void Shoot(Point point)
        {
            shots.Add(point);

        }
        public override bool SetShip(Ship ship)
        {
            bool correctlyPlaced;
            // Places all ships from settings
            
                // Gets a ship from player
                // Validates ship location and places if possible
            correctlyPlaced = board.PlaceShip(ship);
            if (correctlyPlaced)
              {
                return true;
              }
            return false;
            
        }
    }
}
