using System;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task.Hoster
{
    public class NextEpisodeTaskModel : TaskModel
    {
        public NextEpisodeTaskModel(PlaylistModel playlist) : base(2, TimeSpan.FromSeconds(2))
        {
            Playlist = playlist;
        }

        public PlaylistModel Playlist { get; }
        public HosterPreferences Hoster => Playlist.Settings.HosterPreferences;
    }
}