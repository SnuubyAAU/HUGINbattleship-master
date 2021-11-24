using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HuginBattleship.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        IGame game;
        
        public GameController(IGame game)
        {
            this.game = game;
        }
        [Route("start")]
        [HttpPost]
        public void StartGame([FromBody]dynamic json)
        {
            dynamic temp = JsonConvert.DeserializeObject(json.ToString());
            string username = temp.username;
            //Initializes boards and players aswell
            game.SetUsername(username);
        }
        [Route("end")]
        [HttpPost]
        public void EndGame()
        {
            game.GetAI().DeleteDomain();
        }
        [Route("setship")]
        [HttpPost]
        public string SetShip([FromBody]dynamic json)
        {
            dynamic temp = JsonConvert.DeserializeObject(json.ToString());
            char orientation = (char)temp.ship.orientation;
            int length = (int)temp.ship.length;
            string name = temp.ship.name;
            Point shipCoord = new Point(temp.ship.x, temp.ship.y);
            List<Point> coords = new List<Point>();
            if (orientation == 'H')
            {
                for (int i = 0; i < length; i++)
                {
                    coords.Add(new Point(temp.ship.x + i, temp.ship.y));
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    coords.Add(new Point(temp.ship.x, temp.ship.y + i));
                }
            }
            Ship ship = new Ship(name, length, coords, orientation);
            bool error = game.GetHuman().SetShip(ship);
            if (!error)
            {
                return JsonConvert.SerializeObject(true);
            }
            return JsonConvert.SerializeObject(false);
        }
        [Route("human/shoot")]
        [HttpPost]
        public string Shoot([FromBody]dynamic json)
        {
            dynamic temp = JsonConvert.DeserializeObject(json.ToString());
            Point coord = new Point(temp.coord.x, temp.coord.y);
            game.GetHuman().Shoot(coord);
            bool hit = game.GetAI().board.ShootAt(coord);
            bool gameOver = game.GetHuman().AllShipsSunken();
            ShotInfo info = new ShotInfo(hit, gameOver);
            return JsonConvert.SerializeObject(info);
        }
        [Route("ai/shoot")]
        [HttpPost]
        public string AIShoot()
        {
            AI ai = (AI)game.GetAI();
            Point aiCoord = ai.FindShootingPoint(ai.probabilities);
            ai.Shoot(aiCoord);
            bool hit = game.GetHuman().board.ShootAt(aiCoord);
            bool gameOver = ai.AllShipsSunken();
            ShotInfo info = new ShotInfo(hit, gameOver);
            info.x = aiCoord.X;
            info.y = aiCoord.Y;
            return JsonConvert.SerializeObject(info);
        }
        [Route("get/AIships")]
        [HttpGet]
        public string GetAIShips()
        {
            return JsonConvert.SerializeObject(game.GetHuman().board.ships);
        }
        [Route("get/probabilities")]
        [HttpGet]
        public string GetAIProbabilities()
        {
            return JsonConvert.SerializeObject(Enumerable.Range(0, Settings.boardWidth).Select(index1 => Enumerable.Range(0, Settings.boardWidth).Select(index2 =>
                new TileInfo
                {
                    isShot = ((AI)game.GetAI()).shots.Contains(new Point(index2, index1)),
                    probability = ((AI)game.GetAI()).probabilities.Where(p => p.Key == new Point(index2, index1))
                                                        .Select(p => p.Value).Max(),
                    x = index2,
                    y = index1
                }
            )).ToString());
        }
        public class TileInfo
        {
            public bool isShot;
            public double probability;
            public int x;
            public int y;
            public TileInfo()
            {

            }
        }
        private class ShotInfo
        {
            public bool hit;
            public bool gameOver;
            public int x;
            public int y;
            public ShotInfo(bool hit, bool gameOver)
            {
                this.hit = hit;
                this.gameOver = gameOver;
            }
        }

    }
}