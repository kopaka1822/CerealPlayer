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
using CerealPlayer.Commands.Playlist.Loaded;
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
            parent.DownloadPlaylistTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            parent.NextEpisodeTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;

            PlayCommand = new SetActivePlaylistCommand(models, parent);
            StopCommand = new StopPlaylistUpdateCommand(models, parent);
            RetryCommand = new RetryPlaylistUpdateCommand(models, parent);
            DeleteCommand = new DeletePlaylistCommand(models, parent);
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    OnPropertyChanged(nameof(PlayVisibility));
                    break;
            }
        }

        private void PlaylistTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
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
                    OnPropertyChanged(nameof(RetryVisibility));
                    OnPropertyChanged(nameof(StopVisibility));
                    OnPropertyChanged(nameof(Status));
                    break;
            }
        }

        public string Name => parent.Name;

        public string Status
        {
            get
            {
                // display download task
                if (parent.DownloadPlaylistTask.Status != TaskModel.TaskStatus.Finished)
                {
                    return parent.DownloadPlaylistTask.Description;
                }
                // display next episode task
                if (parent.NextEpisodeTask.Status != TaskModel.TaskStatus.Finished)
                {
                    return parent.NextEpisodeTask.Description;
                }

                return "";
            }
        } 

        public int Progress
        {
            get => parent.DownloadPlaylistTask.Progress;
            set { }
        }

        public Visibility ProgressVisibility => parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Running
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility StopVisibility => DoesAnyTask() ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RetryVisibility => (!DoesAnyTask()) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PlayVisibility => ReferenceEquals(models.Playlists.ActivePlaylist, parent) 
            ? Visibility.Collapsed 
            : Visibility.Visible;

        public ICommand DeleteCommand { get; }

        public ICommand RetryCommand { get; }
        
        public ICommand StopCommand { get; }

        public ICommand PlayCommand { get; }

        private bool DoesAnyTask()
        {
            return parent.DownloadPlaylistTask.ReadyOrRunning || parent.NextEpisodeTask.ReadyOrRunning;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
