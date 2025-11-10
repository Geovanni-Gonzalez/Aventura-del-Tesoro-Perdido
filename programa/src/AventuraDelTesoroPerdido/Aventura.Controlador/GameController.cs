using SbsSW.SwiPlCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aventura.Model;

namespace Aventura.Controller
{
    public class GameController
    {
        private static bool motorIniciado = false;
        public event Action<Aventura.Model.GameState> OnGameStateUpdated;

        public GameState Estado { get; private set; }

        public GameController()
        {
            Estado = new GameState
            {
                CurrentPlace = "",
                AvailablePlaces = new List<string>(),
                Inventory = new List<string>(),
                AvailableObjects = new List<string>()
            };
        }

        public void NotifyStateChanged()
        {
            OnGameStateUpdated?.Invoke(Estado);
        }

        // === Inicializa el motor Prolog y carga los archivos ===
        public void InicializarMotor()
        {
            if (!motorIniciado)
            {
                string baseDir = Path.Combine(Directory.GetCurrentDirectory(), "PrologFiles");

                string[] parametros = { "-q", "-f", "none" };
                PlEngine.Initialize(parametros);

                // Cargar archivos Prolog
                PlQuery.PlCall($"consult('{Path.Combine(baseDir, "ServidorProlog.pl").Replace("\\", "/")}').");

                motorIniciado = true;
                Console.WriteLine("Motor Prolog inicializado.");
            }
        }

        // === Finaliza motor ===
        public void FinalizarMotor()
        {
            if (motorIniciado && PlEngine.IsInitialized)
            {
                PlEngine.PlCleanup();
                motorIniciado = false;
                Console.WriteLine("Motor Prolog cerrado.");
            }
        }

        // === Ejecuta una consulta y obtiene el mensaje guardado ===
        private string EjecutarConsulta(string consulta)
        {
            if (!motorIniciado) InicializarMotor();

            new PlQuery("retractall(message(_))").NextSolution();

            try
            {
                using (var q = new PlQuery($"{consulta.TrimEnd('.')}."))
                {
                    q.NextSolution();
                }

                using (var q2 = new PlQuery("message(Msg)."))
                {
                    if (q2.NextSolution())
                        return q2.Variables["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error ejecutando {consulta}: {ex.Message}";
            }

            return "(Sin mensaje)";
        }

        // === Ejecuta consulta que devuelve lista (como inventario o lugares) ===
        private List<string> EjecutarConsultaLista(string consulta)
        {
            var lista = new List<string>();
            string msg = EjecutarConsulta(consulta);

            if (msg.StartsWith("["))
            {
                msg = msg.Trim('[', ']');
                lista = msg.Split(',')
                           .Select(x => x.Trim())
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .ToList();
            }
            else if (!string.IsNullOrEmpty(msg))
            {
                lista.Add(msg);
            }

            return lista;
        }

        // === Actualiza el estado del juego desde Prolog ===
        public void ActualizarEstado()
        {
            Estado.CurrentPlace = EjecutarConsulta("donde_estoy");
            Estado.Inventory = EjecutarConsultaLista("que_tengo");
            Estado.AvailablePlaces = ObtenerLugaresPosibles();
            Estado.AvailableObjects = ObtenerObjetosEnLugarActual();
        }

        // === Mover al jugador ===
        public string MoverA(string destino)
        {
            string mensaje = EjecutarConsulta($"mover({destino})");
            ActualizarEstado();
            NotifyStateChanged();
            return mensaje;
        }

        // === Tomar un objeto ===
        public string Tomar(string objeto)
        {
            string mensaje = EjecutarConsulta($"tomar({objeto})");
            ActualizarEstado();
            NotifyStateChanged();
            return mensaje;
        }

        // === Usar un objeto ===
        public string Usar(string objeto)
        {
            string mensaje = EjecutarConsulta($"usar({objeto})");
            ActualizarEstado();
            NotifyStateChanged();
            return mensaje;
        }

        // === Obtener lugares posibles a visitar ===
        public List<string> ObtenerLugaresPosibles()
        {
            var lugares = new List<string>();

            try
            {
                using (var q = new PlQuery("jugador(LugarActual)."))
                {
                    if (q.NextSolution())
                    {
                        string lugarActual = q.Variables["LugarActual"].ToString();
                        using (var q2 = new PlQuery($"conectado({lugarActual}, LugarDestino)."))
                        {
                            foreach (PlQueryVariables v in q2.SolutionVariables)
                            {
                                lugares.Add(v["LugarDestino"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lugares.Add($"Error: {ex.Message}");
            }

            return lugares.Distinct().ToList();
        }

        // === NUEVO: Lugares visitados (usa el predicado lugar_visitados que guarda message(Lista)) ===
        public List<string> ObtenerLugaresVisitados()
        {
            return EjecutarConsultaLista("lugar_visitados");
        }

        // === NUEVO: Todos los objetos del juego (recorriendo objeto(Obj, _)) ===
        public List<string> ObtenerTodosLosObjetos()
        {
            var objetos = new List<string>();
            try
            {
                using (var q = new PlQuery("objeto(Obj, _Lugar)."))
                {
                    foreach (PlQueryVariables v in q.SolutionVariables)
                    {
                        objetos.Add(v["Obj"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                objetos.Add($"Error: {ex.Message}");
            }
            return objetos.Distinct().ToList();
        }

        // === NUEVO: Objetos en el lugar actual del jugador ===
        public List<string> ObtenerObjetosEnLugarActual()
        {
            var lista = new List<string>();
            try
            {
                using (var q = new PlQuery("jugador(LActual)."))
                {
                    if (q.NextSolution())
                    {
                        string lugarActual = q.Variables["LActual"].ToString();
                        using (var q2 = new PlQuery($"objeto(Obj, {lugarActual})."))
                        {
                            foreach (PlQueryVariables v in q2.SolutionVariables)
                            {
                                lista.Add(v["Obj"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lista.Add($"Error: {ex.Message}");
            }
            return lista.Distinct().ToList();
        }

        // === NUEVO: Ubicación de un objeto (usa donde_esta/1 que guarda message) ===
        public string UbicacionDe(string objeto)
        {
            return EjecutarConsulta($"donde_esta({objeto})");
        }
    }
}
