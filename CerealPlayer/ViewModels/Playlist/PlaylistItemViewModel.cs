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
using CerealPlayer.Commands.Player;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.Video;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.ViewModels.Playlist
{
    public class PlaylistItemViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private readonly VideoModel video;

        public PlaylistItemViewModel(Models.Models models, VideoModel video)
        {
            this.models = models;
            this.video = video;
            video.DownloadTask.PropertyChanged += TaskOnPropertyChanged;
            video.DeleteTask.PropertyChanged += TaskOnPropertyChanged;

            RetryCommand = new RetryVideoDownloadCommand(video.Parent);
            StopCommand = new StopVideoDownloadCommand(video.DownloadTask);
            PlayCommand = new SetActiveVideoCommand(models, video);
            DeleteCommand = new DeleteVideoCommand(models, video, true);
            StopDeleteCommand = new StopVideoDeletionCommand(models, video);
        }

        private void TaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Status):
                    OnPropertyChanged(nameof(RetryVisibility));
                    OnPropertyChanged(nameof(StopVisibility));
                    OnPropertyChanged(nameof(ProgressVisibility));
                    OnPropertyChanged(nameof(StopDeleteVisibility));
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(TaskModel.Description):
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(TaskModel.Progress):
                    OnPropertyChanged(nameof(Progress));
                    break;
            }
        }

        public string Name => video.Name;

        public ICommand PlayCommand { get; }

        public ICommand RetryCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand StopDeleteCommand { get; }

        public Visibility RetryVisibility =>
            video.DownloadTask.Status == TaskModel.TaskStatus.Failed
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility StopVisibility =>
            video.DownloadTask.Status == TaskModel.TaskStatus.Running
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility ProgressVisibility => StopVisibility;

        public Visibility StopDeleteVisibility => video.DeleteTask.Status == TaskModel.TaskStatus.Running
            ? Visibility.Visible
            : Visibility.Collapsed;

        public int Progress
        {
            get => video.DownloadTask.Progress;
            set { } // dummy set
        }

        public string Status
        {
            get
            {
                if (video.DeleteTask.ReadyOrRunning)
                    return video.DeleteTask.Description;

                if (video.DownloadTask.Status == TaskModel.TaskStatus.Finished)
                {
                    return "downloaded";
                }
                return video.DownloadTask.Description;
            }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
