using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Models.Hoster
{
    public interface IVideoHoster
    {
        bool Supports(string website);
        string GetSeriesTitle(string website);
        string GetEpisodeTitle(string website);
        ISubTask GetDownloadTask(TaskModel parent, string website);
        ISubTask GetNextEpisodeTask(TaskModel parent, string website);
    }
}
