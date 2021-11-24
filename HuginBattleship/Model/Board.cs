using System.Collections.Generic;
using System.Drawing;

namespace HuginBattleship
{
    public class Board
    {
        public List<Ship> ships { get; }
        public Tile[,] tiles;

        public Board() 
        {
            ships = new List<Ship>();
            // Creates game board
            tiles = new Tile[Settings.boardWidth, Settings.boardWidth];
            for (int i = 0; i < Settings.boardWidth; i++)
            {
                for (int j = 0; j < Settings.boardWidth; j++)
                {
                    tiles[i, j] = new Tile();
                }
            }
        }
        public bool PlaceShip(Ship ship)
        {
            if (CheckIfError(ship))
            {
                return false;
            }
            else
            {
                ships.Add(ship);

                foreach (Point coord in ship.shipCoords)
                {
                    tiles[coord.X, coord.Y].SetShip(ship);
                }
                return true;
            }
        }
        private bool CheckIfError(Ship ship)
        {
            // Returns true if a ship is being placed out of bounds or is being placed on an existing ship
            return (ShipOutOfBounds(ship) || ShipsOverlap(ship));
        }
        private bool ShipOutOfBounds(Ship ship)
        {
            bool outofbounds;
            // Returns true if a ship is out of bounds
            foreach (Point coord in ship.shipCoords)
            {
                outofbounds = ((coord.X >= Settings.boardWidth) || (coord.Y >= Settings.boardWidth));
                if (outofbounds)
                {
                    return true;
                }
            }
            return false;
        }
        private bool ShipsOverlap(Ship ship)
        {
            bool overlap;
            // Returns true if a ship already exists where the new ship is being placed
            foreach (Point coord in ship.shipCoords)
            {
                overlap = tiles[coord.X, coord.Y].CheckShip();
                if (overlap)
                {
                    return true;
                }
            }
            return false;
        }
        public bool ShootAt(Point point)
        {
            Tile shootingTile = tiles[point.X, point.Y];
            bool wasHit;
            // Shoots at x, y on the board if not already shot
            if (shootingTile.state == (int)Tile.TileStates.unknown)
            {
                wasHit = shootingTile.SetTileState();
                if (wasHit)
                {
                    foreach (Ship ship in ships)
                    {
                        foreach (Point coord in ship.shipCoords)
                        {
                            if (coord == point)
                            {
                                ship.IncreaseHits();
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

    }
}
