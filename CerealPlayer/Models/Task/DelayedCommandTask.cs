using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CerealPlayer.Models.Task
{
    /// <summary>
    /// this tasks executes the given command after the specified time
    /// </summary>
    public class DelayedCommandTask : ISubTask
    {
        private readonly TaskModel parent;
        private readonly ICommand command;
        private readonly string description;
        private readonly int delaySeconds;
        private bool abort = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">parent task</param>
        /// <param name="command">command that will be executed after the delay</param>
        /// <param name="delaySeconds">delay in seconds</param>
        /// <param name="description">message that will be followed by the remaining time => parent.Description = description + $"{remainingTime} s"</param>
        public DelayedCommandTask(TaskModel parent, ICommand command, int delaySeconds, string description)
        {
            this.command = command;
            this.delaySeconds = delaySeconds;
            this.description = description;
            this.parent = parent;
        }

        public async void Start()
        {
            Debug.Assert(command.CanExecute(null));
            abort = false;
            try
            {
                for (int i = 0; i < delaySeconds; ++i)
                {
                    parent.Description = description + $"{delaySeconds - i}s";
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
                    if (abort) throw new Exception("aborted by user");
                }

                Debug.Assert(command.CanExecute(null));
                command.Execute(null);
                parent.SetFinished();
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        public void Stop()
        {
            abort = true;
        }
    }
}
