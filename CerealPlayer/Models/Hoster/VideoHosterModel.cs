using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Hoster.Series;
using CerealPlayer.Models.Hoster.Stream;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster
{
    public class VideoHosterModel
    {
        private readonly List<IVideoHoster> hoster = new List<IVideoHoster>();

        public class WebsiteHosterPair
        {
            public string Website { set; get; }
            public IVideoHoster Hoster { set; get; }

            public ISubTask GetDownloadTask(VideoTaskModel parent)
            {
                return Hoster.GetDownloadTask(parent, Website);
            }

            public ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent)
            {
                return Hoster.GetNextEpisodeTask(parent, Website);
            }
        }

        public VideoHosterModel(Models models)
        {
            // order determines internal ration: The first hoster is the preferred hoster
            
            hoster.Add(new Openload(models));
            hoster.Add(new Oload(models));
            hoster.Add(new StreamAnGo(models));
            hoster.Add(new Mp4Upload(models));
            

            hoster.Add(new JustDubs(models));
            hoster.Add(new GoGoAnimes(models));
            hoster.Add(new MasterAnime(models));
        }

        /// <summary>
        /// tries to find a compatible video hoster
        /// </summary>
        /// <param name="website"></param>
        /// <exception cref="Exception">thrown if no hoster was found</exception>
        /// <returns></returns>
        public IVideoHoster GetCompatibleHoster([NotNull] string website)
        {
            foreach (var videoHoster in hoster)
            {
                if (videoHoster.Supports(website)) return videoHoster;
            }

            throw new Exception("no compatible hoster for " + website);
        }

        /// <summary>
        /// tries to find a compatible hoster on given website
        /// </summary>
        /// <param name="website">website address</param>
        /// <param name="source">source of the website</param>
        /// <returns></returns>
        public async Task<WebsiteHosterPair> GetHosterFromSourceAsynch([NotNull] string website, [NotNull] string source)
        {
            var task = await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var videoHoster in hoster)
                {
                    var link = videoHoster.FindCompatibleLink(source);
                    if (link != null)
                        return new WebsiteHosterPair
                        {
                            Hoster = videoHoster,
                            Website = link
                        };
                }

                return null;
            });
           
            if(task == null)
                throw new Exception("no compatible hosters found on " + website);

            return task;
        }
    }
}
