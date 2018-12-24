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
    public class StopVideoDownloadCommand : ICommand
    {
        private readonly TaskModel task;

        public StopVideoDownloadCommand(TaskModel task)
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
            return task.Status == TaskModel.TaskStatus.Running;
        }

        public void Execute(object parameter)
        {
            task.Stop();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
