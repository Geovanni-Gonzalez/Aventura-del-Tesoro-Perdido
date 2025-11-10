using System.Collections.Generic;

namespace Aventura.Model
{
    public class GameState
    {
        public string CurrentPlace { get; set; } = "";
        public List<string> Inventory { get; set; } = new List<string>();
        public List<string> AvailablePlaces { get; set; } = new List<string>();
        public string Description { get; set; } = "";
    }
}


