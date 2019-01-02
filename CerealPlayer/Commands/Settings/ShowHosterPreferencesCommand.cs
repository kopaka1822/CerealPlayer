using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Hoster;
using CerealPlayer.ViewModels.Settings;
using CerealPlayer.Views;

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
            var view = new HosterPreferencesView();
            var viewModel = new HosterPreferencesViewModel(models);
            view.DataContext = viewModel;

            if(models.App.ShowDialog(view) != true) return;

            // apply settings
            var list = viewModel.Items.Select(item => item.Cargo).ToList();
            models.Web.VideoHoster.ReorderFileHoster(list);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
