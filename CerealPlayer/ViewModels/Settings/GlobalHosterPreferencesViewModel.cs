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
    public class GlobalHosterPreferencesViewModel
    { 
        public GlobalHosterPreferencesViewModel(Models.Models models)
        {
            SaveCancel = new SaveCancelViewModel(models);
            HosterList = new HosterListViewModel(models.Settings.PreferredHoster);
        }

        public SaveCancelViewModel SaveCancel { get; }

        public HosterListViewModel HosterList { get; }
    }
}