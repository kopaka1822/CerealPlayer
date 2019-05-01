using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CerealPlayer.Models.Settings;
using GongSolutions.Wpf.DragDrop;

namespace CerealPlayer.ViewModels.Settings
{
    public class HosterListViewModel : IDropTarget
    {
        public HosterListViewModel(HosterSettingsModel[] settings)
        {
            // init items
            foreach (var videoHoster in settings)
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
    }
}
