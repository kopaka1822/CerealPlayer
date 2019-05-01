using System;
using System.Linq;
using System.Windows.Input;
using CerealPlayer.ViewModels.Settings;
using CerealPlayer.Views;
using CerealPlayer.Views.Settings;

namespace CerealPlayer.Commands.Settings
{
    public class ShowHosterPreferencesCommand : ICommand
    {
        private readonly Models.Models models;

        public ShowHosterPreferencesCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new GlobalHosterPreferencesView();
            var viewModel = new GlobalHosterPreferencesViewModel(models);
            view.DataContext = viewModel;

            if (models.App.ShowDialog(view) != true) return;

            // apply settings
            models.Settings.PreferredHoster = viewModel.HosterList.Items.ToArray();
            models.Settings.Save();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}