using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class TaskController
    {
        private readonly Models.Models models;

        // tasks that are currently executed
        private readonly List<PlaylistModel> activeTasks = new List<PlaylistModel>();
        // tasks in the prority queue
        private readonly List<PlaylistModel> queuedTasks = new List<PlaylistModel>();

        private readonly int maxDownloadTasks = 4;

        public TaskController(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
        }

        /// <summary>
        /// retries a task that has previously failed
        /// </summary>
        /// <param name="task"></param>
        public void RetryTask(PlaylistModel task)
        {
            // clear fail flags or search for new episode
            if (task.NextEpisodeTask?.Status == TaskModel.TaskStatus.Failed || task.NextEpisodeTask?.Status == TaskModel.TaskStatus.Finished)
            {
                task.NextEpisodeTask.ResetStatus();
            }

            foreach (var videoModel in task.Videos)
            {
                if(videoModel.DownloadTask.Status == TaskModel.TaskStatus.Failed)
                    videoModel.DownloadTask.ResetStatus();
            }

            bool inActive = activeTasks.Contains(task);
            bool inQueue = queuedTasks.Contains(task);

            // task was removed and should be continued
            if (!inActive && !inQueue)
            {
                // queue or active?
                if (activeTasks.Count < maxDownloadTasks)
                {
                    activeTasks.Add(task);
                    inActive = true;
                }
                else
                {
                    queuedTasks.Add(task);
                }
            }

            // restart next episode task if it failed
            if (task.NextEpisodeTask?.Status == TaskModel.TaskStatus.NotStarted)
                task.NextEpisodeTask.Start();

            if (inActive)
                StartDownloadTask(task); // resume download

        }

        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // remove these tasks
            if(args.OldItems != null)
                foreach (var item in args.OldItems)
                {
                    ForceRemoveTask(item as PlaylistModel);
                }

            // add those tasks to the queue
            if(args.NewItems != null)
                foreach (var item in args.NewItems)
                {
                    AddTask(item as PlaylistModel);
                }
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(PlaylistsModel.ActivePlaylist):
                    // move the item in the priority queue
                    if (models.Playlists.ActivePlaylist == null) return;
                    var idx = queuedTasks.IndexOf(models.Playlists.ActivePlaylist);
                    if (idx <= 0) return; // item is already active or queued as the next

                    // put it in first place
                    queuedTasks.RemoveAt(idx);
                    queuedTasks.Insert(0, models.Playlists.ActivePlaylist);
                    break;
            }
        }

        private void AddTask(PlaylistModel task)
        {

            // always start next episode task
            task.Videos.CollectionChanged += (sender, args) =>
            {
                // start next download task if active task
                if(!activeTasks.Contains(task)) return;
                // start the next download task if it was not already running
                StartDownloadTask(task);
            };

            task.NextEpisodeTask?.Start();

            if (activeTasks.Count < maxDownloadTasks)
            {
                activeTasks.Add(task);
                StartDownloadTask(task);
            }
            else
            {
                queuedTasks.Add(task);
            }
        }

        private void StartDownloadTask(PlaylistModel task)
        {
            // find first video with not finished task and start it
            foreach (var video in task.Videos)
            {
                switch (video.DownloadTask.Status)
                {
                    case TaskModel.TaskStatus.NotStarted:
                    {
                        var v = video;
                        PropertyChangedEventHandler statusHandler = null;
                        statusHandler = (e, args) =>
                        {
                            Debug.Assert(statusHandler != null);
                            if (v.DownloadTask.Status == TaskModel.TaskStatus.Finished || v.DownloadTask.Status == TaskModel.TaskStatus.Failed)
                            {
                                video.DownloadTask.PropertyChanged -= statusHandler;

                                // start next download
                                StartDownloadTask(task);
                            }
                        };
                        video.DownloadTask.PropertyChanged += statusHandler;
                        video.DownloadTask.Start();
                    }
                        return;
                    case TaskModel.TaskStatus.Running:
                        // already running
                        return;
                    case TaskModel.TaskStatus.Finished:
                        // start next video
                        break;
                    case TaskModel.TaskStatus.Failed:
                        // remove from tasks
                        RemoveDownloadTask(task);
                        return;
                }
            }

            // all tasks done
            RemoveDownloadTask(task);
        }

        private void RemoveDownloadTask(PlaylistModel task)
        {
            if (task.NextEpisodeTask == null)
            {
                if (!TryRemoveActiveTask(task))
                    queuedTasks.Remove(task);

                return;
            }

            Debug.Assert(task.NextEpisodeTask.Status != TaskModel.TaskStatus.NotStarted);
            if (task.NextEpisodeTask.Status == TaskModel.TaskStatus.Running)
                return; // this task is still running. don't delete yet

            Debug.Assert(task.NextEpisodeTask.Status == TaskModel.TaskStatus.Failed || task.NextEpisodeTask.Status == TaskModel.TaskStatus.Finished);
            // failed or finished => stop this task for now
            if (!TryRemoveActiveTask(task))
                queuedTasks.Remove(task);
        }

        private void ForceRemoveTask(PlaylistModel task)
        {
            if(task.NextEpisodeTask?.Status == TaskModel.TaskStatus.Running)
                task.NextEpisodeTask.Stop();


            // stop any video tasks
            foreach (var videoTask in task.Videos)
            {
                if (videoTask.DownloadTask.Status == TaskModel.TaskStatus.Running)
                    videoTask.DownloadTask.Stop();
            }

            if (!TryRemoveActiveTask(task))
                queuedTasks.Remove(task);
        }

        /// <summary>
        /// tries to remove the task from active tasks. if the task was not an active task false is returned
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private bool TryRemoveActiveTask(PlaylistModel task)
        {
            if (!activeTasks.Remove(task)) return false;
            
            if(activeTasks.Count >= maxDownloadTasks) return true;

            if(queuedTasks.Count == 0) return true;

            var newTask = queuedTasks[0];
            queuedTasks.RemoveAt(0);

            activeTasks.Add(newTask);
            StartDownloadTask(newTask);
            return true;
        }
    }
}
