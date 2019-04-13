using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CerealPlayer.Annotations;
using CerealPlayer.Commands.Playlist.Loaded;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Playlist
{
    public class LoadedPlaylistTaskViewModel : INotifyPropertyChanged, IPlaylistTaskViewModel
    {
        private readonly Models.Models models;
        private readonly PlaylistModel parent;

        private readonly DispatcherTimer refreshDownloadTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        public LoadedPlaylistTaskViewModel(Models.Models models, PlaylistModel parent)
        {
            this.models = models;
            this.parent = parent;
            parent.PropertyChanged += ParentOnPropertyChanged;
            parent.DownloadPlaylistTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            parent.NextEpisodeTask.PropertyChanged += PlaylistTaskOnPropertyChanged;
            models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;

            PlayCommand = new SetActivePlaylistCommand(models, parent);
            StopCommand = new StopPlaylistUpdateCommand(models, parent);
            RetryCommand = new RetryPlaylistUpdateCommand(models, parent);
            DeleteCommand = new DeletePlaylistCommand(models, parent);

            refreshDownloadTimer.Tick += RefreshDownloadTimerOnTick;
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistModel.NumEpisodesLeft):
                    OnPropertyChanged(nameof(Status));
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => parent.Name;

        public string Status
        {
            get
            {
                string epLeft = Utility.StringUtil.PluralS(parent.NumEpisodesLeft, "episode") + " left";

                // display download task
                if (parent.DownloadPlaylistTask.Status != TaskModel.TaskStatus.Finished)
                {
                    if (parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.ReadyToStart)
                        return epLeft + " (pausing)";

                    if (parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Running)
                        return parent.DownloadPlaylistTask.Description +
                               parent.DownloadPlaylistTask.GetProgressTimeRemainingString(" ");

                    return parent.DownloadPlaylistTask.Description;
                }

                // display next episode task
                if (parent.NextEpisodeTask.Status == TaskModel.TaskStatus.Finished
                    || parent.NextEpisodeTask.Status == TaskModel.TaskStatus.Failed
                    && parent.NextEpisodeTask.Description.Length == 0)
                    return epLeft;

                return parent.NextEpisodeTask.Description;
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

        public Visibility RetryVisibility => !DoesAnyTask() ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PlayVisibility => ReferenceEquals(models.Playlists.ActivePlaylist, parent)
            ? Visibility.Collapsed
            : Visibility.Visible;

        public ICommand DeleteCommand { get; }

        public ICommand RetryCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand PlayCommand { get; }

        private void RefreshDownloadTimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(Status));
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
                    if (parent.DownloadPlaylistTask.Status == TaskModel.TaskStatus.Running)
                    {
                        if (!refreshDownloadTimer.IsEnabled)
                            refreshDownloadTimer.Start();
                    }
                    else
                    {
                        if (refreshDownloadTimer.IsEnabled)
                            refreshDownloadTimer.Stop();
                    }

                    break;
            }
        }

        private bool DoesAnyTask()
        {
            return parent.DownloadPlaylistTask.ReadyOrRunning || parent.NextEpisodeTask.ReadyOrRunning;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}