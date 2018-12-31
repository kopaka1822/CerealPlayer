using System;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;

namespace CerealPlayer.Models.Hoster.Tasks
{
    /// <summary>
    /// this download tasks search for other existing hosters on the website
    /// </summary>
    public class RecursiveHosterDownloadTask : ISubTask
    {
        private readonly Models models;
        private readonly string website;
        private readonly VideoTaskModel parent;
        private readonly bool useJavascript;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="parent"></param>
        /// <param name="website"></param>
        /// <param name="useJavascript">indicates if javascript should be executed to retrieve the website source</param>
        public RecursiveHosterDownloadTask(Models models, VideoTaskModel parent, string website, bool useJavascript)
        {
            this.website = website;
            this.useJavascript = useJavascript;
            this.parent = parent;
            this.models = models;
        }

        public async void Start()
        {
            try
            {
                parent.Description = "resolving " + website;
                var source = useJavascript 
                    ? await models.Web.Html.GetJsAsynch(website)
                    : await models.Web.Html.GetAsynch(website);
                var newTask = await models.Web.VideoHoster.GetHosterFromSourceAsynch(website, source);

                parent.SetNewSubTask(newTask.GetDownloadTask(parent));
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
