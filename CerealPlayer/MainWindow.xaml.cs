using System;
using System.Windows;

namespace CerealPlayer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
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