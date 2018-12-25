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
    public class PlaylistItemViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private readonly VideoModel video;

        public PlaylistItemViewModel(Models.Models models, VideoModel video, RetryVideoDownloadCommand retryCommand)
        {
            this.models = models;
            this.video = video;
            video.DownloadTask.PropertyChanged += DownloadTaskOnPropertyChanged;

            RetryCommand = retryCommand;
            StopCommand = new StopVideoDownloadCommand(video.DownloadTask);
        }

        private void DownloadTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Status):
                    OnPropertyChanged(nameof(RetryVisibility));
                    OnPropertyChanged(nameof(StopVisibility));
                    OnPropertyChanged(nameof(ProgressVisibility));
                    break;
                case nameof(TaskModel.Description):
                    OnPropertyChanged(nameof(Status));
                    break;
                case nameof(TaskModel.Percentage):
                    OnPropertyChanged(nameof(Progress));
                    break;
            }
        }

        public string Name => video.Name;

        public ICommand RetryCommand { get; }

        public ICommand StopCommand { get; }

        public Visibility RetryVisibility =>
            video.DownloadTask.Status == TaskModel.TaskStatus.Failed
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility StopVisibility =>
            video.DownloadTask.Status == TaskModel.TaskStatus.Running
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility ProgressVisibility => StopVisibility;

        public int Progress
        {
            get => video.DownloadTask.Percentage;
            set { } // dummy set
        }

        // TODO add current play position if playing
        public string Status => video.DownloadTask.Description;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
