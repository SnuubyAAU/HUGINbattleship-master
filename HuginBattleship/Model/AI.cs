using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using HAPI;
using System.Diagnostics;

namespace HuginBattleship
{
    public class AI : Player
    {
        private Domain battleship;
        private List<LabelledDCNode> shipList = new List<LabelledDCNode>();
        private List<List<BooleanDCNode>> tilesList = new List<List<BooleanDCNode>>();
        private List<Point> previousHits = new List<Point>();
        private SortedDictionary<int, int> indexes = new SortedDictionary<int, int>();
        private static Random random = new Random();
        public Dictionary<Point, double> probabilities { get; private set; }
        public bool probabilitiesReady;

        public AI(string name) : base(name)
        {
            probabilitiesReady = false;
            battleship = new Domain();
            probabilities = new Dictionary<Point, double>();
            InitBayesianNetwork();
            CalculateProbabilities(probabilities);
            probabilitiesReady = true;
        }
        private void InitBayesianNetwork()
        {
            InitShips();
            // Initializes overlap constraints
            MakeStatesForOverlap(shipList);
            InitTiles();
            battleship.Compile();
            
        }
        // Initializes ships
        public void InitShips()
        {
            int i = 0;
            foreach (KeyValuePair<string, int> ship in Settings.ships)
            {
                shipList.Add(new LabelledDCNode(battleship));
                shipList[i].SetName(ship.Key);
                // Set states and tables for all ships
                SetAllStatesForShips(shipList[i++], ship.Value);
            }
        }
        private void SetAllStatesForShips(LabelledDCNode ship, int length)
        {
            double possiblePosForRow = Settings.boardWidth - length + 1;
            // Finds all possible positions on the board for the ship
            double numberOfStates = Settings.boardWidth * possiblePosForRow * Settings.dimension;
            ulong count = 0;

            ship.SetNumberOfStates((ulong)numberOfStates);
            // Sets state labels and creates table for all states of the ship
            // Runs through the two orientations; horizontal and vertical
            for (int orientation = 0; orientation < Settings.dimension; orientation++)
            {
                for (int i = 0; i < (orientation == 0 ? Settings.boardWidth : possiblePosForRow); i++)
                {
                    for (int j = 0; j < (orientation == 1 ? Settings.boardWidth : possiblePosForRow); j++)
                    {
                        ship.SetStateLabel(count, (orientation == 1 ? "H" : "V") + $"_{i}{j}");
                        ship.GetTable().SetDataItem(count++, 1 / numberOfStates);
                    }
                }
            }
        }
        private void MakeStatesForOverlap(List<LabelledDCNode> shipList)
        {
            BooleanDCNode overlap;
            for (int i = 0; i < shipList.Count; i++)
            {
                for (int j = i + 1; j < shipList.Count; j++)
                {
                    overlap = new BooleanDCNode(battleship);
                    MakeStatesHelper(overlap, i, $"Overlap{i}_{j}");
                    overlap.AddParent(shipList[j]);
                    SetStatesForOverlaps(overlap, shipList[i], shipList[j]);
                    overlap.SelectState(0);
                }
            }
        }
        private void MakeStatesHelper(BooleanDCNode node, int i, string name)
        {
            node.SetNumberOfStates(2);
            node.SetStateLabel(0, "False");
            node.SetStateLabel(1, "True");
            node.AddParent(shipList[i]);
            node.SetName(name);
        }
        private void SetStatesForOverlaps(BooleanDCNode overlap, LabelledDCNode secondShip, LabelledDCNode firstShip)
        {
            int firstShipLength = Settings.boardWidth + 1 - firstShip.GetTable().GetData().Length /
                             (Settings.dimension * Settings.boardWidth);
            int secondShipLength = Settings.boardWidth + 1 - secondShip.GetTable().GetData().Length /
                              (Settings.dimension * Settings.boardWidth);
            List<Point> firstPoints;
            List<Point> secondPoints;
            ulong count = 0;
            string firstName, secondName;

            // Iterates through entire tables on constraint's parents
            for (int i = 0; i < (int)firstShip.GetNumberOfStates(); i++)
            {
                firstName = firstShip.GetStateLabel((ulong)i);
                // Gets coordinates for first ship
                firstPoints = ReturnCoordinates(firstShipLength, firstName);
                for (int j = 0; j < (int)secondShip.GetNumberOfStates(); j++)
                {
                    secondName = secondShip.GetStateLabel((ulong)j);
                    // Gets coordinates for second ship
                    secondPoints = ReturnCoordinates(secondShipLength, secondName);
                    // Checks if any ship coordinates overlap
                    if (CheckForOverlap(firstPoints, secondPoints))
                    {
                        overlap.GetTable().SetDataItem(count++, 0);
                        overlap.GetTable().SetDataItem(count++, 1);
                    }
                    else
                    {
                        overlap.GetTable().SetDataItem(count++, 1);
                        overlap.GetTable().SetDataItem(count++, 0);
                    }
                }
            }
        }
        // Converts start-coordinate to a list of the ships coordinates
        private List<Point> ReturnCoordinates(int length, string name)
        {
            List<Point> pointList = new List<Point>();
            char orientation = name[0];
            int xCoord = name[2] - '0';
            int yCoord = name[3] - '0';

            for (int i = 0; i < length; i++)
            {
                if (orientation == 'H')
                {
                    pointList.Add(new Point(xCoord + i, yCoord));
                }
                else if (orientation == 'V')
                {
                    pointList.Add(new Point(xCoord, yCoord + i));
                }
                else
                {
                    Debug.WriteLine($"Something seriously wrong with the orientation - said: {orientation}");
                }
            }
            return pointList;
        }
        private bool CheckForOverlap(List<Point> firstPoints, List<Point> secondPoints)
        {
            foreach (Point point in firstPoints)
            {
                if (secondPoints.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }
        // Initializes tiles
        public void InitTiles()
        {
            for (int i = 0; i < Settings.boardWidth; i++)
            {
                char letter = (char)(i + 'A');
                for (int j = 0; j < Settings.boardWidth; j++)
                {
                    MakeTileStates($"{letter}{j}S");
                }
            }
        }
        private void MakeTileStates(string name)
        {
            List<BooleanDCNode> tileList = new List<BooleanDCNode>();
            BooleanDCNode tile = new BooleanDCNode(battleship);
            int templistIndex = 0;
            int constraintNumber = 1;
            // Sets states for the two first tiles
            MakeStatesHelper(tile, 0, $"{name}{constraintNumber++}");
            tile.AddParent(shipList[1]);
            SetTileStateWithTwoShips(tile, shipList[0], shipList[1], name);
            tileList.Add(tile);
            // Sets states for the rest of the tiles
            for (int i = 2; i < shipList.Count; i++)
            {
                tile = new BooleanDCNode(battleship);
                MakeStatesHelper(tile, i, $"{name}{constraintNumber++}");
                tile.AddParent(tileList[templistIndex]);
                SetTileStateWithOneShip(tile, shipList[i], tileList[templistIndex++], name);
                tileList.Add(tile);
            }
            tilesList.Add(tileList);
        }
        // Compares to two ships
        private void SetTileStateWithTwoShips(BooleanDCNode tile, LabelledDCNode secondShip, LabelledDCNode firstShip, string name)
        {
            int firstShipLength = Settings.boardWidth + 1 - firstShip.GetTable().GetData().Length /
                                 (Settings.dimension * Settings.boardWidth);
            int secondShipLength = Settings.boardWidth + 1 - secondShip.GetTable().GetData().Length /
                                  (Settings.dimension * Settings.boardWidth);
            int xCoord = name[0] - 'A';
            int yCoord = name[1] - '0';
            List<Point> firstPoints;
            List<Point> secondPoints;
            Point tilePlace = new Point(xCoord, yCoord);
            ulong count = 0;
            string firstName, secondName;

            for (int i = 0; i < (int)firstShip.GetNumberOfStates(); i++)
            {
                firstName = firstShip.GetStateLabel((ulong)i);
                // Gets coordinates for first ship
                firstPoints = ReturnCoordinates(firstShipLength, firstName);
                for (int j = 0; j < (int)secondShip.GetNumberOfStates(); j++)
                {
                    secondName = secondShip.GetStateLabel((ulong)j);
                    // Gets coordinates for second ship
                    secondPoints = ReturnCoordinates(secondShipLength, secondName);
                    if (secondPoints.Contains(tilePlace) || firstPoints.Contains(tilePlace))
                    {
                        tile.GetTable().SetDataItem(count++, 0);
                        tile.GetTable().SetDataItem(count++, 1);
                    }
                    else
                    {
                        tile.GetTable().SetDataItem(count++, 1);
                        tile.GetTable().SetDataItem(count++, 0);
                    }
                }
            }
        }
        // Compares to previous tile instead of a second ship
        private void SetTileStateWithOneShip(BooleanDCNode tile, LabelledDCNode ship, BooleanDCNode node, string name)
        {
            int shipLength = Settings.boardWidth + 1 - ship.GetTable().GetData().Length /
                            (Settings.dimension * Settings.boardWidth);
            int xCoord = name[0] - 'A';
            int yCoord = name[1] - '0';
            Point tilePlace = new Point(xCoord, yCoord);
            List<Point> shipPoints;
            bool labelIsTrue;
            string shipName;
            ulong count = 0;

            for (ulong i = 0; i < 2; i++)
            {
                labelIsTrue = node.GetStateLabel(i) == "True";
                for (ulong j = 0; j < ship.GetNumberOfStates(); j++)
                {
                    shipName = ship.GetStateLabel(j);
                    shipPoints = ReturnCoordinates(shipLength, shipName);
                    if (labelIsTrue || shipPoints.Contains(tilePlace))
                    {
                        tile.GetTable().SetDataItem(count++, 0);
                        tile.GetTable().SetDataItem(count++, 1);
                    }
                    else
                    {
                        tile.GetTable().SetDataItem(count++, 1);
                        tile.GetTable().SetDataItem(count++, 0);
                    }
                }
            }
        }
        // Executes all necessary methods on the player's turn
        public override bool SetShip(Ship temp)
        {
            int orientation;
            char orientationLetter;
            bool correctlyPlaced;
            foreach (KeyValuePair<string, int> ship in Settings.ships)
            {
                correctlyPlaced = false;
                while (!correctlyPlaced)
                {
                    Point point = new Point
                    {
                        X = new Random().Next(0, Settings.boardWidth),
                        Y = new Random().Next(0, Settings.boardWidth)
                    };
                    orientation = new Random().Next(0, 2);
                    orientationLetter = orientation == 0 ? 'H' : 'V';
                    List<Point> coords = new List<Point>();
                    for (int i = 0; i < ship.Value; i++)
                    {
                        if (orientationLetter == 'H')
                        {
                            coords.Add(new Point(point.X + i, point.Y));
                        }
                        else {
                            coords.Add(new Point(point.X, point.Y + i));

                        }
                    }

                    correctlyPlaced = board.PlaceShip(new Ship(ship.Key, ship.Value, coords, orientationLetter));
                }
            }
            return true;
        }
        public void AIshoot()
        {
            Point shootingPoint;
            probabilitiesReady = false;
            // Finds point with highest probability to contain a ship
            shootingPoint = FindShootingPoint(probabilities);
            shots.Add(shootingPoint);
            // Shoots at the board
            Shoot(shootingPoint);
            Tile shootingTile = board.tiles[shootingPoint.X, shootingPoint.Y];
            // Inserts evidence to the bayesian network
            SetEvidence(shootingTile, shootingPoint);
            battleship.Propagate(Domain.Equilibrium.H_EQUILIBRIUM_SUM,
                                 Domain.EvidenceMode.H_EVIDENCE_MODE_NORMAL);

            CalculateProbabilities(probabilities);
            probabilitiesReady = true;
        }
        // Calculates the probablility for each tile and adds the result to the dictionary
        private void CalculateProbabilities(Dictionary<Point, double> probabilities)
        {
            double probability;

            probabilities.Clear();
            for (int i = 0; i < Settings.boardSize; i++)
            {
                probability = tilesList[i][tilesList[i].Count - 1].GetBelief(1);
                Point tilePoint = new Point(i / Settings.boardWidth, i % Settings.boardWidth);
                probabilities.Add(tilePoint, probability);
            }
        }
        // Finds the points with the greatest probablities and returns one of these randomly
        public Point FindShootingPoint(Dictionary<Point, double> probabilities)
        {
            Dictionary<Point, double> temp = probabilities;
            double maxValue = 0;

            // Removes points already shot from the dictionary
            foreach (Point point in shots)
            {
                temp.Remove(point);
            }
            maxValue = temp.Values.Max();
            Point[] shootingPoints = temp.Where(p => Math.Abs(p.Value - maxValue) < 0.0000000001).Select(p => p.Key).ToArray();

            return shootingPoints[random.Next(0, shootingPoints.Length)];
        }
        private void SetEvidence(Tile shootingTile, Point shootingPoint)
        {
            long shipStateIndex;
            int index = shootingPoint.X * Settings.boardWidth + shootingPoint.Y;
            int divorcedTiles = tilesList[index].Count - 1;

            if (shootingTile.state == (int)Tile.TileStates.hit)
            {
                // Only sets evidence for the last tile node
                tilesList[index][divorcedTiles].SelectState(1);
                previousHits.Add(shootingPoint);
            }
            else if (shootingTile.state == (int)Tile.TileStates.missed)
            {
                tilesList[index][divorcedTiles].SelectState(0);
            }
            else
            {
                string sunkenShipName = shootingTile.GetSunkenShip();
                var sunkenShipLength = Settings.ships.Where(p => p.Key == sunkenShipName).Select(p => p.Value).ElementAt(0);
                int shipNumber = 0;
                while (shipList[shipNumber].GetName() != sunkenShipName) shipNumber++;
                indexes.Add(shipNumber, sunkenShipLength);

                previousHits.Add(shootingPoint);
                previousHits.OrderBy(p => p.X).ThenBy(p => p.Y).Reverse();
                tilesList[index][divorcedTiles].SelectState(1);

                // Inserts evidence only if there is one possible position
                if (sunkenShipLength == previousHits.Count)
                {
                    string shipPos;
                    foreach (KeyValuePair<int, int> shipIndex in indexes)
                    {
                        shipPos = FindShipPos(shipIndex.Value, shipList[shipIndex.Key]);
                        shipStateIndex = shipList[shipIndex.Key].GetStateIndex(shipPos);
                        // Inserts evidence
                        shipList[shipIndex.Key].SelectState((ulong)shipStateIndex);
                    }
                    indexes.Clear();
                    previousHits.Clear();
                }
            }
        }
        private string FindShipPos(int length, LabelledDCNode ship)
        {
            List<List<Point>> allPossiblePos = new List<List<Point>>();

            // Runs through a list of hit tiles, which aren't labeled sunken yet
            for (int i = 0; i < previousHits.Count; i++)
            {
                if (FindShipOrientation(length, 0, 1, previousHits[i]))
                {
                    allPossiblePos.Add(CreateShipPosList(length, 'V', previousHits[i]));
                }
                else if (FindShipOrientation(length, 1, 0, previousHits[i]))
                {
                    allPossiblePos.Add(CreateShipPosList(length, 'H', previousHits[i]));
                }
            }

            return SelectBestBelief(ship, allPossiblePos);
        }
        private bool FindShipOrientation(int length, int xDir, int yDir, Point coord)
        {
            int testLength = length - 1;
            Point newCoord = new Point(coord.X + xDir, coord.Y + yDir);
            // Enters if the ship is found
            if (testLength == 0)
            {
                return true;
            }
            // Enters if there is no ship on the coordinate
            if (!previousHits.Contains(newCoord))
            {
                return false;
            }
            // Calls the method recursively if all the coordinates are not found yet
            return FindShipOrientation(testLength, xDir, yDir, newCoord);
        }
        // Creates a list with all the coordinates for the ship's positions
        private List<Point> CreateShipPosList(int length, char direction, Point start)
        {
            List<Point> returnList = new List<Point>();

            if (direction == 'H')
            {
                for (int i = 0; i < length; i++)
                {
                    returnList.Add(new Point(start.X + i, start.Y));
                }
            }
            else if (direction == 'V')
            {
                for (int i = 0; i < length; i++)
                {
                    returnList.Add(new Point(start.X, start.Y + i));
                }
            }
            else
            {
                throw new ArgumentException("Invalid direction");
            }

            return returnList;
        }
        // Finds the ship position with the greatest probability
        private string SelectBestBelief(LabelledDCNode ship, List<List<Point>> allPossiblePos)
        {
            List<double> beliefs = new List<double>();
            List<string> startCoords = new List<string>();
            double bestBelief = 0;
            string startCoord = "";
            long beliefIndex;
            // Runs through all possible ship positions
            foreach (List<Point> list in allPossiblePos)
            {
                // Enters if the two first Y-coordinates have the same value
                if (list[0].Y - list[1].Y == 0)
                {
                    startCoord = $"H_{list[0].X}{list[0].Y}";
                }
                else
                {
                    startCoord = $"V_{list[0].X}{list[0].Y}";
                }
                beliefIndex = ship.GetStateIndex(startCoord);
                beliefs.Add(ship.GetBelief((ulong)beliefIndex));
                startCoords.Add(startCoord);
            }
            for (int i = 0; i < beliefs.Count; i++)
            {
                // Enters if the probability is greater than those already found
                if (beliefs[i] > bestBelief)
                {
                    bestBelief = beliefs[i];
                    startCoord = startCoords[i];
                }
            }

            if (startCoord != "")
            {
                return startCoord;
            }
            throw new ArgumentException("Something went wrong while trying to find sunken ship's start coordinate.");
        }
        public override void Shoot(Point point)
        {
            shots.Add(point);

        }
        public void DeleteDomain()
        {
            battleship.Delete();
        }
        public bool GetDomainStatus()
        {
            return battleship.IsAlive();
        }
    }
}
