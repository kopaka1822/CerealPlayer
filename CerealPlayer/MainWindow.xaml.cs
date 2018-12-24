using System;
using System.Collections.Generic;
using System.IO;
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

namespace CerealPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var models = new Models.Models(this);
            DataContext = new ViewModels.ViewModels(models);

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
            //Player.Source = new Uri(Directory.GetCurrentDirectory() + "/ruby2.mp4");
            
        }

        private void Player_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (WindowState != WindowState.Maximized)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
            }
        }
    }
}
