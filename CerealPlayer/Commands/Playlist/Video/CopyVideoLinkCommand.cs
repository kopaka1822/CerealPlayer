using System;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Video
{
    public class CopyVideoLinkCommand : ICommand
    {
        private readonly VideoModel video;

        public CopyVideoLinkCommand(VideoModel video)
        {
            this.video = video;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Clipboard.SetText(video.InitialWebsite);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}