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

        // ==============================
        // 🔄 Actualizar estado desde Prolog
        // ==============================
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
                Console.WriteLine($"⚠️ Error al actualizar estado: {ex.Message}");
            }
        }

        // ==============================
        // 🧭 Acciones de juego
        // ==============================

        public async Task<string> MoverAAsync(string destino)
        {
            var body = new { destino };
            return await PostJsonAsync($"{_urlBase}/mover", body);
        }

        public async Task<string> UsarAsync(string objeto)
        {
            var body = new { objeto };
            return await PostJsonAsync($"{_urlBase}/usar", body);
        }

        // (Si luego agregas "tomar/1" en el servidor, puedes usar el mismo patrón)
        public async Task<string> TomarAsync(string objeto)
        {
            return "⚠️ Acción 'tomar' no implementada en el servidor todavía.";
        }

        public async Task<string> ReiniciarJuegoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/reiniciar");
                var json = JsonDocument.Parse(response);
                return json.RootElement.GetProperty("mensaje").GetString();
            }
            catch (Exception ex)
            {
                return $"❌ Error al reiniciar: {ex.Message}";
            }
        }

        // ==============================
        // ⚙️ Utilidades HTTP
        // ==============================
        private async Task<string> PostJsonAsync(string url, object data)
        {
            try
            {
                var contenido = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, contenido);
                var jsonString = await response.Content.ReadAsStringAsync();

                using (var json = JsonDocument.Parse(jsonString))
                {
                    if (json.RootElement.TryGetProperty("mensaje", out var mensaje))
                        return mensaje.GetString() ?? "Sin mensaje.";
                    if (json.RootElement.TryGetProperty("resultado", out var resultado))
                        return resultado.GetString() ?? "Sin resultado.";

                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error al comunicarse con Prolog: {ex.Message}";
            }
        }

        // ==============================
        // 🔍 Utilidades
        // ==============================
        public async Task<List<string>> ObtenerLugaresVisitadosAsync()
        {
            await ActualizarEstadoAsync();
            return Estado.visitados ?? new List<string>();
        }
    }
}
