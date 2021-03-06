﻿using System.Windows.Input;
using CerealPlayer.Commands.Playlist.All;
using CerealPlayer.Commands.Playlist.NonLoaded;
using CerealPlayer.Commands.Settings;
using CerealPlayer.Controllers;
using CerealPlayer.ViewModels.Player;
using CerealPlayer.ViewModels.Playlist;

namespace CerealPlayer.ViewModels
{
    public class ViewModels
    {
        private readonly Models.Models models;

        // controllers
        public NextEpisodeTaskController NextEpisodeTaskCtrl { get; }
        public DownloadTaskController DownloadTaskCtrl { get; }
        public SaveFileController SaveCtrl { get; }
        public PlayerController PlayerCtrl { get; }
        public DeleteAfterWatchedController DeleteAfterWatchedCtrl { get; }
        public PlayerTouchController PlayerTouchCtrl { get; }

        // commands
        public ICommand NewPlaylistCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand UpdateAllCommand { get; }
        public ICommand GeneralSettingsCommand { get; }
        public ICommand HosterPreferencesCommand { get; }

        public ViewModels(Models.Models models)
        {
            this.models = models;

            // controller
            NextEpisodeTaskCtrl = new NextEpisodeTaskController(models);
            DownloadTaskCtrl = new DownloadTaskController(models);
            SaveCtrl = new SaveFileController(models);
            PlayerCtrl = new PlayerController(models);
            DeleteAfterWatchedCtrl = new DeleteAfterWatchedController(models);
            PlayerTouchCtrl = new PlayerTouchController(models);

            // view models
            ActivePlaylist = new ActivePlaylistViewModel(models);
            PlaylistPreview = new PlaylistsPreviewViewModel(models);
            Player = new PlayerViewModel(models);
            Display = new DisplayViewModel(models);

            // commands
            NewPlaylistCommand = new NewPlaylistCommand(models);
            StopAllCommand = new StopAllTasksCommand(models);
            UpdateAllCommand = new UpdateAllPlaylistsCommand(models, PlaylistPreview);
            GeneralSettingsCommand = new ShowGeneralSettingsCommand(models);
            HosterPreferencesCommand = new ShowHosterPreferencesCommand(models);
        }

        // view models
        public ActivePlaylistViewModel ActivePlaylist { get; }
        public PlaylistsPreviewViewModel PlaylistPreview { get; }
        public PlayerViewModel Player { get; }
        public DisplayViewModel Display { get; }

        public void Dispose()
        {
            models.Playlists.ActivePlaylist = null;

            DeleteAfterWatchedCtrl.Dispose();
            SaveCtrl.Dispose();
        }
    }
}