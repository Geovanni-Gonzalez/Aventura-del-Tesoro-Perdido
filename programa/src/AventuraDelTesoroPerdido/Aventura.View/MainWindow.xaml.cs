using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Aventura.Controller;

namespace Aventura.View
{
    public partial class MainWindow : Window
    {
        private readonly GameController controller;
        private readonly Dictionary<string, Button> placeButtons = new Dictionary<string, Button>();
        private double personajeX = 0;
        private const double buttonSpacing = 160;
        private const double startX = 50;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                controller = new GameController();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar GameController: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Cargar imagen del personaje
            string spritePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Personaje", "explorador.png");
            if (!File.Exists(spritePath))
            {
                PersonajeImg.Source = null;
            }
            else
            {
                PersonajeImg.Source = new BitmapImage(new Uri(spritePath));
            }

            // Suscribirse a actualizaciones del estado
            controller.OnGameStateUpdated += GameController_OnGameStateUpdated;

            // Inicializar interfaz
            RefreshLugares();
            PositionCharacterAt(controller.gameState.CurrentLocation);
        }

        // --- EVENTOS UI ---------------------------------------------------

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            RefreshLugares();
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            var inv = controller.GetInventory();
            string msg = inv.Count == 0 ? "Inventario vacío." : string.Join(", ", inv);
            MessageBox.Show(msg, "Inventario", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
            var objs = controller.GetObjetosEnLugarActual();
            if (objs.Count == 0)
            {
                MessageBox.Show("No hay objetos aquí.", "Objetos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sel = ShowSelectionDialog("Objetos disponibles", objs);
            if (!string.IsNullOrEmpty(sel))
            {
                string resp = controller.EjecutarComando($"tomar({sel})");
                MessageBox.Show(resp, "Tomar objeto", MessageBoxButton.OK, MessageBoxImage.Information);
                controller.NotifyStateChanged(); // 🔹 Notifica actualización de UI
            }
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            string resp = controller.EjecutarComando("reiniciar");
            MessageBox.Show(resp, "Reiniciar juego", MessageBoxButton.OK, MessageBoxImage.Information);
            controller.NotifyStateChanged();
            PositionCharacterAt(controller.gameState.CurrentLocation);
        }

        private void LugarButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            string destino = (string)btn.Tag;

            string resp = controller.EjecutarComando($"mover({destino})");
            MessageBox.Show(resp, "Mover", MessageBoxButton.OK, MessageBoxImage.Information);

            // Solo animar si no hay error
            if (!string.IsNullOrEmpty(resp) && !resp.StartsWith("⚠️") && !resp.StartsWith("❌"))
            {
                AnimateCharacterTo(destino);
            }
        }

        // --- MÉTODOS LÓGICOS Y VISUALES -----------------------------------

        private void GameController_OnGameStateUpdated(Aventura.Model.GameState gs)
        {
            Dispatcher.Invoke(() =>
            {
                EstadoTxt.Text = $"Jugador: {gs.PlayerName} | Lugar: {gs.CurrentLocation} | " +
                                 $"Puntos: {gs.Score} | Inventario: {string.Join(", ", gs.Inventory)}";
            });
        }

        private void RefreshLugares()
        {
            LugarButtonsPanel.Children.Clear();
            placeButtons.Clear();
            var lugares = controller.GetLugares();

            double x = startX;
            foreach (var lugar in lugares)
            {
                var btn = new Button
                {
                    Content = lugar,
                    Width = 120,
                    Height = 48,
                    Margin = new Thickness(8),
                    Tag = lugar
                };
                btn.Click += LugarButton_Click;
                LugarButtonsPanel.Children.Add(btn);
                placeButtons[lugar] = btn;
                x += buttonSpacing;
            }

            PositionCharacterAt(controller.gameState.CurrentLocation);
        }

        private void PositionCharacterAt(string lugar)
        {
            if (string.IsNullOrEmpty(lugar))
            {
                Canvas.SetLeft(PersonajeImg, startX);
                personajeX = startX;
                return;
            }

            if (!placeButtons.ContainsKey(lugar))
            {
                Canvas.SetLeft(PersonajeImg, startX);
                personajeX = startX;
                return;
            }

            int idx = placeButtons.Keys.ToList().IndexOf(lugar);
            double x = startX + idx * buttonSpacing;
            Canvas.SetLeft(PersonajeImg, x);
            personajeX = x;
        }

        private void AnimateCharacterTo(string lugar)
        {
            if (!placeButtons.ContainsKey(lugar)) return;
            int idx = placeButtons.Keys.ToList().IndexOf(lugar);
            double destinoX = startX + idx * buttonSpacing;

            var anim = new DoubleAnimation
            {
                From = personajeX,
                To = destinoX,
                Duration = TimeSpan.FromSeconds(0.9),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            PersonajeImg.BeginAnimation(Canvas.LeftProperty, anim);
            personajeX = destinoX;
        }

        // --- DIÁLOGO DE SELECCIÓN SIMPLE ----------------------------------

        private string ShowSelectionDialog(string title, List<string> items)
        {
            var dlg = new Window
            {
                Title = title,
                Width = 300,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var sp = new StackPanel { Margin = new Thickness(8) };
            var lb = new ListBox { ItemsSource = items, Height = 200 };
            sp.Children.Add(lb);
            var btnOk = new Button
            {
                Content = "OK",
                Margin = new Thickness(0, 8, 0, 0),
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            sp.Children.Add(btnOk);
            dlg.Content = sp;

            string selected = null;
            btnOk.Click += (_, __) =>
            {
                selected = lb.SelectedItem as string;
                dlg.Close();
            };
            dlg.ShowDialog();
            return selected;
        }
    }
}
