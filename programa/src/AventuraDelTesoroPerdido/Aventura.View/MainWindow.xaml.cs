using Aventura.Controller;
using Aventura.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Aventura.View
{
    public partial class MainWindow : Window
    {
        private readonly GameController gameController;

        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController();
            gameController.OnGameStateUpdated += ActualizarUI_OnGameStateUpdated;
            CargarEstadoInicialAsync();
        }

        private async void CargarEstadoInicialAsync()
        {
            try
            {
                await gameController.ActualizarEstadoAsync();
                ActualizarUI_OnGameStateUpdated(gameController.Estado);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar el estado inicial: {ex.Message}");
            }
        }

        private void ActualizarUI_OnGameStateUpdated(GameState estado)
        {
            EstadoTxt.Text = $"📍 Lugar: {estado.ubicacion ?? "Desconocido"}";

            // Inventario
            LstInventario.ItemsSource = null;
            LstInventario.ItemsSource = estado.inventario ?? new List<string>();

            // Combos dinámicos
            CmbMover.ItemsSource = estado.caminosPosibles ?? new List<string>();
            CmbUsar.ItemsSource = estado.inventario ?? new List<string>();
            CmbTomar.ItemsSource = estado.objetosEnLugar ?? new List<string>();
        }

        private void MostrarMensaje(string mensaje)
        {
            if (!string.IsNullOrWhiteSpace(mensaje) && !mensaje.Equals("(Sin mensaje)"))
                MessageBox.Show(mensaje);
        }

        // 🔄 Refrescar estado
        private async void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            await gameController.ActualizarEstadoAsync();
        }

        // 🎒 Inventario
        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            var inv = gameController.Estado.inventario;
            string mensaje = inv != null && inv.Count > 0
                ? $"🎒 Inventario: {string.Join(", ", inv)}"
                : "🎒 Inventario vacío.";
            MostrarMensaje(mensaje);
        }

        // 🚶 Mover a destino
        private async void BtnMover_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMover.SelectedItem is string destino)
            {
                string resultado = await gameController.MoverAAsync(destino);
                MostrarMensaje(resultado);
                await gameController.ActualizarEstadoAsync();
            }
            else
            {
                MostrarMensaje("Selecciona un destino válido.");
            }
        }

        // 🤲 Tomar objeto
        private async void BtnTomar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTomar.SelectedItem is string objeto)
            {
                string resultado = await gameController.TomarAsync(objeto);
                MostrarMensaje(resultado);
            }
            else
            {
                MostrarMensaje("Selecciona un objeto para tomar.");
            }
        }

        // 🪄 Usar objeto
        private async void BtnUsar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbUsar.SelectedItem is string objeto)
            {
                string resultado = await gameController.UsarAsync(objeto);
                MostrarMensaje(resultado);
                await gameController.ActualizarEstadoAsync();
            }
            else
            {
                MostrarMensaje("Selecciona un objeto para usar.");
            }
        }

        // 📜 Lugares visitados
        private async void BtnLugaresVisitados_Click(object sender, RoutedEventArgs e)
        {
            var lugares = await gameController.ObtenerLugaresVisitadosAsync();

            if (lugares == null || lugares.Count == 0)
                MostrarMensaje("📍 Aún no has visitado ningún lugar.");
            else
                MostrarMensaje($"🏞️ Lugares visitados:\n{string.Join(", ", lugares)}");
        }

        // 🧭 Objetos en el lugar actual
        private async void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
            var objetos = await gameController.ObtenerObjetosEnLugarAsync();

            if (objetos == null || objetos.Count == 0)
                MostrarMensaje("📦 No hay objetos visibles en este lugar.");
            else
                MostrarMensaje($"🧭 Objetos en {gameController.Estado.ubicacion}:\n{string.Join(", ", objetos)}");
        }

        // 🔁 Reiniciar juego
        private async void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            string resultado = await gameController.ReiniciarJuegoAsync();
            MostrarMensaje(resultado);
            await gameController.ActualizarEstadoAsync();
        }

        // 🌍 ComboBox dinámico para caminos posibles
        private async void CmbMover_DropDownOpened(object sender, EventArgs e)
        {
            var caminos = await gameController.ObtenerCaminosAsync();
            CmbMover.ItemsSource = caminos ?? new List<string>();
        }

        private void CmbMover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMover.SelectedItem is string destino)
                EstadoTxt.Text = $"➡️ Vas hacia: {destino}";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Aquí puedes agregar la lógica que necesites al cerrar la ventana, por ejemplo:
            // Guardar estado, liberar recursos, etc.
        }

        private async void CmbTomar_DropDownOpened(object sender, EventArgs e)
        {
            var objetos = await gameController.ObtenerObjetosEnLugarAsync();
            CmbTomar.ItemsSource = objetos ?? new List<string>();
        }

        private void CmbTomar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que debe ejecutarse cuando cambia la selección en el ComboBox CmbTomar.
            // Por ejemplo:
            // var seleccionado = CmbTomar.SelectedItem;
        }

        private void CmbUsar_DropDownOpened(object sender, EventArgs e)
        {
            var inventario = gameController.Estado.inventario;
            CmbUsar.ItemsSource = inventario ?? new List<string>();
        }

        private void CmbUsar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando cambie la selección
            // Por ejemplo, actualizar información relacionada con el objeto seleccionado
        }

        private void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            // Aquí va la lógica para verificar si el jugador ha ganado.
            MessageBox.Show("Funcionalidad de verificación de gane aún no implementada.");
        }

        private void BtnDondeEstoy_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes agregar la lógica que desees para el botón "Donde estoy"
            MessageBox.Show("Estás en el lugar actual de la aventura.");
        }

        private void BtnQueTengo_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando se haga clic en el botón "Que tengo"
            MessageBox.Show("Inventario actual:\n" + string.Join("\n", LstInventario.Items.Cast<string>()));
        }

        private void CmbDondeEsta_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes agregar la lógica para actualizar el ComboBox si es necesario
        }

        private void CmbDondeEsta_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando cambie la selección
            // Por ejemplo, actualizar información relacionada con el lugar seleccionado
        }

        private void CmbPuedoIr_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes agregar la lógica para actualizar el ComboBox si es necesario
        }

        private void CmbPuedoIr_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que necesites cuando cambie la selección
        }
    }
}
