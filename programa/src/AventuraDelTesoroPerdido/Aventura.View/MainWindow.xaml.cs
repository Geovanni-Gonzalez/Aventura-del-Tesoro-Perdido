using Aventura.Controller;
using Aventura.Model;
using System;
using System.Collections.Generic;
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

            // Iniciar carga asíncrona al iniciar la ventana
            Loaded += async (s, e) => await InicializarAsync();
        }

        // Inicializa el sistema de juego de manera asíncrona
        private async Task InicializarAsync()
        {
            try
            {
                MostrarMensaje("Inicializando conexión con Prolog...");

                // Probar conexión al servidor
                await gameController.ActualizarEstadoAsync();

                MostrarMensaje("✅ Conexión establecida. ¡Comienza la aventura!");
                ActualizarUI(gameController.Estado);
            }
            catch (Exception ex)
            {
                string errorMsg = $"❌ Error al conectar con el servidor Prolog.\n\n" +
                                  $"Asegúrate de haberlo iniciado con:\n" +
                                  $"swipl ServidorProlog.pl, luego ?- server(5000).\n\n" +
                                  $"Detalle: {ex.Message}";

                MostrarMensaje(errorMsg);
                MessageBox.Show(errorMsg, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Limpieza ---
        private void Window_Closed(object sender, EventArgs e)
        {
            // No es necesario limpiar motor, ya que es HTTP.
        }

        // --- Actualización de UI ---
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

            EstadoTxt.Text = $"📍 Lugar: {estado.CurrentPlace ?? "Desconocido"} | 🎒 Inventario: {inventarioStr}";
        }

        private void MostrarMensaje(string mensaje)
        {
            if (!string.IsNullOrEmpty(mensaje) && !mensaje.Equals("(Sin mensaje)"))
            {
                MessageBox.Show(mensaje);
            }
        }

        // --- EVENTOS DE BOTONES ---
        private async void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            await gameController.ActualizarEstadoAsync();
            MostrarMensaje("🔄 Estado actualizado.");
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            string inventarioStr = (gameController.Estado.Inventory != null && gameController.Estado.Inventory.Count > 0)
                ? string.Join(", ", gameController.Estado.Inventory)
                : "(Vacío)";
            MostrarMensaje($"🎒 Inventario actual: {inventarioStr}");
        }

        private async void BtnMover_Click(object sender, RoutedEventArgs e)
        {
            // Ejemplo: podrías obtener el destino desde un ComboBox o un input
            string destino = "playa"; // <- Reemplázalo por tu selección dinámica
            var mensaje = await gameController.MoverAAsync(destino);
            MostrarMensaje(mensaje);
        }

        private async void BtnTomar_Click(object sender, RoutedEventArgs e)
        {
            string objeto = "llave"; // <- Reemplázalo por tu selección dinámica
            var mensaje = await gameController.TomarAsync(objeto);
            MostrarMensaje(mensaje);
        }

        private async void BtnUsar_Click(object sender, RoutedEventArgs e)
        {
            string objeto = "llave"; // <- Reemplázalo por tu selección dinámica
            var mensaje = await gameController.UsarAsync(objeto);
            MostrarMensaje(mensaje);
        }

        private async void BtnLugaresVisitados_Click(object sender, RoutedEventArgs e)
        {
            var lugares = await gameController.ObtenerLugaresPosiblesAsync();
            MostrarMensaje($"🌍 Lugares posibles: {string.Join(", ", lugares)}");
        }

        private void BtnQueTengo_Click(object sender, RoutedEventArgs e)
        {
            BtnInventario_Click(sender, e);
        }

        private void BtnDondeEstoy_Click(object sender, RoutedEventArgs e)
        {
            MostrarMensaje($"📍 Estás en: {gameController.Estado.CurrentPlace}");
        }

        private void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            MostrarMensaje("Funcionalidad 'Verificar Gane' aún no implementada.");
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            MostrarMensaje("Funcionalidad 'Reiniciar' aún no implementada.");
        }

        private void CmbDondeEsta_DropDownOpened(object sender, EventArgs e)
        {
            // TODO: Cargar dinámicamente los lugares disponibles desde gameController.Estado.AvailablePlaces
        }

        private void CmbDondeEsta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Acción cuando se seleccione un nuevo lugar
        }

        private void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando se haga clic en el botón "Objetos en Lugar"
            MessageBox.Show("Funcionalidad de 'Objetos en Lugar' aún no implementada.");
        }

        private void BtnUsarObjeto_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para usar un objeto, puedes personalizar según tu aplicación
            MessageBox.Show("Funcionalidad 'Usar Objeto' aún no implementada.");
        }

        private void CmbMover_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes cargar dinámicamente los lugares disponibles desde gameController.Estado.AvailablePlaces
            // Ejemplo:
            // var comboBox = sender as ComboBox;
            // comboBox.ItemsSource = gameController.Estado.AvailablePlaces;
        }

        private void CmbMover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando se seleccione un nuevo lugar en el ComboBox "CmbMover"
            // Por ejemplo, actualizar el destino para el movimiento
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                string destinoSeleccionado = comboBox.SelectedItem.ToString();
                // Puedes guardar el destino seleccionado en una variable de instancia si lo necesitas
                // Ejemplo: this.destinoActual = destinoSeleccionado;
            }
        }

        private void CmbTomar_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes cargar dinámicamente los objetos disponibles para tomar
            // Ejemplo:
            // var comboBox = sender as ComboBox;
            // comboBox.ItemsSource = gameController.Estado.Inventory;
        }

        private void CmbUsar_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes cargar dinámicamente los objetos disponibles para usar
            // Ejemplo:
            // var comboBox = sender as ComboBox;
            // comboBox.ItemsSource = gameController.Estado.Inventory;
        }

        private void CmbTomar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando se seleccione un nuevo objeto en el ComboBox "CmbTomar"
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                string objetoSeleccionado = comboBox.SelectedItem.ToString();
                // Puedes guardar el objeto seleccionado en una variable de instancia si lo necesitas
                // Ejemplo: this.objetoActual = objetoSeleccionado;
            }
        }

        private void CmbUsar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aquí puedes agregar la lógica que deseas ejecutar cuando se seleccione un nuevo objeto en el ComboBox "CmbUsar"
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                string objetoSeleccionado = comboBox.SelectedItem.ToString();
                // Puedes guardar el objeto seleccionado en una variable de instancia si lo necesitas
                // Ejemplo: this.objetoActual = objetoSeleccionado;
            }
        }

        private void CmbPuedoIr_DropDownOpened(object sender, EventArgs e)
        {
            // Aquí puedes agregar la lógica para actualizar el ComboBox si es necesario
        }

        private void CmbPuedoIr_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Lógica para manejar el cambio de selección en el ComboBox "CmbPuedoIr"
            // Por ejemplo, puedes dejarlo vacío si aún no tienes lógica definida.
        }
    }
}
