using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster.Tasks
{
    /// <summary>
    /// searches for a video link on the goven website and starts the YoutubeDlLinkTask to downloaded the video
    /// </summary>
    public class SearchVideoLinkYtDlTask : SearchVideoLinkTask
    {
        public SearchVideoLinkYtDlTask(Models models, VideoTaskModel parent, string website, string searchString, bool useJavascript) : base(models, parent, website, searchString, useJavascript)
        {
        }

        protected override ISubTask GetNewTask(Models mdls, VideoTaskModel video, string site)
        {
            return new YoutubeDlLinkTask(mdls, video, site);
        }
    }
}
