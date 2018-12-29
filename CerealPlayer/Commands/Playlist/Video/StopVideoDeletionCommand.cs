using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Commands.Playlist.Video
{
    public class StopVideoDeletionCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly VideoModel video;

        public StopVideoDeletionCommand(Models.Models models, VideoModel video)
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
            if(video.DeleteTask.ReadyOrRunning)
                video.DeleteTask.Stop();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
