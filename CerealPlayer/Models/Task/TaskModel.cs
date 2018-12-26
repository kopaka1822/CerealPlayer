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
    public class TaskModel : INotifyPropertyChanged, ISubTask
    {
        public enum TaskStatus
        {
            ReadyToStart, // The sub task is not running and ready to be started
                          // => will be scheduled as soon as possible
            Running,      // The sub task is currently executing
                          // => can be stopped by the user
            Finished,     // The sub task is ready and there are not subtasks left 
                          // => this wont be scheduled again
            Failed        // The sub task execution failed or was stopped (and not finished)
                          // => restart by user required
        }

        private readonly TimeSpan taskDelay;

        private readonly int maxTries;
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

        // indicates if this action should stop as soon as possible
        private bool stopRequested = false;

        private TaskStatus status = TaskStatus.Finished;

        /// <summary>
        /// Sets the status of the task.
        /// Note: should only be set by its subtasks.
        /// The initial status is Finished (since there are not subtasks)
        /// </summary>
        public TaskStatus Status
        {
            get => status;
            private set
            {
                if (status == value) return;
                status = value;

                OnPropertyChanged(nameof(Status));
            }
        }

        private ISubTask subTask;

        /// <param name="maxTries">maximum number of tries to get a subtask done before the failed flag is set</param>
        /// <param name="taskDelay">delay between execution of subtasks</param>
        public TaskModel(int maxTries, TimeSpan taskDelay)
        {
            this.maxTries = maxTries;
            this.taskDelay = taskDelay;
        }

        public void Start()
        {
            Debug.Assert(subTask != null);
            Debug.Assert(Status == TaskStatus.ReadyToStart);
            stopRequested = false;
            Status = TaskStatus.Running;
            subTask.Start();
            curRetries = 0;
        }

        public void Stop()
        {
            Debug.Assert(Status == TaskStatus.Running);
            stopRequested = true;
            subTask.Stop();
        }

        /// <summary>
        /// sets the new sub task (and executes it if status == running).
        /// this should be the last command in a sub task
        /// </summary>
        /// <param name="newSubTask"></param>
        public async void SetNewSubTask([NotNull] ISubTask newSubTask)
        {
            Debug.Assert(Status == TaskStatus.Running || Status == TaskStatus.Finished);
            subTask = newSubTask;
            curRetries = 0;

            if (stopRequested)
                Status = TaskStatus.Failed;

            if (Status == TaskStatus.Running)
            {
                // wait a little bit before continuing
                if (taskDelay != TimeSpan.Zero)
                    await System.Threading.Tasks.Task.Delay(taskDelay);

                subTask.Start();
            }

            if (Status == TaskStatus.Finished)
                Status = TaskStatus.ReadyToStart;
        }

        public void ResetStatus()
        {
            Debug.Assert(Status == TaskStatus.Failed);
            Status = TaskStatus.ReadyToStart;
        }

        /// <summary>
        /// Sets the error and schedules a retry if maximum number of tries was not exceeded
        /// </summary>
        /// <param name="error">error message (may be null)</param>
        public async void SetError(string error)
        {
            Debug.Assert(Status == TaskStatus.Running || Status == TaskStatus.Failed);
            if(!string.IsNullOrEmpty(error))
                Description = error;
            // retry if max retries not reached
            if (++curRetries < maxTries && !stopRequested)
            {
                // start again after 5s timeout
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5));
                subTask.Start();
            }
            else
            {
                Status = TaskStatus.Failed;
            }
        }

        public void SetReadyToStart()
        {
            Debug.Assert(Status == TaskStatus.Running);
            Status = stopRequested ? TaskStatus.Failed : TaskStatus.ReadyToStart;
        }

        public void SetFinished()
        {
            Debug.Assert(Status == TaskStatus.Running);
            subTask = null;
            OnSetFinished();

            if(subTask == null)
                Status = TaskStatus.Finished;
        }

        /// <summary>
        /// will be called if the sub tasks called SetFinished (from a running task).
        /// A new subtask can be set to avoid the task from setting the finished flag
        /// </summary>
        /// <returns></returns>
        protected virtual void OnSetFinished()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
