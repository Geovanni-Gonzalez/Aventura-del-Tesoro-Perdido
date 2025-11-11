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
        private readonly string _urlBase = "http://localhost:5000"; // Servidor Prolog HTTP

        public GameState Estado { get; private set; } = new GameState();

        public event Action<GameState> OnGameStateUpdated;

        // ==============================
        // 🔄 Actualizar estado general
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
            var mensaje = await PostJsonAsync($"{_urlBase}/mover", body);
            await ActualizarEstadoAsync();
            return mensaje;
        }

        public async Task<string> UsarAsync(string objeto)
        {
            var body = new { objeto };
            var mensaje = await PostJsonAsync($"{_urlBase}/usar", body);
            await ActualizarEstadoAsync();
            return mensaje;
        }

        public async Task<string> TomarAsync(string objeto)
        {
            var body = new { objeto };
            var mensaje = await PostJsonAsync($"{_urlBase}/tomar", body);
            await ActualizarEstadoAsync();
            return mensaje;
        }

        public async Task<string> ReiniciarJuegoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/reiniciar");
                var json = JsonDocument.Parse(response);
                var msg = json.RootElement.GetProperty("mensaje").GetString();
                await ActualizarEstadoAsync();
                return msg ?? "Juego reiniciado.";
            }
            catch (Exception ex)
            {
                return $"❌ Error al reiniciar: {ex.Message}";
            }
        }

        // ==============================
        // 🔍 Consultas especiales
        // ==============================

        /// <summary>
        /// Devuelve la lista de lugares visitados por el jugador.
        /// </summary>
        public async Task<List<string>> ObtenerLugaresVisitadosAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/visitados");
                var json = JsonDocument.Parse(response);

                if (json.RootElement.TryGetProperty("visitados", out var arr))
                {
                    var lista = new List<string>();
                    foreach (var item in arr.EnumerateArray())
                        lista.Add(item.GetString());
                    Estado.visitados = lista;
                    return lista;
                }

                return Estado.visitados ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener lugares visitados: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Devuelve los objetos que están en el lugar actual del jugador.
        /// </summary>
        public async Task<List<string>> ObtenerObjetosEnLugarAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/objetos_lugar");
                var json = JsonDocument.Parse(response);

                if (json.RootElement.TryGetProperty("objetos", out var arr))
                {
                    var lista = new List<string>();
                    foreach (var item in arr.EnumerateArray())
                        lista.Add(item.GetString());
                    Estado.objetosEnLugar = lista;
                    return lista;
                }

                return Estado.objetosEnLugar ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener objetos del lugar: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<string>> ObtenerCaminosAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_urlBase}/caminos");
                var json = JsonDocument.Parse(response);

                if (json.RootElement.TryGetProperty("caminos", out var caminosJson))
                {
                    var lista = new List<string>();
                    foreach (var item in caminosJson.EnumerateArray())
                        lista.Add(item.GetString());

                    Estado.caminosPosibles = lista; // actualiza el estado local
                    return lista;
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener caminos: {ex.Message}");
                return new List<string>();
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
    }
}
