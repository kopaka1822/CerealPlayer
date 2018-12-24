using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;

namespace CerealPlayer.Models.Hoster
{
    public class VideoHosterModel
    {
        private readonly List<IVideoHoster> hoster = new List<IVideoHoster>();

        public VideoHosterModel(Models models)
        {
            hoster.Add(new JustDubs(models));
            hoster.Add(new Mp4Upload(models));
        }

        /// <summary>
        /// tries to find a compatible video hoster
        /// </summary>
        /// <param name="website"></param>
        /// <exception cref="Exception">thrown if no hoster was found</exception>
        /// <returns></returns>
        public IVideoHoster GetCompatibleHoster(string website)
        {
            foreach (var videoHoster in hoster)
            {
                if (videoHoster.Supports(website)) return videoHoster;
            }

            throw new Exception("no compatible hoster for " + website);
        }
    }
}
