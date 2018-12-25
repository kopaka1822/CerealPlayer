using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;

namespace CerealPlayer.Models.Task
{
    public class NextEpisodeTaskModel : TaskModel
    {
        public NextEpisodeTaskModel(PlaylistModel playlist)
        {
            Playlist = playlist;
        }

        public PlaylistModel Playlist { get; }
    }
}
