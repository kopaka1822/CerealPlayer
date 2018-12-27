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
    public class LoadedPlaylistTaskViewModel : INotifyPropertyChanged, IPlaylistTaskViewModel
    {
        private readonly Models.Models models;
        private readonly PlaylistModel parent;

        public LoadedPlaylistTaskViewModel(Models.Models models, PlaylistModel parent)
        {
            this.models = models;
            this.parent = parent;
            parent.DownloadPlaylistTask.PropertyChanged += DownloadPlaylistTaskOnPropertyChanged;
            PlayCommand = new SetActivePlaylistCommand(models, parent);
        }

        private void DownloadPlaylistTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Description):
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(TaskModel.Progress):
                    OnPropertyChanged(nameof(Progress));
                    break;
                case nameof(TaskModel.Status):
                    OnPropertyChanged(nameof(ProgressVisibility));
                    OnPropertyChanged(nameof(RetryCommand));
                    OnPropertyChanged(nameof(StopVisibility));
                    break;
            }
        }

        public string Name => parent.Name;

        public string Status => parent.DownloadPlaylistTask.Description;

        public int Progress
        {
            get => parent.DownloadPlaylistTask.Progress;
            set { }
        }

        public Visibility ProgressVisibility => parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Running
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility StopVisibility => ProgressVisibility;

        public Visibility PlayVisibility => ReferenceEquals(models.Playlists.ActivePlaylist, parent) 
            ? Visibility.Collapsed 
            : Visibility.Visible;

        public Visibility RetryVisibility => parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Failed
            ? Visibility.Visible
            : Visibility.Collapsed;

        public ICommand DeleteCommand { get; }

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
