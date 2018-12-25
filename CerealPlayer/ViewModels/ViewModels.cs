using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Controllers;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.ViewModels
{
    public class ViewModels
    {
        // view models
        public ActivePlaylistViewModel ActivePlaylist { get; }

        // controllers
        public TaskController TaskCtrl { get; }

        // commands
        public ICommand OpenPlaylistCommand { get; }
        public ICommand NewPlaylistCommand { get; }

        public ViewModels(Models.Models models)
        {
            // controller
            TaskCtrl = new TaskController(models);

            // view models
            ActivePlaylist = new ActivePlaylistViewModel(models, TaskCtrl);

            // commands
            OpenPlaylistCommand = new OpenPlaylistCommand(models);
            NewPlaylistCommand = new NewPlaylistCommand(models);
        }
    }
}
