using System.Collections.Generic;

namespace Aventura.Model
{
    // Nombre: GameState
    // Entrada: N/A (se rellena desde las respuestas del servidor)
    // Salida: Contenedor con el estado actual del juego en el cliente
    // Descripcion: Modelo de datos que mantiene la ubicación, inventario, lugares visitados y demás
    //              información sincronizada con el servidor Prolog.
    public class GameState
    {
        // Nombre: ubicacion
        // Entrada: string con el nombre del lugar actual (p.ej. "bosque")
        // Salida: Valor leído por la UI para mostrar la ubicación
        // Descripcion: Ubicación actual del jugador. Puede ser null si aún no se sincroniza el estado.
        public string ubicacion { get; set; }

        // Nombre: inventario
        // Entrada: Lista de strings con nombres de objetos
        // Salida: Colección consultada por la UI y lógica para acciones (usar, etc.)
        // Descripcion: Objetos que el jugador posee actualmente. Puede ser null si no se ha cargado.
        public List<string> inventario { get; set; }

        // Nombre: visitados
        // Entrada: Lista de strings con nombres de lugares visitados
        // Salida: Colección para consultas y visualización
        // Descripcion: Historial de lugares que el jugador ha visitado. Puede ser null si no se ha cargado.
        public List<string> visitados { get; set; }

        // Nombre: objetosEnLugar
        // Entrada: Lista de strings con objetos visibles en la ubicación actual
        // Salida: Colección usada por la UI para el ComboBox de "Tomar"
        // Descripcion: Objetos disponibles en el lugar actual. Puede ser null si no se ha consultado.
        public List<string> objetosEnLugar { get; set; }

        // Nombre: caminosPosibles
        // Entrada: Lista de strings con destinos conectados a la ubicación actual
        // Salida: Colección usada por la UI para el ComboBox de "Mover"
        // Descripcion: Destinos válidos a los que el jugador puede intentar moverse.
        public List<string> caminosPosibles { get; set; }

    }
}

