using System;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Video
{
    public class SetActiveVideoCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly VideoModel video;

        public SetActiveVideoCommand(Models.Models models, VideoModel video)
        {
            this.models = models;
            this.video = video;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            video.Parent.PlayingVideo = video;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}