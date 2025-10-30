using System;
using System.Collections.Generic;

namespace Aventura.Model
{
    public class GameState
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public string CurrentLocation { get; set; }
        public List<string> Inventory { get; set; }
        public List<string> VisitedLocations { get; set; }

        public GameState()
        {
            PlayerName = "Explorador";
            Score = 0;
            CurrentLocation = "bosque"; // Estado inicial según Prolog
            Inventory = new List<string>();
            VisitedLocations = new List<string> { CurrentLocation };
        }

        public void AddToInventory(string item)
        {
            if (!Inventory.Contains(item))
            {
                Inventory.Add(item);
                Score += 10; // ejemplo: cada objeto suma puntos
            }
        }

        public void MoveTo(string location)
        {
            CurrentLocation = location;
            if (!VisitedLocations.Contains(location))
                VisitedLocations.Add(location);
        }

        public string InventoryAsString()
        {
            return Inventory.Count == 0 ? "No tienes ningún objeto." : "Tienes: " + string.Join(", ", Inventory);
        }

        public string VisitedAsString()
        {
            return VisitedLocations.Count == 0 ? "No has visitado ningún lugar." : string.Join(" -> ", VisitedLocations);
        }
    }
}
