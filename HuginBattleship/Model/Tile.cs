
namespace HuginBattleship
{
    public class Tile
    {
        public enum TileStates { unknown, missed, hit };
        // 0 not shot at, 1 missed, 2 hit
        public int state;
        public Ship ship;
        public Tile()
        {
            state = (int)TileStates.unknown;
        }
        public bool CheckShip()
        {
            return !(ship == null);
        }
        public void SetShip(Ship ship)
        {
            this.ship = ship;
        }
        public bool SetTileState()
        {
            // Runs if ship is hit, but not sunken
            if (ship != null)
            {
                state = (int)TileStates.hit;
                return true;
            }
            else
            {
                state = (int)TileStates.missed;
                return false;
            }
        }
        public string GetSunkenShip()
        {
            return ship.IsSunken() ? ship.name : ship.name + " is not sunken";
        }
    }
}
