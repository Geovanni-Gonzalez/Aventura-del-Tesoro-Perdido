using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aventura.Model;

namespace Aventura.Controller
{
    public class GameController
    {
        private readonly HttpClient _httpClient;
        private readonly string _urlBase;

        public GameState Estado { get; private set; } = new GameState();
        public event Action<GameState> OnGameStateUpdated;

        public GameController()
        {
            _urlBase = "http://localhost:5000"; // Servidor Prolog HTTP
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_urlBase)
            };
        }

        // ==============================
        // Actualizar estado general
        // ==============================
        public async Task ActualizarEstadoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/estado");
                var json = JsonDocument.Parse(response).RootElement;

                Estado.ubicacion = json.GetProperty("ubicacion").GetString() ?? "";
                Estado.inventario = json.GetProperty("inventario")
                                        .EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .ToList();
                Estado.visitados = json.GetProperty("visitados")
                                        .EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .ToList();

                OnGameStateUpdated?.Invoke(Estado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar estado: {ex.Message}");
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
            if (string.IsNullOrWhiteSpace(objeto))
                return "Selecciona un objeto válido.";

            // Creamos el body de forma correcta
            var body = new { objeto = objeto };

            // Post JSON y obtenemos mensaje
            string resultado = await PostJsonAsync("/tomar", body);

            // Actualizamos estado del juego
            await ActualizarEstadoAsync();

            return resultado;
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

                // Lee la respuesta como string
                var jsonString = await response.Content.ReadAsStringAsync();

                // Verifica si la respuesta tiene JSON válido
                if (string.IsNullOrWhiteSpace(jsonString))
                    return "Sin respuesta del servidor.";

                try
                {
                    using (var json = JsonDocument.Parse(jsonString))
                    {
                        var root = json.RootElement;

                        // Intentamos leer "mensaje"
                        if (root.TryGetProperty("mensaje", out var mensajeProp))
                            return mensajeProp.GetString() ?? "Sin mensaje.";

                        // Intentamos leer "resultado"
                        if (root.TryGetProperty("resultado", out var resultadoProp))
                            return resultadoProp.GetString() ?? "Sin resultado.";

                        // Si solo hay "error"
                        if (root.TryGetProperty("error", out var errorProp))
                            return $"❌ Error del servidor: {errorProp.GetString()}";

                        // Si no hay nada reconocido, retorna JSON crudo
                        return jsonString;
                    }
                }
                catch (JsonException)
                {
                    // Si la respuesta no es JSON válido
                    return $"❌ Respuesta no JSON: {jsonString}";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error al comunicarse con Prolog: {ex.Message}";
            }
        }

    }
}
