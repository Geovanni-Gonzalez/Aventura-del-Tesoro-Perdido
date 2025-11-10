using Aventura.Controller;
using Aventura.Model;
using System;
using System.Windows;
using System.Windows.Controls;

// Asegúrate de que el namespace coincide con el de tu proyecto de Vista/UI
namespace Aventura.View
{
    public partial class MainWindow : Window
    {
        private GameController gameController;

        public MainWindow()
        {
            InitializeComponent();

            gameController = new GameController();

            // Suscribirse al evento de actualización de estado
            gameController.OnGameStateUpdated += ActualizarUI_OnGameStateUpdated;

            // Inicializar el motor y cargar el estado inicial
            try
            {
                gameController.InicializarMotor();
                gameController.ActualizarEstado();

                // Actualizar la UI con el estado inicial
                ActualizarUI(gameController.Estado);
                MostrarMensaje("Motor Prolog inicializado. ¡Comienza la aventura!");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error crítico al inicializar el motor Prolog. " +
                                  $"Asegúrate de que los archivos .pl estén en la carpeta 'PrologFiles'.\n\n" +
                                  $"Detalle: {ex.Message}";

                MostrarMensaje(errorMsg);
                MessageBox.Show(errorMsg, "Error de Inicialización", MessageBoxButton.OK, MessageBoxImage.Error);

                // --- ERRORES ELIMINADOS ---
                // Se eliminaron las referencias a botones que no existen
                // (btnMover, btnTomar, btnUsar, txtObjetoTomar)
            }
        }

        // --- MANEJO DEL ESTADO DE LA VENTANA ---

        // Esta función AHORA SÍ se conecta con el XAML
        private void Window_Closed(object sender, EventArgs e)
        {
            // Limpiar el motor de Prolog al cerrar la aplicación
            gameController?.FinalizarMotor();
        }

        // --- ACTUALIZACIÓN DE LA UI ---

        // Event handler que se llama cuando el GameController notifica un cambio
        private void ActualizarUI_OnGameStateUpdated(GameState estado)
        {
            // El evento puede venir de otro hilo, usamos Dispatcher para asegurar
            // que la actualización de la UI ocurra en el hilo principal.
            Dispatcher.Invoke(() =>
            {
                ActualizarUI(estado);
            });
        }

        // Método helper principal para actualizar los controles de la UI
        // --- CORREGIDO ---
        private void ActualizarUI(GameState estado)
        {
            if (estado == null) return;

            // Actualizar barra de estado (Usa el control 'EstadoTxt' que SÍ existe en el XAML)
            string inventarioStr = (estado.Inventory != null && estado.Inventory.Count > 0)
                                    ? string.Join(", ", estado.Inventory)
                                    : "(Vacío)";

            // Actualiza el TextBlock 'EstadoTxt' de la barra superior
            EstadoTxt.Text = $"Lugar: {estado.CurrentPlace ?? "Desconocido"} | Inventario: {inventarioStr}";

            // TODO: (Próximos pasos)
            // Aquí deberás actualizar la posición de 'PersonajeImg' en el Canvas
            // y generar los botones de movimiento en 'LugarButtonsPanel'
            // basado en 'estado.AvailablePlaces'.

            // Se eliminó el código que actualizaba 'lblLugarActual', 'lstInventario' y 'lstLugares'
            // porque esos controles no existen en tu MainWindow.xaml
        }

        // Método helper para añadir mensajes a la consola del juego
        // --- CORREGIDO ---
        private void MostrarMensaje(string mensaje)
        {
            // Evitar mensajes vacíos o de placeholder
            if (!string.IsNullOrEmpty(mensaje) && !mensaje.Equals("(Sin mensaje)"))
            {
                // -- IMPORTANTE --
                // Tu XAML no tiene un TextBox llamado 'txtMensajes'.
                // Para solucionarlo, debes añadir un control <TextBox x:Name="txtMensajes"> a tu XAML.
                //
                // Como solución temporal, usamos un MessageBox:
                MessageBox.Show(mensaje);
            }
        }

        // --- MANEJADORES DE EVENTOS DE BOTONES (NUEVOS) ---
        // Estos son los métodos que tu XAML SÍ está buscando

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            gameController.ActualizarEstado();
            gameController.NotifyStateChanged(); // Forzamos la actualización de la UI
            MostrarMensaje("Estado actualizado.");
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            string inventarioStr = (gameController.Estado.Inventory != null && gameController.Estado.Inventory.Count > 0)
                                    ? string.Join(", ", gameController.Estado.Inventory)
                                    : "(Vacío)";
            MostrarMensaje($"Inventario: {inventarioStr}");
        }

        private void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una función en tu GameController que te diga
            // qué objetos hay en el 'gameController.Estado.CurrentPlace'
            MostrarMensaje("Función 'Objetos en Lugar' no implementada.");
        }

        private void BtnUsarObjeto_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una forma de seleccionar qué objeto usar
            // (quizás un ComboBox o un InputDialog)
            // string objeto = ... (objeto seleccionado)
            // if (!string.IsNullOrEmpty(objeto))
            // {
            //    string mensaje = gameController.Usar(objeto);
            //    MostrarMensaje(mensaje);
            // }
            MostrarMensaje("Función 'Usar Objeto' no implementada.");
        }

        private void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una función 'gameController.VerificarGane()'
            // que consulte a Prolog si se ha ganado el juego.
            MostrarMensaje("Función 'Verificar Gane' no implementada.");
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una función 'gameController.ReiniciarJuego()'
            // que llame a 'retractall' en Prolog y luego llame a 
            // gameController.ActualizarEstado() y gameController.NotifyStateChanged()
            MostrarMensaje("Función 'Reiniciar' no implementada.");
        }

        // --- MANEJADORES DE EVENTOS ANTIGUOS (ELIMINADOS) ---
        // Se eliminaron BtnMover_Click, BtnTomar_Click y BtnUsar_Click
        // porque no corresponden a los botones de tu XAML actual.
    }
}