using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Controllers;
using CerealPlayer.ViewModels.Player;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.ViewModels
{
    public class ViewModels
    {
        // view models
        public ActivePlaylistViewModel ActivePlaylist { get; }
        public PlayerViewModel Player { get; }
        public DisplayViewModel Display { get; }

        // controllers
        public TaskController TaskCtrl { get; }
        public SaveFileController SaveCtrl { get; }

        // commands
        public ICommand OpenPlaylistCommand { get; }
        public ICommand NewPlaylistCommand { get; }

        public ViewModels(Models.Models models)
        {
            // controller
            TaskCtrl = new TaskController(models);
            SaveCtrl = new SaveFileController(models);

            // view models
            ActivePlaylist = new ActivePlaylistViewModel(models, TaskCtrl);
            Player = new PlayerViewModel(models);
            Display = new DisplayViewModel(models);

            // commands
            OpenPlaylistCommand = new OpenPlaylistCommand(models);
            NewPlaylistCommand = new NewPlaylistCommand(models);
        }

        public void Dispose()
        {
            SaveCtrl.Dispose();
        }
    }
}
