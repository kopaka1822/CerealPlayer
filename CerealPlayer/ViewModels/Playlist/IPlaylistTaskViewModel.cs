using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CerealPlayer.ViewModels.Playlist
{
    /// <summary>
    /// inteface for PlaylistTaskView compatible view model
    /// </summary>
    interface IPlaylistTaskViewModel
    {
        string Name { get; }

        ICommand PlayCommand { get; }

        ICommand RetryCommand { get; }

        ICommand StopCommand { get; }

        ICommand DeleteCommand { get; }

        Visibility PlayVisibility { get; }
        
        Visibility RetryVisibility { get; }

        Visibility StopVisibility { get; }

        string Status { get; }

        Visibility ProgressVisibility { get; }

        int Progress { get; set; }
    }
}
