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
    // Nombre: GameController
    // Entrada: (N/A) Instanciación opcional sin parámetros.
    // Salida: Objeto controlador del estado y acciones del juego.
    // Descripcion: Encapsula llamadas HTTP al servidor Prolog, mantiene un GameState local y expone métodos async para acciones y consultas.
    public class GameController
    {
        private readonly HttpClient _httpClient;
        private readonly string _urlBase;

        // Nombre: Estado
        // Entrada: (asignado internamente tras peticiones)
        // Salida: Instancia GameState con ubicación, inventario, visitados, objetos y caminos.
        // Descripcion: Representa el estado actual cacheado del juego en el cliente.
        public GameState Estado { get; private set; } = new GameState();

        // Nombre: OnGameStateUpdated
        // Entrada: Delegate Action<GameState> suscrito externamente.
        // Salida: Evento disparado tras actualizar el estado.
        // Descripcion: Notifica a la UI u otros componentes que el estado cambió.
        public event Action<GameState> OnGameStateUpdated;

        // Nombre: GameController (constructor)
        // Entrada: urlBase (string)
        // Salida: Instancia inicializada con HttpClient apuntando al servidor.
        // Descripcion: Configura la URL base y el cliente HTTP reutilizable.
        public GameController(string urlBase)
        {
            _urlBase = urlBase;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_urlBase)
            };
        }

        // Nombre: PingAsync
        // Entrada: (ninguna)
        // Salida: bool (true si hay conexión, false si falla)
        // Descripcion: Realiza una petición ligera HEAD para verificar si el servidor responde.
        public async Task<bool> PingAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/estado");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ==============================
        // Actualizar estado general
        // ==============================

        // Nombre: ActualizarEstadoAsync
        // Entrada: (ninguna)
        // Salida: Task (actualiza Estado y dispara evento)
        // Descripcion: Obtiene /estado del servidor, parsea JSON y sincroniza el estado local.
        public async Task ActualizarEstadoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/estado");
                var dto = JsonSerializer.Deserialize<GameStateDto>(response);

                Estado.ubicacion = dto.Ubicacion ?? "";
                Estado.inventario = dto.Inventario ?? new List<string>();
                Estado.visitados = dto.Visitados ?? new List<string>();

                // [Fix] Fetch additional context for the "Intelligent UI"
                // We await these so the State is fully populated before the UI renders.
                // We ignore the return values because these methods update 'Estado' internally.
                await ObtenerObjetosEnLugarAsync();
                await ObtenerCaminosAsync();

                OnGameStateUpdated?.Invoke(Estado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar estado: {ex.Message}");
            }
        }

        // ==============================
        // Acciones de juego
        // ==============================

        // Nombre: MoverAsync
        // Entrada: destino (string)
        // Salida: Task<string> mensaje retornado por el servidor
        // Descripcion: Solicita mover al jugador al destino y refresca el estado.
        public async Task<string> MoverAsync(string destino)
        {
            var body = new { destino=destino };
            var mensaje = await PostJsonAsync($"{_urlBase}/mover", body);
            await ActualizarEstadoAsync();
            return mensaje;
        }

        // Nombre: UsarAsync
        // Entrada: objeto (string)
        // Salida: Task<string> mensaje de resultado
        // Descripcion: Intenta usar un objeto del inventario y refresca estado.
        public async Task<string> UsarAsync(string objeto)
        {
            var body = new { objeto };
            var mensaje = await PostJsonAsync($"{_urlBase}/usar", body);
            await ActualizarEstadoAsync();
            return mensaje;
        }

        // Nombre: TomarAsync
        // Entrada: objeto (string)
        // Salida: Task<string> mensaje de resultado
        // Descripcion: Solicita tomar un objeto del lugar actual y actualiza estado.
        public async Task<string> TomarAsync(string objeto)
        {
            if (string.IsNullOrWhiteSpace(objeto))
                return "Selecciona un objeto válido.";

            var body = new { objeto = objeto };
            string resultado = await PostJsonAsync("/tomar", body);
            await ActualizarEstadoAsync();
            return resultado;
        }

        // Nombre: ReiniciarJuegoAsync
        // Entrada: (ninguna)
        // Salida: Task<string> mensaje de reinicio o error
        // Descripcion: Llama al endpoint de reinicio y refresca estado inicial.
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
                return $"Error al reiniciar: {ex.Message}";
            }
        }

        // Nombre: DondeEstoyAsync
        // Entrada: (ninguna)
        // Salida: Task<string> mensaje con ubicación actual
        // Descripcion: Consulta textual de la ubicación mediante endpoint donde_estoy.
        public async Task<string> DondeEstoyAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/donde_estoy");
                var json = JsonDocument.Parse(response);
                return json.RootElement.GetProperty("mensaje").GetString() ?? "(Sin mensaje)";
            }
            catch (Exception ex)
            {
                return $"Error al obtener ubicación: {ex.Message}";
            }
        }

        // Nombre: ObtenerInventarioAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> lista de objetos en inventario
        // Descripcion: Recupera inventario completo desde /estado (sin alterar eventos).
        public async Task<List<string>> ObtenerInventarioAsync()
        {
            var response = await _httpClient.GetStringAsync($"{_urlBase}/estado");
            List<string> lista = new List<string>();
            using (var json = JsonDocument.Parse(response))
            {
                var invJson = json.RootElement.GetProperty("inventario");
                foreach (var item in invJson.EnumerateArray())
                    lista.Add(item.GetString() ?? string.Empty);
            }
            return lista;
        }


        // Nombre: ObtenerTodosLosObjetosAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> unión de inventario y objetos en el lugar
        // Descripcion: Combina objetos del lugar actual con los del inventario (distintos).
        public async Task<List<string>> ObtenerTodosLosObjetosAsync()
        {
            var enLugar = await ObtenerObjetosEnLugarAsync();
            var inv = await ObtenerInventarioAsync();
            var todos = enLugar.Concat(inv).Distinct().ToList();
            return todos;
        }

        // Nombre: DondeEstaAsync
        // Entrada: objeto (string)
        // Salida: Task<string> mensaje sobre ubicación del objeto
        // Descripcion: Consulta al servidor para saber dónde se encuentra un objeto.
        public async Task<string> DondeEstaAsync(string objeto)
        {
            var body = new { objeto };
            return await PostJsonAsync($"{_urlBase}/donde_esta", body);
        }

        // Nombre: PuedoIrAsync
        // Entrada: destino (string)
        // Salida: Task<string> mensaje de verificación
        // Descripcion: Verifica si el jugador podría desplazarse al destino sin moverlo.
        public async Task<string> PuedoIrAsync(string destino)
        {
            var body = new { destino };
            return await PostJsonAsync($"{_urlBase}/puedo_ir", body);
        }
        // ==============================
        // Consultas especiales
        // ==============================

        // Nombre: ObtenerLugaresVisitadosAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> lista de lugares visitados
        // Descripcion: Sincroniza y devuelve los lugares visitados desde el servidor.
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
                Console.WriteLine($"Error al obtener lugares visitados: {ex.Message}");
                return new List<string>();
            }
        }

        // Nombre: ObtenerObjetosEnLugarAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> objetos presentes en la ubicación actual
        // Descripcion: Consulta /objetos_lugar y actualiza la caché local.
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
                Console.WriteLine($"Error al obtener objetos del lugar: {ex.Message}");
                return new List<string>();
            }
        }

        // Nombre: ObtenerCaminosAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> destinos conectados
        // Descripcion: Recupera caminos posibles desde la ubicación actual y actualiza estado.
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
                Console.WriteLine($"Error al obtener caminos: {ex.Message}");
                return new List<string>();
            }
        }

        // Nombre: VerificarGaneAsync
        // Entrada: (ninguna)
        // Salida: Task<string> mensaje indicando si ganó o no
        // Descripcion: Llama a /verifica_gane para evaluar condición de victoria.
        public async Task<string> VerificarGaneAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("/verifica_gane");
                var json = JsonDocument.Parse(response);
                return json.RootElement.GetProperty("mensaje").GetString() ?? "(Sin mensaje)";
            }
            catch (Exception ex)
            {
                return $"Error al verificar gane: {ex.Message}";
            }
        }

        // Nombre: ComoGanoAsync
        // Entrada: (ninguna)
        // Salida: Task<List<string>> lista de mensajes con objetivos/rutas
        // Descripcion: Consulta /como_gano para obtener consejos de victoria.
        public async Task<List<string>> ComoGanoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/como_gano");
                var jsonString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(jsonString))
                    return new List<string> { "Sin respuesta del servidor." };

                using (var json = JsonDocument.Parse(jsonString))
                {
                    var root = json.RootElement;

                    if (root.TryGetProperty("mensajes", out var mensajesProp) && mensajesProp.ValueKind == JsonValueKind.Array)
                    {
                        var lista = new List<string>();
                        foreach (var item in mensajesProp.EnumerateArray())
                            lista.Add(item.GetString() ?? "");
                        return lista;
                    }

                    return new List<string> { "Respuesta inesperada del servidor." };
                }
            }
            catch (Exception ex)
            {
                return new List<string> { $"Error al comunicarse con Prolog: {ex.Message}" };
            }
        }


        // ==============================
        // Utilidades HTTP
        // ==============================

        // Nombre: PostJsonAsync
        // Entrada: url (string), data (object anónimo serializable)
        // Salida: Task<string> mensaje interpretado del JSON o error
        // Descripcion: Envía POST JSON, parsea respuesta y extrae campos estándar (mensaje/resultado/error).
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
                    var dto = JsonSerializer.Deserialize<MessageDto>(jsonString);

                    if (!string.IsNullOrWhiteSpace(dto.Mensaje))
                        return dto.Mensaje;

                    if (!string.IsNullOrWhiteSpace(dto.Resultado))
                        return dto.Resultado;

                    if (!string.IsNullOrWhiteSpace(dto.Error))
                        return $"Error del servidor: {dto.Error}";

                    return jsonString; // Fallback
                }
                catch (JsonException)
                {
                    // Si la respuesta no es JSON válido
                    return $"Respuesta no JSON: {jsonString}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al comunicarse con Prolog: {ex.Message}";
            }
        }

    }
}
