using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public abstract class TaskControllerBase
    {
        protected readonly Models.Models models;
        private readonly List<PlaylistModel> priorityQueue = new List<PlaylistModel>();

        protected TaskControllerBase(Models.Models models)
        {
            this.models = models;
            this.models.Playlists.List.CollectionChanged += PlaylistOnCollectionChanged;
            this.models.Playlists.PropertyChanged += PlaylistsOnPropertyChanged;
        }

        private void PlaylistsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(PlaylistsModel.ActivePlaylist)) return;
            if (models.Playlists.ActivePlaylist == null) return;

            // raise priority of active playlist
            var idx = priorityQueue.IndexOf(models.Playlists.ActivePlaylist);
            if(idx <= 0) return; // already on top

            var tmp = priorityQueue[idx];
            priorityQueue.RemoveAt(idx);
            priorityQueue.Insert(0, tmp);
        }

        protected int NumTaskRunning { get; private set; } = 0;

        protected abstract TaskModel GetTask(PlaylistModel playlist);

        // indicates if one new task may be started
        protected abstract bool CanExecuteTasks();

        // indicates if the task for this playlist should be started
        protected abstract bool CanExecuteTask(PlaylistModel playlist);

        // keep priority queu up to date
        private void PlaylistOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (var oldItem in args.OldItems)
                {
                    var task = GetTask(oldItem as PlaylistModel);
                    // stop tasks if they are still running
                    if(task.Status == TaskModel.TaskStatus.Running)
                        task.Stop();

                    priorityQueue.Remove(oldItem as PlaylistModel);
                }
            }

            if (args.NewItems != null)
            {
                foreach (var newItem in args.NewItems)
                {
                    var task = GetTask(newItem as PlaylistModel);
                    // callbacks
                    task.PropertyChanged += (s, a) => TaskOnPropertyChanged(newItem as PlaylistModel, a);
                    priorityQueue.Add(newItem as PlaylistModel);
                }
            }

            StartNewTasks();
        }

        /// <summary>
        /// tests if new tasks can be executed
        /// </summary>
        protected void StartNewTasks()
        {
            if(!CanExecuteTasks()) return;

            // start tasks as long as tasks may be executed
            foreach (var playlistModel in priorityQueue)
            {
                var task = GetTask(playlistModel);
                if (task.Status != TaskModel.TaskStatus.ReadyToStart) continue;

                if(!CanExecuteTask(playlistModel)) continue;

                task.Start();
                if(!CanExecuteTasks()) return;
            }
        }

        private void TaskOnPropertyChanged(PlaylistModel sender, PropertyChangedEventArgs args)
        {
            // only status is interesting
            if (args.PropertyName != nameof(TaskModel.Status)) return;

            var task = GetTask(sender);

            switch (task.Status)
            {
                case TaskModel.TaskStatus.Running:
                    NumTaskRunning++;
                    break;
                case TaskModel.TaskStatus.Failed:
                case TaskModel.TaskStatus.Finished:
                    NumTaskRunning--;
                    StartNewTasks();
                    break;
                case TaskModel.TaskStatus.ReadyToStart:
                    StartNewTasks();
                    break;
            }
        }
    }
}
