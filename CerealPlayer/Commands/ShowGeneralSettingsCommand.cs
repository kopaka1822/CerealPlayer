﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.ViewModels;
using CerealPlayer.Views;

namespace CerealPlayer.Commands
{
    public class ShowGeneralSettingsCommand : ICommand
    {
        private readonly Models.Models models;

        public ShowGeneralSettingsCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var view = new GeneralSettingsView();
            var vm = new GeneralSettingsViewModel(models);
            view.DataContext = vm;

            if(models.App.ShowDialog(view) != true) return;

            // save settings
            var s = models.Settings;
            s.MaxDownloads = vm.MaxDownloads;
            s.MaxAdvanceDownloads = vm.MaxAdvanceDownloads;
            s.DownloadSpeed = vm.DownloadSpeed;
            s.HidePlaybarTime = vm.HidePlaybarTime;
            s.DeleteAfterWatching = vm.DeleteAfterWatching;
            s.MaxChromiumInstances = vm.MaxChromium;

            s.Save();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}