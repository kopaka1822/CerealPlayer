﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task.Hoster;

namespace CerealPlayer.Models.Hoster.Tasks
{
    public class TestWebsiteNot404 : TestWebsiteExistsTask
    {
        private readonly string string404;

        public TestWebsiteNot404(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster, string string404, bool useJavascript = false) : base(models, parent, website, hoster, useJavascript)
        {
            this.string404 = string404;
        }

        protected override async System.Threading.Tasks.Task OnWebsiteExists()
        {
            var source = useJavascript 
                ? await models.Web.Html.GetJsAsynch(website) 
                : await models.Web.Html.GetAsynch(website);
            if(source.Contains(string404))
                throw new Exception("page not found message");
        }
    }
}
