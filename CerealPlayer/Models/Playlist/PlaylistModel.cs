using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Hoster;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Playlist
{
    public class PlaylistModel
    {
        private readonly IVideoHoster hoster;
        private readonly Models models;

        public PlaylistModel(Models models, string initialWebsite)
        {
            InitialWebsite = initialWebsite;
            this.models = models;

            // get series info
            hoster = models.Web.VideoHoster.GetCompatibleHoster(initialWebsite);
            Name = hoster.GetSeriesTitle(initialWebsite);
            System.IO.Directory.CreateDirectory(Directory);

            // create video and download task
            Videos.Add(new VideoModel(models, initialWebsite, this));
        }

        public string InitialWebsite { get; }

        public string Name { get; }

        public string Directory => models.App.PlaylistDirectory + "/" + Name;

        public ObservableCollection<VideoModel> Videos = new ObservableCollection<VideoModel>();

        public TaskModel NextEpisodeTask { get; } = null;
    }
}
