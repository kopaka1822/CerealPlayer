using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Video
{
    class OpenVideoLinkCommand : ICommand
    {
        private readonly VideoModel model;

        public OpenVideoLinkCommand(VideoModel model)
        {
            this.model = model;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Process.Start(model.InitialWebsite);
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
