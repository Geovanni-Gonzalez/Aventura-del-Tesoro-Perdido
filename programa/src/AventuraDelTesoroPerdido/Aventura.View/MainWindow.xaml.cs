using Aventura.Controller;
using Aventura.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Aventura.View
{
    public partial class MainWindow : Window
    {
        private GameController gameController;

        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController();
            gameController.OnGameStateUpdated += ActualizarUI_OnGameStateUpdated;

            try
            {
                gameController.InicializarMotor();
                gameController.ActualizarEstado();
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
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            gameController?.FinalizarMotor();
        }

        private void ActualizarUI_OnGameStateUpdated(GameState estado)
        {
            Dispatcher.Invoke(() => ActualizarUI(estado));
        }

        private void ActualizarUI(GameState estado)
        {
            if (estado == null) return;

            string inventarioStr = (estado.Inventory != null && estado.Inventory.Count > 0)
                                    ? string.Join(", ", estado.Inventory)
                                    : "(Vacío)";
            EstadoTxt.Text = $"Lugar: {estado.CurrentPlace ?? "Desconocido"} | Inventario: {inventarioStr}";


            LstInventario.ItemsSource = null;
            if (estado.Inventory != null && estado.Inventory.Count > 0)
            {
                LstInventario.ItemsSource = estado.Inventory;
            }
        }

        private void MostrarMensaje(string mensaje)
        {
            if (!string.IsNullOrEmpty(mensaje) && !mensaje.Equals("(Sin mensaje)"))
            {
                MessageBox.Show(mensaje, "Información del Juego");
            }
        }

        private void CmbMover_DropDownOpened(object sender, EventArgs e)
        {
            gameController.ActualizarEstado();
            CmbMover.ItemsSource = gameController.Estado.AvailablePlaces;
        }

        private void CmbMover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No hacemos nada aquí, la acción la dispara el botón "Mover".
        }

        private void CmbTomar_DropDownOpened(object sender, EventArgs e)
        {
            // Rellena el ComboBox con los objetos en el lugar actual
            gameController.ActualizarEstado(); // Asegura que la lista esté al día
            CmbTomar.ItemsSource = gameController.Estado.AvailableObjects;
        }

        private void CmbTomar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No hacemos nada aquí, la acción la dispara el botón "Tomar".
        }

        private void CmbUsar_DropDownOpened(object sender, EventArgs e)
        {
            // Rellena el ComboBox con los objetos del inventario
            gameController.ActualizarEstado(); // Asegura que la lista esté al día
            CmbUsar.ItemsSource = gameController.Estado.Inventory;
        }

        private void CmbUsar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No hacemos nada aquí, la acción la dispara el botón "Usar".
        }

        // --- BOTONES DE ACCIÓN (PANEL IZQUIERDO) ---

        // Esta función ahora SÍ está conectada al botón "Mover"
        private void BtnMover_Click(object sender, RoutedEventArgs e)
        {
            string destino = CmbMover.SelectedItem as string;
            if (!string.IsNullOrEmpty(destino))
            {
                string mensaje = gameController.MoverA(destino);
                MostrarMensaje(mensaje);
                CmbMover.SelectedIndex = -1; // Limpia la selección
                CmbMover.ItemsSource = null; // Limpia la lista para forzar recarga
            }
            else
            {
                MostrarMensaje("Por favor, selecciona un destino del ComboBox 'Mover a:'.");
            }
        }

        // Esta función ahora SÍ está conectada al botón "Tomar"
        private void BtnTomar_Click(object sender, RoutedEventArgs e)
        {
            string objeto = CmbTomar.SelectedItem as string;
            if (!string.IsNullOrEmpty(objeto))
            {
                string mensaje = gameController.Tomar(objeto);
                MostrarMensaje(mensaje);
                CmbTomar.SelectedIndex = -1; // Limpia la selección
                CmbTomar.ItemsSource = null; // Limpia la lista para forzar recarga
            }
            else
            {
                MostrarMensaje("Por favor, selecciona un objeto del ComboBox 'Tomar objeto:'.");
            }
        }

        // Esta función ahora SÍ está conectada al botón "Usar"
        private void BtnUsar_Click(object sender, RoutedEventArgs e)
        {
            string objeto = CmbUsar.SelectedItem as string;
            if (!string.IsNullOrEmpty(objeto))
            {
                string mensaje = gameController.Usar(objeto);
                MostrarMensaje(mensaje);
                CmbUsar.SelectedIndex = -1; // Limpia la selección
                CmbUsar.ItemsSource = null; // Limpia la lista para forzar recarga
            }
            else
            {
                MostrarMensaje("Por favor, selecciona un objeto del ComboBox 'Usar objeto'.");
            }
        }

        // --- OTROS BOTONES (PANEL IZQUIERDO) ---

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            gameController.ActualizarEstado();
            gameController.NotifyStateChanged();
            MostrarMensaje("Estado actualizado.");
        }

        private void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una función 'gameController.VerificarGane()'
            MostrarMensaje("Función 'Verificar Gane' no implementada.");
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Necesitas una función 'gameController.ReiniciarJuego()'
            MostrarMensaje("Función 'Reiniciar' no implementada.");
        }

        // --- MANEJADORES DE LA BARRA INFERIOR ---
        // (Estas funciones faltaban y causaban los errores de la captura)

        private void BtnDondeEstoy_Click(object sender, RoutedEventArgs e)
        {
            MostrarMensaje($"Actualmente estás en: {gameController.Estado.CurrentPlace}");
        }

        private void BtnQueTengo_Click(object sender, RoutedEventArgs e)
        {
            string inventarioStr = (gameController.Estado.Inventory != null && gameController.Estado.Inventory.Count > 0)
                                    ? string.Join(", ", gameController.Estado.Inventory)
                                    : "(Inventario vacío)";
            MostrarMensaje($"Tienes: {inventarioStr}");
        }

        private void BtnLugaresVisitados_Click(object sender, RoutedEventArgs e)
        {
            List<string> visitados = gameController.ObtenerLugaresVisitados();
            string visitadosStr = (visitados != null && visitados.Count > 0)
                                  ? string.Join(", ", visitados)
                                  : "Aún no has visitado ningún lugar.";
            MostrarMensaje($"Lugares Visitados: {visitadosStr}");
        }

        // --- COMBOBOX "Donde Esta" (BARRA INFERIOR) ---

        // Corregido de "DropUpOpened" a "DropDownOpened"
        private void CmbDondeEsta_DropDownOpened(object sender, EventArgs e)
        {
            // Rellena la lista de TODOS los objetos del juego
            if (CmbDondeEsta.Items.Count == 0)
            {
                List<string> todosObjetos = gameController.ObtenerTodosLosObjetos();
                if (todosObjetos != null && todosObjetos.Count > 0)
                {
                    CmbDondeEsta.ItemsSource = todosObjetos;
                }
            }
        }

        private void CmbDondeEsta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDondeEsta.SelectedItem is string objetoSeleccionado)
            {
                string ubicacion = gameController.UbicacionDe(objetoSeleccionado);
                MostrarMensaje(ubicacion);
                CmbDondeEsta.SelectedIndex = -1; // Limpia para permitir seleccionar de nuevo
            }
        }

        // --- COMBOBOX "Puedo Ir A" (BARRA INFERIOR) ---

        // Evento NUEVO (estaba mal en tu XAML)
        private void CmbPuedoIr_DropDownOpened(object sender, EventArgs e)
        {
            // Rellena con los lugares disponibles
            gameController.ActualizarEstado(); // Asegura que la lista esté al día
            CmbPuedoIr.ItemsSource = gameController.Estado.AvailablePlaces;
        }

        // Evento NUEVO (estaba mal en tu XAML)
        private void CmbPuedoIr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Mueve al jugador al lugar seleccionado
            if (CmbPuedoIr.SelectedItem is string destino)
            {
                string mensaje = gameController.MoverA(destino);
                MostrarMensaje(mensaje);
                CmbPuedoIr.SelectedIndex = -1; // Limpia la selección
                CmbPuedoIr.ItemsSource = null; // Limpia la lista para forzar recarga
            }
        }
    }
}