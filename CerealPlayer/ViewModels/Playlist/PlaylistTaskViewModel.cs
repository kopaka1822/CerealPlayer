using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CerealPlayer.Annotations;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistTaskViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private readonly PlaylistModel parent;

        public PlaylistTaskViewModel(Models.Models models, PlaylistModel parent)
        {
            this.models = models;
            this.parent = parent;
            parent.PropertyChanged += ParentOnPropertyChanged;

            PlayCommand = new SetActivePlaylistCommand(models, parent);
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.DownloadDescription):
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(PlaylistModel.DownloadProgress):
                    OnPropertyChanged(nameof(Progress));
                    break;
                case nameof(PlaylistModel.DownloadStatus):
                    OnPropertyChanged(nameof(ProgressVisibility));
                    OnPropertyChanged(nameof(RetryCommand));
                    OnPropertyChanged(nameof(StopVisibility));
                    break;
            }
        }

        public string Name => parent.Name;

        public string Status => parent.DownloadDescription;

        public int Progress
        {
            get => parent.DownloadProgress;
            set { }
        }

        public Visibility ProgressVisibility => parent.DownloadStatus == TaskModel.TaskStatus.Running
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility StopVisibility => ProgressVisibility;

        public Visibility RetryVisibility => parent.DownloadStatus == TaskModel.TaskStatus.Failed
            ? Visibility.Visible
            : Visibility.Collapsed;

        public ICommand RetryCommand { get; }
        
        public ICommand StopCommand { get; }

        public ICommand PlayCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
