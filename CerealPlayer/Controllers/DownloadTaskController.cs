﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Playlist;
using CerealPlayer.Models.Task;

namespace CerealPlayer.Controllers
{
    public class DownloadTaskController : TaskControllerBase
    {
        public DownloadTaskController(Models.Models models) : base(models)
        {
        }

        protected override TaskModel GetTask(PlaylistModel playlist)
        {
            return playlist.DownloadPlaylistTask;
        }

        protected override bool CanExecuteTasks()
        {
            return NumTaskRunning < 3;
        }
    }
}