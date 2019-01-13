using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task
{
    public class DownloadPlaylistTask : TaskModel
    {
        private readonly PlaylistModel playlist;
        private VideoModel activeVideo = null;

        public DownloadPlaylistTask(PlaylistModel playlist) : base(1, TimeSpan.Zero)
        {
            // initialize this before adding videos!
            Debug.Assert(playlist.Videos.Count == 0);

            this.playlist = playlist;
            playlist.VideosCollectionChanged += VideosOnCollectionChanged;
            PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        ///     called if the property of the underlying task model changed
        /// </summary>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Status):
                    switch (Status)
                    {
                        case TaskStatus.ReadyToStart:
                            // reset own subtask to ready (if failed)
                            if (activeVideo?.DownloadTask.Status == TaskStatus.Failed)
                                activeVideo.DownloadTask.ResetStatus();
                            break;
                        case TaskStatus.Running:
                            Debug.Assert(activeVideo != null);
                            break;
                        case TaskStatus.Finished:
                            // set new task if available
                            SetNextTask();
                            break;
                        case TaskStatus.Failed:
                            // stop own subtask (if running)
                            if (activeVideo?.DownloadTask.Status == TaskStatus.Running)
                                activeVideo.DownloadTask.Stop();
                            break;
                    }

                    break;
            }
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // active video task was deleted?
            if (activeVideo != null && args.OldItems != null && args.OldItems.Contains(activeVideo))
            {
                UnregisterActiveTask();
                SetFinished();
                SetNextTask();
                return;
            }

            // is currently a task running/waiting to be executed?
            if (Status == TaskStatus.Finished && args.NewItems != null)
            {
                Debug.Assert(activeVideo == null);
                SetNextTask();
            }
        }

        private void DownloadTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Status):
                    switch (activeVideo.DownloadTask.Status)
                    {
                        case TaskStatus.Failed:
                            // use error message from the property changed event
                            SetError(null);
                            break;
                        case TaskStatus.Finished:
                            UnregisterActiveTask();
                            // set finished flag to set next subtask (if any) but dont execute yet (this will be handled by the download task controller)
                            SetFinished();
                            // new sub task will be set in OnPropertyChanged
                            break;
                        case TaskStatus.ReadyToStart:
                            // subtask was reset from failed to ready
                            // => switch own status to ready to start
                            if (Status != TaskStatus.ReadyToStart)
                                ResetStatus();
                            break;
                        case TaskStatus.Running:
                            Debug.Assert(Status == TaskStatus.Running);
                            break;
                    }

                    break;
                case nameof(Description):
                    Description = $"{activeVideo.Number} - {activeVideo.DownloadTask.Description}";
                    break;
                case nameof(Progress):
                    Progress = activeVideo.DownloadTask.Progress;
                    break;
            }
        }

        private void RegisterTask(VideoModel video)
        {
            Debug.Assert(activeVideo == null);
            activeVideo = video;
            video.DownloadTask.PropertyChanged += DownloadTaskOnPropertyChanged;
            Progress = video.DownloadTask.Progress;
            SetNewSubTask(video.DownloadTask);
        }

        private void UnregisterActiveTask()
        {
            if (activeVideo.DownloadTask.Status == TaskStatus.Running)
                activeVideo.DownloadTask.Stop();

            activeVideo.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;
            activeVideo = null;
        }

        private void SetNextTask()
        {
            Debug.Assert(activeVideo == null);
            foreach (var video in playlist.Videos)
            {
                if (video.DownloadTask.Status == TaskStatus.ReadyToStart)
                {
                    RegisterTask(video);
                    return;
                }

                Debug.Assert(video.DownloadTask.Status == TaskStatus.Finished);
            }
        }
    }
}