using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster
{
    public class TestWebsiteAndHosterExists : TestWebsiteExistsTask
    {
        private readonly bool runJavascript;

        public TestWebsiteAndHosterExists(Models models, NextEpisodeTaskModel parent, string website, IVideoHoster hoster, bool runJavascript) : base(models, parent, website, hoster)
        {
            this.runJavascript = runJavascript;
        }

        protected override async System.Threading.Tasks.Task OnWebsiteExists()
        {
            var source = runJavascript 
                ? await models.Web.Html.GetJsAsynch(website)
                : await models.Web.Html.GetAsynch(website);
            try
            {
                await models.Web.VideoHoster.GetHosterFromSourceAsynch(website, source);
            }
            catch (Exception)
            {
                // episode does not exists
                throw new Exception("");
            }
            // hosters do exist! continue
        }
    }
}
