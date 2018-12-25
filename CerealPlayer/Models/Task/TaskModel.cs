using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
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

        private readonly int maxTries = 5;
        private int curRetries = 0;

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
                percentage = clamped;
                OnPropertyChanged(nameof(Percentage));
            }
        }

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

        public void SetNewSubTask(ISubTask newSubTask)
        {
            Debug.Assert(Status == TaskStatus.Running || Status == TaskStatus.NotStarted);
            subTask = newSubTask;
            curRetries = 0;

            if (Status == TaskStatus.Running)
                subTask.Start();
        }

        public void Start()
        {
            Debug.Assert(subTask != null);
            Debug.Assert(Status == TaskStatus.NotStarted);
            Status = TaskStatus.Running;
            subTask.Start();
            curRetries = 0;
        }

        public void Stop()
        {
            Debug.Assert(Status == TaskStatus.Running);
            subTask.Stop();
            Description = "stopped";
            Status = TaskStatus.Failed;
        }

        public void ResetStatus()
        {
            Debug.Assert(Status == TaskStatus.Failed || Status == TaskStatus.Finished);
            Status = TaskStatus.NotStarted;
        }

        public async void SetError(string error)
        {
            Debug.Assert(Status == TaskStatus.Running || Status == TaskStatus.Failed);
            Description = error;
            // retry if max retries not reached
            if (++curRetries < maxTries)
            {
                // start again after 5 s timeout
                await System.Threading.Tasks.Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(5)));
                subTask.Start();
            }
            else
            {
                Status = TaskStatus.Failed;
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
