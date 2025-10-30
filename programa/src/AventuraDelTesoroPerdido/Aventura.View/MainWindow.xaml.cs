using System.Windows;
using Aventura.Controller;

namespace AventuraGUI
{
    public partial class MainWindow : Window
    {
        private readonly GameController controller;

        public MainWindow()
        {
            InitializeComponent();
            controller = new GameController();

            // Mensaje de bienvenida
            txtResponse.Text = "🗺️ Bienvenido a la Aventura del Tesoro Perdido!\n";
            txtResponse.Text += "Escribe un comando, por ejemplo: mover(templo).\n\n";
            txtResponse.Text += $"📍 Estado inicial: {controller.PlayerInfo}\n\n";
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string cmd = txtCommand.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) return;

            // Ejecutar comando Prolog vía GameController
            string response = controller.EjecutarComando(cmd);

            // Mostrar comando, respuesta y estado actual
            txtResponse.Text += $"> {cmd}\n{response}\n";
            txtResponse.Text += $"📍 Estado actual: {controller.PlayerInfo}\n";
            txtResponse.Text += $"🎒 Inventario: {string.Join(", ", controller.GetInventory())}\n\n";

            // Actualizar scroll hacia abajo automáticamente
            txtResponse.ScrollToEnd();

            txtCommand.Clear();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            controller.Dispose();
            base.OnClosed(e);
        }
    }
}
