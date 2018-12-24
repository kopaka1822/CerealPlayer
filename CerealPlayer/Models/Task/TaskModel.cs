using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task
{
    public class TaskModel : INotifyPropertyChanged
    {
        public enum TaskStatus
        {
            NotStarted,
            Running,
            Finished,
            Failed
        }

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>{"Pending"};

        public string Description
        {
            get => History.Count > 0 ? History.Last() : "";
            set
            {
                History.Add(value);
                OnPropertyChanged(nameof(Description));
            }
        }

        private int percentage = 0;

        public int Percentage
        {
            get => percentage;
            set
            {
                var clamped = Math.Min(Math.Max(value, 0), 100);
                if (percentage == clamped) return;
                OnPropertyChanged(nameof(Percentage));
            }
        }

        public VideoModel Video { get; }

        private TaskStatus status = TaskStatus.NotStarted;

        public TaskStatus Status
        {
            get => status;
            set
            {
                if (status == value) return;
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private ISubTask subTask;

        public TaskModel(VideoModel parent)
        {
            this.Video = parent;
        }

        public void SetNewSubTask(ISubTask newSubTask)
        {
            Debug.Assert(Status == TaskStatus.Running || Status == TaskStatus.NotStarted);
            subTask = newSubTask;

            if (Status == TaskStatus.Running)
                subTask.Start();
        }

        public void Start()
        {
            Debug.Assert(subTask != null);
            Debug.Assert(Status == TaskStatus.NotStarted);
            subTask.Start();
        }

        public void Stop()
        {
            Debug.Assert(Status == TaskStatus.Running);
            subTask.Stop();
            Status = TaskStatus.NotStarted;
        }

        public void Retry()
        {
            Debug.Assert(Status == TaskStatus.Failed);
            subTask.Start();
        }

        public void SetError(string error)
        {
            Description = error;
            Status = TaskStatus.Failed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
