using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster
{
    public class TestWebsiteExistsTask : ISubTask
    {
        private readonly Models models;
        private readonly NextEpisodeTaskModel parent;
        private readonly IVideoHoster hoster;
        private readonly string website;

        public TestWebsiteExistsTask(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster)
        {
            this.models = models;
            this.parent = parent;
            this.website = website;
            this.hoster = hoster;
        }

        public async void Start()
        {
            try
            {
                // test if the website exists
                parent.Description = "resolving " + website;
                var existing = await models.Web.Html.IsAvailable(website);
                if (existing)
                {
                    // schedule next episode and start new task
                    parent.Playlist.AddNextEpisode(website);
                    parent.SetNewSubTask(hoster.GetNextEpisodeTask(parent, website));
                }
                else
                {
                    // no more episodes (for now)
                    parent.SetError("");
                }
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        public void Stop()
        {

        }
    }
}
