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
    // Nombre: MainWindow
    // Entrada: (instanciación WPF)
    // Salida: Ventana principal inicializada
    // Descripcion: Ventana que coordina la interacción de la UI con el GameController y muestra estado del juego.
    public partial class MainWindow : Window
    {
        // Nombre: gameController
        // Entrada: (se crea en el constructor)
        // Salida: Instancia lista para realizar llamadas HTTP al servidor Prolog
        // Descripcion: Puente entre la interfaz y la lógica remota del juego.
        private readonly GameController gameController;

        // Nombre: MainWindow (constructor)
        // Entrada: (ninguna)
        // Salida: Inicializa componentes y suscripciones
        // Descripcion: Configura la UI, crea GameController y dispara carga inicial de estado.
        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController();
            gameController.OnGameStateUpdated += ActualizarUI_OnGameStateUpdated;
            CargarEstadoInicialAsync();
        }

        // Nombre: CargarEstadoInicialAsync
        // Entrada: (ninguna)
        // Salida: Task (asincrónico, sin valor)
        // Descripcion: Obtiene estado inicial desde el servidor y fuerza actualización de la UI.
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

        // Nombre: ActualizarUI_OnGameStateUpdated
        // Entrada: estado (GameState)
        // Salida: (void) actualiza controles visuales
        // Descripcion: Refresca la UI con datos del estado (ubicación, inventario, combos).
        private void ActualizarUI_OnGameStateUpdated(GameState estado)
        {
            EstadoTxt.Text = $"Lugar: {estado.ubicacion ?? "Desconocido"}";

            // Inventario
            LstInventario.ItemsSource = null;
            LstInventario.ItemsSource = estado.inventario ?? new List<string>();

            // Combos dinámicos
            CmbMover.ItemsSource = estado.caminosPosibles ?? new List<string>();
            CmbUsar.ItemsSource = estado.inventario ?? new List<string>();
            CmbTomar.ItemsSource = estado.objetosEnLugar ?? new List<string>();
        }

        // Nombre: MostrarMensaje
        // Entrada: mensaje (string)
        // Salida: (void) muestra diálogo si procede
        // Descripcion: Helper para mostrar mensajes filtrando vacíos o "(Sin mensaje)".
        private void MostrarMensaje(string mensaje)
        {
            if (!string.IsNullOrWhiteSpace(mensaje) && !mensaje.Equals("(Sin mensaje)"))
                MessageBox.Show(mensaje);
        }

        // Nombre: BtnRefrescar_Click
        // Entrada: sender, e (evento WPF)
        // Salida: (void) asincrónico
        // Descripcion: Solicita actualización del estado al servidor.
        private async void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            await gameController.ActualizarEstadoAsync();
        }

        // Nombre: BtnInventario_Click
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Muestra el inventario actual en un cuadro de mensaje.
        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            var inv = gameController.Estado.inventario;
            string mensaje = inv != null && inv.Count > 0
                ? $"Inventario: {string.Join(", ", inv)}"
                : "Inventario vacío.";
            MostrarMensaje(mensaje);
        }

        // Nombre: BtnMover_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Envía la acción de mover al destino seleccionado y refresca estado.
        private async void BtnMover_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMover.SelectedItem is string destino)
            {
                string resultado = await gameController.MoverAsync(destino);
                MostrarMensaje(resultado);
                await gameController.ActualizarEstadoAsync();
            }
            else
            {
                MostrarMensaje("Selecciona un destino.");
            }
        }


        // Nombre: BtnTomar_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Intenta tomar el objeto seleccionado en el ComboBox.
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

        // Nombre: BtnUsar_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Usa el objeto seleccionado del inventario y actualiza estado.
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

        // Nombre: BtnLugaresVisitados_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Consulta la lista de lugares visitados y la muestra.
        private async void BtnLugaresVisitados_Click(object sender, RoutedEventArgs e)
        {
            var lugares = await gameController.ObtenerLugaresVisitadosAsync();

            if (lugares == null || lugares.Count == 0)
                MostrarMensaje("Aún no has visitado ningún lugar.");
            else
                MostrarMensaje($"Lugares visitados:\n{string.Join(", ", lugares)}");
        }

        // Nombre: BtnObjetosLugar_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Muestra los objetos disponibles en la ubicación actual.
        private async void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
            var objetos = await gameController.ObtenerObjetosEnLugarAsync();

            if (objetos == null || objetos.Count == 0)
                MostrarMensaje("No hay objetos visibles en este lugar.");
            else
                MostrarMensaje($"Objetos en {gameController.Estado.ubicacion}:\n{string.Join(", ", objetos)}");
        }

        // Nombre: BtnReiniciar_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Reinicia el juego y sincroniza nuevo estado.
        private async void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            string resultado = await gameController.ReiniciarJuegoAsync();
            MostrarMensaje(resultado);
            await gameController.ActualizarEstadoAsync();
        }

        // Nombre: CmbMover_DropDownOpened
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Actualiza las opciones de destinos al abrir el ComboBox de mover.
        private async void CmbMover_DropDownOpened(object sender, EventArgs e)
        {
            var caminos = await gameController.ObtenerCaminosAsync();
            CmbMover.ItemsSource = caminos ?? new List<string>();
        }

        // Nombre: CmbMover_SelectionChanged
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Actualiza texto de estado indicando el destino seleccionado.
        private void CmbMover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMover.SelectedItem is string destino)
                EstadoTxt.Text = $"Vas hacia: {destino}";
        }

        // Nombre: Window_Closed
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Punto para liberar recursos al cerrar la ventana (actualmente vacío).
        private void Window_Closed(object sender, EventArgs e)
        {
            // Guardar estado, liberar recursos, etc.
        }

        // Nombre: CmbTomar_DropDownOpened
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Actualiza objetos disponibles para tomar.
        private async void CmbTomar_DropDownOpened(object sender, EventArgs e)
        {
            var objetos = await gameController.ObtenerObjetosEnLugarAsync();
            CmbTomar.ItemsSource = objetos ?? new List<string>();
        }

        // Nombre: CmbTomar_SelectionChanged
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Gancho para lógica adicional al elegir objeto a tomar (vacío actual).
        private void CmbTomar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // var seleccionado = CmbTomar.SelectedItem;
        }

        // Nombre: CmbUsar_DropDownOpened
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Rellena ComboBox con inventario actual al abrir.
        private void CmbUsar_DropDownOpened(object sender, EventArgs e)
        {
            var inventario = gameController.Estado.inventario;
            CmbUsar.ItemsSource = inventario ?? new List<string>();
        }

        // Nombre: CmbUsar_SelectionChanged
        // Entrada: sender, e
        // Salida: (void)
        // Descripcion: Gancho para lógica adicional al cambiar selección de uso (vacío actual).
        private void CmbUsar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // logica
        }

        // Nombre: BtnVerificarGane_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Consulta condición de victoria y muestra resultado.
        private async void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            // Llama a Prolog para verificar condiciones de victoria
            string mensaje = await gameController.VerificarGaneAsync();
            MostrarMensaje(mensaje);
            await gameController.ActualizarEstadoAsync();
        }

        // Nombre: BtnDondeEstoy_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Solicita un mensaje textual de ubicación al servidor.
        private async void BtnDondeEstoy_Click(object sender, RoutedEventArgs e)
        {
            string mensaje = await gameController.DondeEstoyAsync();
            MostrarMensaje(mensaje);
        }

        // Nombre: BtnQueTengo_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Muestra inventario actual consultándolo directamente del servidor.
        private async  void BtnQueTengo_Click(object sender, RoutedEventArgs e)
        {
            var inventario = await gameController.ObtenerInventarioAsync();
            string mensaje = (inventario != null && inventario.Count > 0)
                ? $"Inventario actual:\n{string.Join(", ", inventario)}"
                : "Inventario vacío.";
            MostrarMensaje(mensaje);
        }

        // Nombre: CmbDondeEsta_DropDownOpened
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Rellena ComboBox con todos los objetos (inventario + lugar).
        private async void CmbDondeEsta_DropDownOpened(object sender, EventArgs e)
        {
            var objetos = await gameController.ObtenerTodosLosObjetosAsync();
            CmbDondeEsta.ItemsSource = objetos ?? new List<string>();
        }

        // Nombre: CmbDondeEsta_SelectionChanged
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Consulta ubicación del objeto seleccionado y la muestra.
        private async void CmbDondeEsta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDondeEsta.SelectedItem is string objeto)
            {
                string mensaje = await gameController.DondeEstaAsync(objeto);
                MostrarMensaje(mensaje);
            }
        }
        // Nombre: CmbPuedoIr_DropDownOpened
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Carga destinos disponibles para verificación (sin mover).
        private async void CmbPuedoIr_DropDownOpened(object sender, EventArgs e)
        {
            var caminos = await gameController.ObtenerCaminosAsync();
            CmbPuedoIr.ItemsSource = caminos ?? new List<string>();
        }

        // Nombre: CmbPuedoIr_SelectionChanged
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Verifica si el movimiento al destino es posible y muestra mensaje.
        private async void CmbPuedoIr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbPuedoIr.SelectedItem is string destino)
            {
                string mensaje = await gameController.PuedoIrAsync(destino);
                MostrarMensaje(mensaje);
            }
        }

        // Nombre: BtnComoGano_Click
        // Entrada: sender, e
        // Salida: (void) asincrónico
        // Descripcion: Obtiene instrucciones/rutas sugeridas para ganar el juego.
        private async void BtnComoGano_Click(object sender, RoutedEventArgs e)
        {
            var instrucciones = await gameController.ComoGanoAsync();
            string mensaje = (instrucciones != null && instrucciones.Count > 0)
                ? $"Cómo ganar:\n{string.Join("\n", instrucciones)}"
                : "No hay instrucciones disponibles.";
            MostrarMensaje(mensaje);
        }
    }
}
