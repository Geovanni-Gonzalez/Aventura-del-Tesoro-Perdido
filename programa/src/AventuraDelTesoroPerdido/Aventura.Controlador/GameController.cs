using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Aventura.Controller
{
    public class GameController : IDisposable
    {
        private readonly string prologExe = @"C:\Program Files\swipl\bin\swipl.exe";
        private readonly string prologFile;

        public string PlayerName { get; set; }
        public string CurrentLocation { get; private set; }
        public List<string> Inventory { get; private set; }
        public int Score { get; private set; }

        public GameController()
        {
            PlayerName = "Explorador";
            CurrentLocation = "bosque";  // Estado inicial
            Inventory = new List<string>();
            Score = 0;

            // Intentar encontrar ServidorProlog.pl en el directorio de salida
            prologFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServidorProlog.pl");

            if (!File.Exists(prologFile))
            {
                throw new FileNotFoundException(
                    "❌ No se encontró el archivo ServidorProlog.pl en el directorio de salida.\n" +
                    $"Ruta buscada: {prologFile}\n" +
                    "Asegúrate de copiar ServidorProlog.pl al directorio bin/Debug de este proyecto.");
            }
        }

        public string EjecutarComando(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "⚠️ Comando vacío.";

            string cleanCommand = command.Trim().TrimEnd('.');
            string prologCmd = $"-q -g \"consult('{prologFile.Replace("\\", "/")}'), {cleanCommand}, halt.\"";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = prologExe,
                Arguments = prologCmd,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                Process proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    return "Error Prolog: " + error.Trim();

                output = output.Trim();
                if (cleanCommand.StartsWith("mover"))
                    ActualizarUbicacion(cleanCommand);
                else if (cleanCommand.StartsWith("tomar"))
                    ActualizarInventario(cleanCommand);

                return string.IsNullOrEmpty(output) ? "✅ OK" : output;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        private void ActualizarUbicacion(string command)
        {
            int start = command.IndexOf('(') + 1;
            int end = command.IndexOf(')');
            if (start > 0 && end > start)
                CurrentLocation = command.Substring(start, end - start);
        }

        private void ActualizarInventario(string command)
        {
            int start = command.IndexOf('(') + 1;
            int end = command.IndexOf(')');
            if (start > 0 && end > start)
            {
                string obj = command.Substring(start, end - start);
                if (!Inventory.Contains(obj))
                {
                    Inventory.Add(obj);
                    Score += 10;
                }
            }
        }

        public string PlayerInfo => $"{PlayerName} - Puntos: {Score} - Lugar: {CurrentLocation}";

        public List<string> GetInventory() => new List<string>(Inventory);

        public void Dispose() { }
    }
}
