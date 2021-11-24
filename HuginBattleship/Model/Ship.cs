using System.Collections.Generic;
using System.Drawing;

namespace HuginBattleship
{
    public class Ship
    {
        public int length { get; }
        public int hits;
        public string name { get; }
        public char orientation { get; }
        public List<Point> shipCoords { get; }

        public Ship(string name, int length, List<Point> shipCoord, char orientation)
        {
            this.length = length;
            this.name = name;
            this.orientation = orientation;
            this.shipCoords = shipCoord;
            hits = 0;
        }
        public Ship()
        {

        }
        public void IncreaseHits()
        {
            hits++;
        }
        public bool IsSunken()
        {
            return hits == length;
        }
    }
}
