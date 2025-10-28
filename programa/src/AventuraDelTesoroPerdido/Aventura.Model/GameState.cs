using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aventura.Model
{
    public class GameState
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }

        public GameState()
        {
            PlayerName = "Explorador";
            Score = 0;
        }
    }
}
