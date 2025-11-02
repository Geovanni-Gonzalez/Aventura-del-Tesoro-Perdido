using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Aventura.Model;

namespace Aventura.Controller
{
    public class GameController : IDisposable
    {
        private readonly string prologExe = @"C:\Program Files\swipl\bin\swipl.exe";
        private readonly string prologFile;
        private Process prologProcess;
        private StreamWriter prologInput;
        private StreamReader prologOutput;
        private StreamReader prologError;
        private readonly object prologLock = new object();

        public GameState gameState = new GameState();
        public event Action<GameState> OnGameStateUpdated;

        public GameController()
        {
            prologFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServidorProlog.pl");
            if (!File.Exists(prologFile))
                throw new FileNotFoundException("No se encontró ServidorProlog.pl en: " + prologFile);

            StartPrologSession();

            gameState.PlayerName = "Explorador";
            gameState.Score = 0;
            gameState.CurrentLocation = ObtenerJugadorDesdeProlog() ?? "bosque";
            gameState.Inventory = ObtenerInventarioDesdeProlog();
        }

        private void StartPrologSession()
        {
            if (!File.Exists(prologExe))
                throw new FileNotFoundException("No se encontró swipl.exe en la ruta: " + prologExe);

            var psi = new ProcessStartInfo
            {
                FileName = prologExe,
                Arguments = "-q",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            prologProcess = new Process { StartInfo = psi };
            prologProcess.Start();

            prologInput = prologProcess.StandardInput;
            prologOutput = prologProcess.StandardOutput;
            prologError = prologProcess.StandardError;

            string ruta = prologFile.Replace("\\", "/");
            SendRawCommand($"consult('{ruta}').");
        }

        // 🔹 Nuevo método público para notificar la UI
        public void NotifyStateChanged()
        {
            OnGameStateUpdated?.Invoke(gameState);
        }

        // 🔹 Nuevo método público GetInventory() para la UI
        public List<string> GetInventory()
        {
            return new List<string>(gameState.Inventory);
        }

        private string SendRawCommand(string command)
        {
            lock (prologLock)
            {
                string cmd = command.Trim();
                if (!cmd.EndsWith("."))
                    cmd += ".";

                prologInput.WriteLine(cmd);
                prologInput.Flush();

                var sb = new StringBuilder();
                string line;
                var sw = Stopwatch.StartNew();

                while (true)
                {
                    if (!prologError.EndOfStream)
                    {
                        string errline = prologError.ReadLine();
                        if (!string.IsNullOrEmpty(errline))
                            sb.AppendLine(errline);
                    }

                    if (prologOutput.Peek() >= 0)
                    {
                        line = prologOutput.ReadLine();
                        if (line == null) break;
                        if (line.Contains("__END__")) break;
                        sb.AppendLine(line);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }

                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        sb.AppendLine("error: Timeout leyendo respuesta de Prolog.");
                        break;
                    }
                }

                return sb.ToString().Trim();
            }
        }

        private string SendRawCommandWithEndMarker(string cmd)
        {
            string wrapped = $"( (call(({cmd}))) ; true ), write('__END__'), nl.";
            return SendRawCommand(wrapped);
        }

        public string EjecutarComando(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "warn: Comando vacío.";

            string clean = command.Trim().TrimEnd('.');
            string rawResponse = SendRawCommandWithEndMarker(clean);

            if (string.IsNullOrWhiteSpace(rawResponse))
                return "warn: Sin respuesta de Prolog.";

            string[] lines = rawResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string firstLine = lines.Length > 0 ? lines[0] : "";

            bool esOk = firstLine.StartsWith("ok:");
            bool esWarn = firstLine.StartsWith("warn:");
            bool esError = firstLine.StartsWith("error:");

            string salidaLimpia = firstLine;
            if (esOk) salidaLimpia = firstLine.Substring(3).Trim();
            else if (esWarn) salidaLimpia = "⚠️ " + firstLine.Substring(5).Trim();
            else if (esError) salidaLimpia = "❌ " + firstLine.Substring(6).Trim();

            if (esOk)
            {
                if (clean.StartsWith("mover", StringComparison.OrdinalIgnoreCase))
                {
                    string dest = ExtraerParametro(clean);
                    if (!string.IsNullOrEmpty(dest))
                        gameState.CurrentLocation = dest;
                    gameState.Inventory = ObtenerInventarioDesdeProlog();
                }
                else if (clean.StartsWith("tomar", StringComparison.OrdinalIgnoreCase))
                {
                    string obj = ExtraerParametro(clean);
                    if (!string.IsNullOrEmpty(obj) && !gameState.Inventory.Contains(obj))
                    {
                        gameState.Inventory.Add(obj);
                        gameState.Score += 10;
                    }
                }
                else if (clean.StartsWith("reiniciar", StringComparison.OrdinalIgnoreCase))
                {
                    gameState = new GameState();
                }

                NotifyStateChanged();
            }

            return salidaLimpia;
        }

        public List<string> GetLugares()
        {
            string resp = SendRawCommandWithEndMarker("findall(N, lugar(N,_), L), write(L)");
            return ParsearListaProlog(resp);
        }

        public List<string> GetObjetosEnLugarActual()
        {
            string resp = SendRawCommandWithEndMarker("jugador(J), findall(O, objeto(O,J), R), write(R)");
            return ParsearListaProlog(resp);
        }

        private List<string> ObtenerInventarioDesdeProlog()
        {
            string resp = SendRawCommandWithEndMarker("inventario(I), write(I)");
            return ParsearListaProlog(resp);
        }

        private string ObtenerJugadorDesdeProlog()
        {
            string resp = SendRawCommandWithEndMarker("jugador(J), write(J)");
            if (string.IsNullOrWhiteSpace(resp)) return null;
            string line = resp.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return line;
        }

        private List<string> ParsearListaProlog(string prologOutput)
        {
            var res = new List<string>();
            if (string.IsNullOrWhiteSpace(prologOutput)) return res;

            int start = prologOutput.IndexOf('[');
            int end = prologOutput.IndexOf(']');
            if (start == -1 || end == -1 || end <= start) return res;
            string inner = prologOutput.Substring(start + 1, end - start - 1).Trim();

            if (string.IsNullOrEmpty(inner)) return res;

            foreach (var p in inner.Split(','))
            {
                string s = p.Trim();
                if (s.StartsWith("'") && s.EndsWith("'"))
                    s = s.Substring(1, s.Length - 2);
                res.Add(s);
            }
            return res;
        }

        private static string ExtraerParametro(string command)
        {
            int s = command.IndexOf('(') + 1;
            int e = command.IndexOf(')');
            if (s > 0 && e > s) return command.Substring(s, e - s);
            return "";
        }

        public void Dispose()
        {
            try
            {
                if (prologProcess != null && !prologProcess.HasExited)
                {
                    prologInput.WriteLine("halt.");
                    prologInput.Flush();
                    if (!prologProcess.WaitForExit(1000))
                        prologProcess.Kill();
                }
            }
            catch { }
            finally
            {
                prologInput?.Dispose();
                prologOutput?.Dispose();
                prologError?.Dispose();
                prologProcess?.Dispose();
            }
        }
    }
}
