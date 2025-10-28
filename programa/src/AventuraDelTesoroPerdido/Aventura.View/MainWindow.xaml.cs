using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Aventura.Controller;

namespace Aventura.View
{
    public partial class MainWindow : Window
    {
        private GameController controller;

        public MainWindow()
        {
            InitializeComponent();
            controller = new GameController();
            lblInfo.Content = controller.GetPlayerInfo();
        }

        private void BtnAddPoints_Click(object sender, RoutedEventArgs e)
        {
            controller.AddPoints(10);
            lblInfo.Content = controller.GetPlayerInfo();
        }
    }
}
