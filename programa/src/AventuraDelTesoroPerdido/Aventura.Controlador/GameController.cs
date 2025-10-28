using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aventura.Model;
namespace Aventura.Controller

{
    public class GameController
    {
        private GameState state;

        public GameController()
        {
            state = new GameState();
        }

        public string GetPlayerInfo()
        {
            return $"{state.PlayerName} - Puntos: {state.Score}";
        }

        public void AddPoints(int points)
        {
            state.Score += points;
        }
    }
}
