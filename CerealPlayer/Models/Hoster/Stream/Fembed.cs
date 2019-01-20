using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Hoster.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Stream
{
    public class Fembed : IVideoHoster
    {
        private readonly Models models;

        public Fembed(Models models)
        {
            this.models = models;
        }

        public string Name => "FEmbed";
        public bool IsFileHoster => true;
        public bool Supports(string website)
        {
            return website.Contains("fembed.com");
        }

        public EpisodeInfo GetInfo(string website)
        {
            // the random string after the website title
            // TODO determine correct title
            var idx = website.LastIndexOf('/');
            if (idx < 0) throw new Exception(website + " does not contain a / to determine series title");
            var res = website.Substring(idx + 1);

            return new EpisodeInfo
            {
                SeriesTitle = res,
                EpisodeTitle = res,
                EpisodeNumber = 0
            }; 
        }

        // e620f992735298d0b2d3d763fa034d0ad5c7

        public ISubTask GetDownloadTask(VideoTaskModel parent, string website)
        {
            if (website.Contains("femded.com/v/"))
                return null;

            return new RecursiveHosterDownloadTask(models, parent, website, true);
        }

        public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website)
        {
            return null;
        }

        public string FindCompatibleLink(string source)
        {
            var idx = source.IndexOf("femded.com/v/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return StringUtil.ReadLink(source, idx);

            // hosted on comiconline?
            idx = source.IndexOf("fembed.comicsonline.to/getLinkSimple?file=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return StringUtil.ReadLink(source, idx);

            return null;
        }
    }
}
