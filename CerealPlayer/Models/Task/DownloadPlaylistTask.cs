using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            playlist.Videos.CollectionChanged += VideosOnCollectionChanged;
        }

        private void VideosOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // active video task was deleted?
            if (activeVideo != null && args.OldItems != null && args.OldItems.Contains(activeVideo))
            {
                UnregisterActiveTask();
                StartNextTask();
                return;
            }

            // is currently a task running/waiting to be executed?
            if (Status == TaskStatus.Finished && args.NewItems != null)
            {
                Debug.Assert(activeVideo == null);
                StartNextTask();
            }
        }

        private void DownloadTaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Status):
                    if (activeVideo.DownloadTask.Status == TaskStatus.Failed)
                    {
                        // use error message from the property changed event
                        SetError(null);
                    }
                    if (activeVideo.DownloadTask.Status == TaskStatus.Finished)
                    {
                        UnregisterActiveTask();
                        // set next subtask (if any) but dont execute yet (this will be handled by the download task controller)
                        SetFinished();
                        StartNextTask();
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
            activeVideo.DownloadTask.PropertyChanged -= DownloadTaskOnPropertyChanged;
            activeVideo = null;
        }

        private void StartNextTask()
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
