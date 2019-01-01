using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Tasks
{
    /// <summary>
    /// searches for a video link on the goven website and starts the default downloaded with the video link
    /// </summary>
    public class SearchVideoLinkTask : ISubTask
    {
        private readonly Models models;
        private readonly VideoTaskModel parent;
        private readonly string website;
        private readonly string searchString;
        private readonly bool useJavascript;

        /// <param name="models"></param>
        /// <param name="parent"></param>
        /// <param name="website"></param>
        /// <param name="searchString">substring that is included in the video link</param>
        /// <param name="useJavascript">indicates if the website should be resolved with javascript</param>
        public SearchVideoLinkTask(Models models, VideoTaskModel parent, string website, string searchString, bool useJavascript)
        {
            this.models = models;
            this.parent = parent;
            this.website = website;
            this.searchString = searchString;
            this.useJavascript = useJavascript;
        }

        public async void Start()
        {
            try
            {
                parent.Description = "resolving " + website;
                var source = useJavascript
                        ? await models.Web.Html.GetJsAsynch(website)
                        : await models.Web.Html.GetAsynch(website);

                var subIndex = source.IndexOf(searchString, StringComparison.Ordinal);
                if (subIndex < 0)
                {
                    // remove website from cache (did not load correctly)
                    if(useJavascript)
                        models.Web.Html.RemoveCachedJs(website);
                    else
                        models.Web.Html.RemoveCached(website);
                    
                    throw new Exception($"failed to locate \"{searchString}\" in {website}");
                }

                var address = StringUtil.ReadLink(source, subIndex);

                parent.SetNewSubTask(new DefaultVideoDownloader(models, parent, address));
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
