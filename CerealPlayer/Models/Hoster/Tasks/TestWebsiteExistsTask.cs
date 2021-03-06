﻿using System;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;

namespace CerealPlayer.Models.Hoster.Tasks
{
    public class TestWebsiteExistsTask : ISubTask
    {
        protected readonly IVideoHoster hoster;
        protected readonly Models models;
        protected readonly NextEpisodeTaskModel parent;
        protected readonly string website;
        protected readonly bool useJavascript;

        public TestWebsiteExistsTask(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster, bool useJavascript = false)
        {
            this.models = models;
            this.parent = parent;
            this.website = website;
            this.hoster = hoster;
            this.useJavascript = useJavascript;
        }

        public async void Start()
        {
            try
            {
                // test if the website exists
                parent.Description = "resolving " + website;
                var existing = useJavascript
                    ? await models.Web.Html.IsAvailableJs(website)
                    : await models.Web.Html.IsAvailable(website);
                if (existing)
                {
                    await OnWebsiteExists();
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

        /// <summary>
        ///     this method will be called if the website exists.
        ///     An error should be thrown if the next episode does not exist nevertheless
        /// </summary>
#pragma warning disable 1998
        protected virtual async System.Threading.Tasks.Task OnWebsiteExists()
        {
        }
#pragma warning restore 1998
    }
}