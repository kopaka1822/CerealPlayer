using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class NextEpisodeTaskController : TaskControllerBase
    {
        public NextEpisodeTaskController(Models.Models models) : base(models)
        {
        }

        protected override TaskModel GetTask(PlaylistModel playlist)
        {
            return playlist.NextEpisodeTask;
        }

        protected override bool CanExecuteTasks()
        {
            return true;
        }

        protected override bool CanExecuteTask(PlaylistModel playlist)
        {
            return true;
        }
    }
}