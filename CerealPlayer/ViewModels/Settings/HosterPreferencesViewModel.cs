using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Models.Hoster;

namespace CerealPlayer.ViewModels.Settings
{
    public class HosterPreferencesViewModel : INotifyPropertyChanged
    {
        public class HosterView
        {
            public IVideoHoster Cargo { get; set; }

            public override string ToString()
            {
                return Cargo.Name;
            }
        }

        private readonly Models.Models models;

        public HosterPreferencesViewModel(Models.Models models)
        {
            this.models = models;

            CancelCommand = new SetDialogResultCommand(models, false);
            SaveCommand = new SetDialogResultCommand(models, true);

            // init items
            foreach (var videoHoster in models.Web.VideoHoster.GetFileHoster())
            {
                Items.Add(new HosterView
                {
                    Cargo = videoHoster
                });
            }
        }

        public ObservableCollection<HosterView> Items { get; } = new ObservableCollection<HosterView>();

        public HosterView SelectedItem { get; set; } = null;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
