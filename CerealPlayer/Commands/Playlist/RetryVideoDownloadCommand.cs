using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Commands.Playlist
{
    public class RetryVideoDownloadCommand : ICommand
    {
        private TaskModel task;

        public RetryVideoDownloadCommand(TaskModel task)
        {
            this.task = task;
            task.PropertyChanged += TaskOnPropertyChanged;
        }

        private void TaskOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(TaskModel.Status):
                    OnCanExecuteChanged();
                    break;
            }
        }


        public bool CanExecute(object parameter)
        {
            return task.Status == TaskModel.TaskStatus.Failed;
        }

        public void Execute(object parameter)
        {
            task.Retry();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
