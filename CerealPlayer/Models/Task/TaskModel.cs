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

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        public string Description
        {
            get => History.Count > 0 ? History.Last() : "";
            set
            {
                if(Description == value) return;
                History.Add(value);
                OnPropertyChanged(nameof(Description));
            }
        }

        private int progress = 0;

        public int Progress
        {
            get => progress;
            set
            {
                var clamped = Math.Min(Math.Max(value, 0), 100);
                if (progress == clamped) return;
                // progress: oldValue
                // clamped: newValue
                var oldValue = progress;
                var newValue = clamped;
                if (newValue < oldValue || oldValue == 0)
                {
                    // reset progress timer etc.
                    lastProgressReceived = DateTime.Now;
                    timeForOnePercent = 0;
                    ProgressTimeRemaining = TimeSpan.Zero;
                }
                else // newValue > oldValue > 0
                {
                    // calculate progress time
                    var now = DateTime.Now;
                    var elapsed = now - lastProgressReceived;
                    lastProgressReceived = now;

                    var newTimeForOnePercent = (long)(elapsed.Ticks / (double)(newValue - oldValue));
                    if (timeForOnePercent == 0)
                        timeForOnePercent = newTimeForOnePercent;
                    else
                    {
                        // use some of the previously predicted time
                        timeForOnePercent = (long)(timeForOnePercent * 0.8 + newTimeForOnePercent * 0.2);
                    }

                    ProgressTimeRemaining = TimeSpan.FromTicks(timeForOnePercent * (100 - newValue));
                }
                progress = clamped;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private DateTime lastProgressReceived;

        private long timeForOnePercent = 0;

        private TimeSpan progressTimeRemaining = TimeSpan.Zero;
        private DateTime progressTimeRemainingSet = DateTime.Now;

        /// <summary>
        /// a prediction when the progress will reach 100
        /// this is exluded from the OnPropertyChanged and will
        /// be changed each second and after Progress was set
        /// </summary>
        public TimeSpan ProgressTimeRemaining
        {
            get
            {
                var now = DateTime.Now;
                var elapsed = now - progressTimeRemainingSet;

                var remaining = progressTimeRemaining.Subtract(elapsed);
                if(remaining < TimeSpan.Zero)
                    return TimeSpan.Zero;
                return remaining;
            }
            private set
            {
                progressTimeRemaining = value;
                progressTimeRemainingSet = DateTime.Now;
            }
        }

        /// <summary>
        /// returns the progress time remaining as string or an emtpy string if the time is zero
        /// </summary>
        /// <param name="prefix">prefix that will be added if the time is not zero</param>
        /// <returns></returns>
        public string GetProgressTimeRemainingString(string prefix)
        {
            var time = ProgressTimeRemaining;
            if (time == TimeSpan.Zero) return "";

            return prefix + time.ToString(@"hh\:mm\:ss");   
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

        /// <summary>
        /// returns true if the status is either ReadyToStart or Running
        /// </summary>
        public bool ReadyOrRunning => Status == TaskStatus.ReadyToStart || Status == TaskStatus.Running;

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
            Debug.Assert(Status == TaskStatus.ReadyToStart || Status == TaskStatus.Failed);
            stopRequested = false;
            Status = TaskStatus.Running;
            curRetries = 0;
            subTask.Start();
        }

        public void Stop()
        {
            if (Status == TaskStatus.Running)
            {
                stopRequested = true;
                subTask.Stop();
            }
            else if(Status == TaskStatus.ReadyToStart)
            {
                // prevent execution by setting the failed flag
                Description = "Stopped by user";
                Status = TaskStatus.Failed;
            }
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
            if(error != null)
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
            Status = TaskStatus.Finished;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
