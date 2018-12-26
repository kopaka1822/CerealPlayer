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
        private readonly Models.Models models;
        private readonly ViewModels.ViewModels viewModels;

        public MainWindow()
        {
            InitializeComponent();

            models = new Models.Models(this);
            viewModels = new ViewModels.ViewModels(models);
            DataContext = viewModels;

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
            //Player.Source = new Uri(Directory.GetCurrentDirectory() + "/ruby2.mp4");
            
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            viewModels.Dispose();
            models.Dispose();
        }
    }
}
