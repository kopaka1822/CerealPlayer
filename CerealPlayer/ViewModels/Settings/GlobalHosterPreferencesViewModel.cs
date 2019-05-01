using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands;
using CerealPlayer.Models.Settings;
using CerealPlayer.ViewModels.General;
using GongSolutions.Wpf.DragDrop;

namespace CerealPlayer.ViewModels.Settings
{
    public class GlobalHosterPreferencesViewModel : INotifyPropertyChanged, IDropTarget
    {
        private readonly Models.Models models;

        public GlobalHosterPreferencesViewModel(Models.Models models)
        {
            this.models = models;
            SaveCancel = new SaveCancelViewModel(models);

            // init items
            foreach (var videoHoster in models.Settings.PreferredHoster)
            {
                // add copy
                Items.Add(new HosterSettingsModel
                {
                    UseHoster = videoHoster.UseHoster,
                    Name = videoHoster.Name
                });
            }
        }

        public ObservableCollection<HosterSettingsModel> Items { get; } = new ObservableCollection<HosterSettingsModel>();

        public string SelectedItem { get; set; } = null;

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public void DragOver(IDropInfo dropInfo)
        {
            // enable if both items are HosterView
            if (dropInfo.Data is HosterSettingsModel && dropInfo.TargetItem is HosterSettingsModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var item = (HosterSettingsModel)dropInfo.Data;
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

        public SaveCancelViewModel SaveCancel { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}