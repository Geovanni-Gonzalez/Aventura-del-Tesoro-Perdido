using Aventura.Controller;
using Aventura.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Configuration;

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
            string serverUrl = ConfigurationManager.AppSettings["PrologServerUrl"] ?? "http://localhost:5000";
            gameController = new GameController(serverUrl);
            gameController.OnGameStateUpdated += ActualizarUI_OnGameStateUpdated;
            CargarEstadoInicialAsync();
            IniciarTimerConexion();
        }

        // Nombre: IniciarTimerConexion
        // Entrada: (ninguna)
        // Salida: (void)
        // Descripcion: Configura un timer que revisa el estado del servidor cada 5 segundos.
        private void IniciarTimerConexion()
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += async (s, e) =>
            {
                bool conectado = await gameController.PingAsync();
                ConnectionStatusLed.Fill = conectado ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.Red;
            };
            timer.Start();
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

            // Actualizar Inventario
            LstInventario.ItemsSource = null;
            LstInventario.ItemsSource = estado.inventario ?? new List<string>();

            // Actualizar Combos y Botones Inteligentes
            ActualizarControlesInteligentes(estado);

            // Verificar cambio de ubicación para animar
            if (_ultimaUbicacion != estado.ubicacion)
            {
                AnimarViaje(estado.ubicacion);
                _ultimaUbicacion = estado.ubicacion;
            }
            else
            {
                // Carga inicial o refresh sin movimiento
                ActualizarFondoMapa(estado.ubicacion);
            }
        }

        private string _ultimaUbicacion; // Para detectar cambios

        private void ActualizarControlesInteligentes(GameState estado)
        {
            // MOVER
            var caminos = estado.caminosPosibles ?? new List<string>();
            CmbMover.ItemsSource = caminos;
            CmbMover.IsEnabled = caminos.Any();
            BtnMover.IsEnabled = caminos.Any();
            if (caminos.Any()) CmbMover.SelectedIndex = 0; // Auto-select

            // TOMAR
            var objetosLugar = estado.objetosEnLugar ?? new List<string>();
            CmbTomar.ItemsSource = objetosLugar;
            CmbTomar.IsEnabled = objetosLugar.Any();
            BtnTomar.IsEnabled = objetosLugar.Any();
            if (objetosLugar.Any()) CmbTomar.SelectedIndex = 0;

            // USAR
            var inventario = estado.inventario ?? new List<string>();
            CmbUsar.ItemsSource = inventario;
            CmbUsar.IsEnabled = inventario.Any();
            BtnUsar.IsEnabled = inventario.Any();
            if (inventario.Any()) CmbUsar.SelectedIndex = 0;
        }

        private void AnimarViaje(string nuevaUbicacion)
        {
            // 1. Salida (Slide hacia derecha + Fade Out)
            var animSalidaX = new System.Windows.Media.Animation.DoubleAnimation(82, 400, TimeSpan.FromSeconds(0.4));
            var animSalidaOpacidad = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));

            animSalidaX.Completed += (s, e) =>
            {
                // 2. Cambiar Fondo (al terminar la salida)
                ActualizarFondoMapa(nuevaUbicacion);

                // 3. Preparar Entrada (Resetear posición a izquierda)
                Canvas.SetLeft(PersonajeImg, -100);

                // 4. Entrada (Slide desde izquierda + Fade In)
                var animEntradaX = new System.Windows.Media.Animation.DoubleAnimation(-100, 82, TimeSpan.FromSeconds(0.4));
                var animEntradaOpacidad = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));

                // Configurar easing para efecto más suave
                animEntradaX.EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };

                PersonajeImg.BeginAnimation(Canvas.LeftProperty, animEntradaX);
                PersonajeImg.BeginAnimation(UIElement.OpacityProperty, animEntradaOpacidad);
            };

            PersonajeImg.BeginAnimation(Canvas.LeftProperty, animSalidaX);
            PersonajeImg.BeginAnimation(UIElement.OpacityProperty, animSalidaOpacidad);
        }

        private void ActualizarFondoMapa(string lugar)
        {
            try
            {
                // [User Request] Usar el mismo fondo siempre (bosque.jpg)
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Lugar", "bosque.jpg");

                if (System.IO.File.Exists(imagePath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Si la imagen ya es la misma, tal vez no necesitamos animar el fondo, 
                    // pero mantenemos la lógica para asegurar que se muestre.
                    MapBackgroundBrush.ImageSource = bitmap;
                    
                    // Asegurar opacidad por si acaso
                    MapBackgroundBrush.Opacity = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando imagen: {ex.Message}");
            }
        }

        // Nombre: MostrarMensaje (REFACTORIZADO)
        // Entrada: mensaje (string)
        // Salida: (void)
        // Descripcion: Agrega el mensaje a la bitácora en lugar de mostrar un popup.
        private void MostrarMensaje(string mensaje)
        {
            if (!string.IsNullOrWhiteSpace(mensaje) && !mensaje.Equals("(Sin mensaje)"))
            {
                AgregarLog(mensaje);
            }
        }

        // Nombre: AgregarLog
        // Entrada: mensaje (string)
        // Salida: (void)
        // Descripcion: Agrega una línea a la bitácora visual con timestamp.
        private void AgregarLog(string mensaje)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            if (TxtLog != null)
            {
                TxtLog.Text = $"[{time}] {mensaje}\n\n" + TxtLog.Text;
            }
        }

        // Nombre: EjecutarTareaAsync
        // Entrada: funcionAsync (Func<Task>)
        // Salida: (void)
        // Descripcion: Wrapper para ejecutar tareas asincrónicas manejando estado de ocupado y errores.
        private async void EjecutarTareaAsync(Func<Task> funcionAsync)
        {
            try
            {
                IsEnabled = false; // Bloquear UI
                Mouse.OverrideCursor = Cursors.Wait; // Cursor de espera
                await funcionAsync();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}");
            }
            finally
            {
                IsEnabled = true; // Desbloquear UI
                Mouse.OverrideCursor = null; // Restaurar cursor
            }
        }

        // --------------------------------------------------------------------------------
        // HANDLERS DE BOTONES (Refactorizados para usar EjecutarTareaAsync)
        // --------------------------------------------------------------------------------

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () => await gameController.ActualizarEstadoAsync());
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            var inv = gameController.Estado.inventario;
            string mensaje = inv != null && inv.Count > 0
                ? $"Inventario: {string.Join(", ", inv)}"
                : "Tu inventario está vacío.";
            MostrarMensaje(mensaje);
        }

        private void BtnMover_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                if (CmbMover.SelectedItem is string destino)
                {
                    string resultado = await gameController.MoverAsync(destino);
                    MostrarMensaje(resultado);
                    await gameController.ActualizarEstadoAsync();
                }
                else
                {
                    MostrarMensaje("Selecciona un destino pálido.");
                }
            });
        }


        private void BtnTomar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                if (CmbTomar.SelectedItem is string objeto)
                {
                    string resultado = await gameController.TomarAsync(objeto);
                    MostrarMensaje(resultado);
                    await gameController.ActualizarEstadoAsync(); // Actualizar para remover objeto de lista
                }
                else
                {
                    MostrarMensaje("Selecciona un objeto para tomar.");
                }
            });
        }

        private void BtnUsar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
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
            });
        }

        private void BtnLugaresVisitados_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                var lugares = await gameController.ObtenerLugaresVisitadosAsync();
                if (lugares == null || lugares.Count == 0)
                    MostrarMensaje("Aún no has visitado ningún lugar.");
                else
                    MostrarMensaje($"Lugares visitados: {string.Join(", ", lugares)}");
            });
        }

        private void BtnObjetosLugar_Click(object sender, RoutedEventArgs e)
        {
             // Este botón parece no existir en la UI XAML actual, pero lo mantenemos por compatibilidad si se agrega
            EjecutarTareaAsync(async () =>
            {
                var objetos = await gameController.ObtenerObjetosEnLugarAsync();
                MostrarMensaje(objetos != null && objetos.Any() ? $"Objetos aquí: {string.Join(", ", objetos)}" : "No hay objetos visibles.");
            });
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                if (MessageBox.Show("¿Seguro que quieres reiniciar?", "Reiniciar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    string resultado = await gameController.ReiniciarJuegoAsync();
                    MostrarMensaje(resultado);
                    await gameController.ActualizarEstadoAsync();
                    if(TxtLog != null) TxtLog.Text = ""; // Limpiar log
                    AgregarLog("--- JUEGO REINICIADO ---");
                }
            });
        }

        private async void CmbMover_DropDownOpened(object sender, EventArgs e)
        {
            // No bloqueamos toda la UI para dropdowns, pero manejamos errores
            try
            {
                var caminos = await gameController.ObtenerCaminosAsync();
                CmbMover.ItemsSource = caminos ?? new List<string>();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void CmbMover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMover.SelectedItem is string destino)
                EstadoTxt.Text = $"Vas hacia: {destino}";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup
        }

        private async void CmbTomar_DropDownOpened(object sender, EventArgs e)
        {
             try
            {
                var objetos = await gameController.ObtenerObjetosEnLugarAsync();
                CmbTomar.ItemsSource = objetos ?? new List<string>();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void CmbTomar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

        private void CmbUsar_DropDownOpened(object sender, EventArgs e)
        {
            var inventario = gameController.Estado.inventario;
            CmbUsar.ItemsSource = inventario ?? new List<string>();
        }

        private void CmbUsar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

        private void BtnVerificarGane_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                string mensaje = await gameController.VerificarGaneAsync();
                MostrarMensaje(mensaje);
                await gameController.ActualizarEstadoAsync();
            });
        }

        private void BtnDondeEstoy_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                string mensaje = await gameController.DondeEstoyAsync();
                MostrarMensaje(mensaje);
            });
        }

        private void BtnQueTengo_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                var inventario = await gameController.ObtenerInventarioAsync();
                MostrarMensaje(inventario != null && inventario.Count > 0
                    ? $"Inventario actual: {string.Join(", ", inventario)}"
                    : "Inventario vacío.");
            });
        }

        private async void CmbDondeEsta_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                var objetos = await gameController.ObtenerTodosLosObjetosAsync();
                CmbDondeEsta.ItemsSource = objetos ?? new List<string>();
            }
            catch { }
        }

        private void CmbDondeEsta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDondeEsta.SelectedItem is string objeto)
            {
                EjecutarTareaAsync(async () =>
                {
                    string mensaje = await gameController.DondeEstaAsync(objeto);
                    MostrarMensaje(mensaje);
                });
            }
        }
        
        private async void CmbPuedoIr_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                var caminos = await gameController.ObtenerCaminosAsync();
                CmbPuedoIr.ItemsSource = caminos ?? new List<string>();
            }
            catch { }
        }

        private void CmbPuedoIr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbPuedoIr.SelectedItem is string destino)
            {
                EjecutarTareaAsync(async () =>
                {
                    string mensaje = await gameController.PuedoIrAsync(destino);
                    MostrarMensaje(mensaje);
                });
            }
        }

        private void BtnComoGano_Click(object sender, RoutedEventArgs e)
        {
            EjecutarTareaAsync(async () =>
            {
                var instrucciones = await gameController.ComoGanoAsync();
                MostrarMensaje(instrucciones != null && instrucciones.Count > 0
                    ? $"Cómo ganar:\n{string.Join("\n", instrucciones)}"
                    : "No hay instrucciones disponibles.");
            });
        }
        
        private void BtnAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string infoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.txt");
                if (System.IO.File.Exists(infoPath))
                {
                    string contenido = System.IO.File.ReadAllText(infoPath);
                    // Mantenemos MessageBox para "Acerca de" ya que es informativo modal
                    MessageBox.Show(contenido, "Acerca de - Aventura del Tesoro Perdido");
                }
                else
                {
                    MostrarMensaje("No se encontró el archivo info.txt.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al leer info.txt: {ex.Message}");
            }
        }
    }
}
