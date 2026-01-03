using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aventura.Model
{
    public class GameStateDto
    {
        [JsonPropertyName("ubicacion")]
        public string Ubicacion { get; set; }

        [JsonPropertyName("inventario")]
        public List<string> Inventario { get; set; }

        [JsonPropertyName("visitados")]
        public List<string> Visitados { get; set; }

        [JsonPropertyName("objetos")]
        public List<string> Objetos { get; set; } // Para respuesta de objetos_lugar

        [JsonPropertyName("caminos")]
        public List<string> Caminos { get; set; } // Para respuesta de caminos
    }

    public class MessageDto
    {
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("resultado")]
        public string Resultado { get; set; }
        
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
    
    // Para endpoints que devuelven listas de mensajes (como como_gano)
    public class MessageListDto
    {
         [JsonPropertyName("mensajes")]
         public List<string> Mensajes { get; set; }
    }
}
