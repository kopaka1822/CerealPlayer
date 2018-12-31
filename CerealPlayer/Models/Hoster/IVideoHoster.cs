using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;
using CerealPlayer.Models.Task;
using CerealPlayer.Models.Task.Hoster;

namespace CerealPlayer.Models.Hoster
{
    public struct EpisodeInfo
    {
        [NotNull]
        public string SeriesTitle { get; set; }
        [NotNull]
        public string EpisodeTitle { get; set; }

        public int EpisodeNumber { get; set; }
    }

    public interface IVideoHoster
    {
        bool Supports(string website);

        EpisodeInfo GetInfo(string website);
        
        /// <summary>
        /// get task that resolved the website into a download link and downloads the video
        /// to the location specified by the VideoModel (member of VideoTaskModel).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="website"></param>
        /// <returns></returns>
        ISubTask GetDownloadTask(VideoTaskModel parent, string website);
        
        /// <summary>
        /// get task that checks if the next episode exists.
        /// Note: The task status should change to Failed if
        /// the next episode does not exits yet but may come in the future.
        /// Only set the finished flag if it is certain that there will be
        /// no more episodes. (use task.setError(""))
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="website"></param>
        /// <returns>may be null if a next episode cannot be determined</returns>
        ISubTask GetNextEpisodeTask(NextEpisodeTaskModel parent, string website);

        /// <summary>
        /// tries to find a link that is supported by this hoster on the given website source
        /// </summary>
        /// <param name="websiteSource"></param>
        /// <returns>a compatible link or null if incompatible</returns>
        string FindCompatibleLink(string websiteSource);
    }
}
