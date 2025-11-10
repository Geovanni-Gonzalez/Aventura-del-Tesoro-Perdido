using Aventura.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aventura.Controller
{
    public class GameController
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _urlBase = "http://localhost:5000"; // Puerto del servidor Prolog HTTP

        public GameState Estado { get; private set; } = new GameState();

        public event Action<GameState> OnGameStateUpdated;

        // ------------------------------
        // 🔄 Actualizar estado desde Prolog
        // ------------------------------
        public async Task ActualizarEstadoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/estado");
                var nuevoEstado = JsonSerializer.Deserialize<GameState>(response);
                if (nuevoEstado != null)
                {
                    Estado = nuevoEstado;
                    OnGameStateUpdated?.Invoke(Estado);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar estado: {ex.Message}");
                throw;
            }
        }

        // ------------------------------
        // 🧭 Consultas de acción
        // ------------------------------
        public async Task<string> MoverAAsync(string destino)
        {
            return await EnviarConsultaAsync($"mover({destino}).");
        }

        public async Task<string> TomarAsync(string objeto)
        {
            return await EnviarConsultaAsync($"tomar({objeto}).");
        }

        public async Task<string> UsarAsync(string objeto)
        {
            return await EnviarConsultaAsync($"usar({objeto}).");
        }

        public async Task<List<string>> ObtenerLugaresPosiblesAsync()
        {
            string res = await EnviarConsultaAsync("lugares_posibles(L).");
            return ParsearLista(res);
        }

        // ------------------------------
        // 💾 Reinicio / Gane
        // ------------------------------
        public async Task<string> VerificarGaneAsync()
        {
            return await EnviarConsultaAsync("verificar_gane.");
        }

        public async Task<string> ReiniciarJuegoAsync()
        {
            return await EnviarConsultaAsync("reiniciar_juego.");
        }

        // ------------------------------
        // 🔗 Comunicación HTTP
        // ------------------------------
        private async Task<string> EnviarConsultaAsync(string consulta)
        {
            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(new { query = consulta }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_urlBase}/consultar", contenido);
                response.EnsureSuccessStatusCode();

                var respuesta = await response.Content.ReadAsStringAsync();
                // Reemplazo de "using var" por bloque using tradicional compatible con C# 7.3
                using (var jsonDoc = JsonDocument.Parse(respuesta))
                {
                    return jsonDoc.RootElement.GetProperty("respuesta").GetString();
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error al comunicarse con Prolog: {ex.Message}";
            }
        }

        // ------------------------------
        // 🔍 Utilidades
        // ------------------------------
        private List<string> ParsearLista(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<string>();

            texto = texto.Trim('[', ']', ' ');
            var partes = texto.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
            var lista = new List<string>();
            foreach (var p in partes)
                lista.Add(p.Trim());
            return lista;
        }
    }
}
