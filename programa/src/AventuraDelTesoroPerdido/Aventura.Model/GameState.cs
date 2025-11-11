using System.Collections.Generic;

namespace Aventura.Model
{
    public class GameState
    {
        public string ubicacion { get; set; }
        public List<string> inventario { get; set; }
        public List<string> visitados { get; set; }
        public List<string> objetosEnLugar { get; set; }

        public List<string> caminosPosibles { get; set; }

    }
}

