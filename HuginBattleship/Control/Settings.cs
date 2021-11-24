using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuginBattleship
{
    public static class Settings
    {
        public const int boardWidth = 8;
        public const int dimension = 2;
        public const int boardSize = boardWidth * boardWidth;
        // Largest ships first for ai purposes
        public static readonly Dictionary<string, int> ships = new Dictionary<string, int> // {shipName, shipLength}
        {
            {"Battleship", 4},
            {"Cruiser", 3},
            {"Submarine", 3}
        };
        public const int shipCount = 3;
    }
}
