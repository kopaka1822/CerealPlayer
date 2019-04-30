using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using GongSolutions.Wpf.DragDrop;

namespace CerealPlayer.ViewModels.Settings
{
    public class HosterPreferencesViewModel : INotifyPropertyChanged, IDropTarget
    {
        private readonly Models.Models models;

        public class HosterInfo
        {
            public string Name { get; set; }
            public bool UseHoster { get; set; }
        }

        public HosterPreferencesViewModel(Models.Models models)
        {
            this.models = models;

            CancelCommand = new SetDialogResultCommand(models, false);
            SaveCommand = new SetDialogResultCommand(models, true);

            // init items
            foreach (var videoHoster in models.Settings.PreferredHoster)
            {
                Items.Add(new HosterInfo
                {
                    Name = videoHoster,
                    UseHoster = true
                });
            }
        }

        public ObservableCollection<HosterInfo> Items { get; } = new ObservableCollection<HosterInfo>();

        public string SelectedItem { get; set; } = null;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public void DragOver(IDropInfo dropInfo)
        {
            // enable if both items are HosterView
            if (dropInfo.Data is HosterInfo && dropInfo.TargetItem is HosterInfo)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var item = (HosterInfo)dropInfo.Data;
            var insertIndex = dropInfo.InsertIndex;
            var oldIndex = Items.IndexOf(item);
            Items.RemoveAt(oldIndex);
            if (oldIndex >= insertIndex)
            {
                Items.Insert(insertIndex, item);
            }
            else // if(oldIndex < insertIndex)
            {
                Items.Insert(insertIndex - 1, item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}