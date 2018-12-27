using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CerealPlayer.Commands;
using CerealPlayer.Commands.Playlist;
using CerealPlayer.Commands.Playlist.All;
using CerealPlayer.Commands.Playlist.NonLoaded;
using CerealPlayer.Controllers;
using CerealPlayer.ViewModels.Player;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.ViewModels
{
    public class ViewModels
    {
        // view models
        public ActivePlaylistViewModel ActivePlaylist { get; }
        public PlaylistsPreviewViewModel PlaylistPreview { get; }
        public PlayerViewModel Player { get; }
        public DisplayViewModel Display { get; }

        // controllers
        public NextEpisodeTaskController NextEpisodeTaskCtrl { get; }
        public DownloadTaskController DownloadTaskCtrl { get; }
        public SaveFileController SaveCtrl { get; }

        // commands
        public ICommand NewPlaylistCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand UpdateAllCommand { get; }

        public ViewModels(Models.Models models)
        {
            // controller
            NextEpisodeTaskCtrl = new NextEpisodeTaskController(models);
            DownloadTaskCtrl = new DownloadTaskController(models);
            SaveCtrl = new SaveFileController(models);

            // view models
            ActivePlaylist = new ActivePlaylistViewModel(models);
            PlaylistPreview = new PlaylistsPreviewViewModel(models);
            Player = new PlayerViewModel(models);
            Display = new DisplayViewModel(models);

            // commands
            NewPlaylistCommand = new NewPlaylistCommand(models);
            StopAllCommand = new StopAllTasksCommand(models);
            UpdateAllCommand = new UpdateAllPlaylistsCommand(models, PlaylistPreview);
        }

        public void Dispose()
        {
            SaveCtrl.Dispose();
        }
    }
}
