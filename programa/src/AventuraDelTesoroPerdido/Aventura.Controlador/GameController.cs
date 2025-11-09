using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Aventura.Model;
using SbsSW.SwiPlCs;

namespace Aventura.Controller
{
    public class GameController
    {
        public void EjecutarConsulta()
        {
            // Asegúrate de usar la ruta correcta de instalación de SWI-Prolog
            string[] initParams = { "-q", "-f", "none", "-g", "true" };
            PlEngine.Initialize(initParams);

            try
            {
                // Cargar archivo Prolog
                PlQuery.PlCall("consult('ServidorProlog.pl')");
                // Ejemplo de consulta
                using (var q = new PlQuery("padre(juan, X)"))
                {
                    foreach (PlQueryVariables v in q.SolutionVariables)
                    {
                        Console.WriteLine($"X = {v["X"]}");
                    }
                }
            }
            finally
            {
                PlEngine.PlCleanup();
            }
        }
    }
}
