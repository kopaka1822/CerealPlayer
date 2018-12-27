using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CerealPlayer.Models
{
    public class AppModel
    {
        private readonly Stack<Window> windowStack = new Stack<Window>();

        public MainWindow Window { get; }
        public string WorkingDirectory { get; }
        public string PlaylistDirectory { get; }
        public Window TopmostWindow => windowStack.Peek();

        public AppModel(MainWindow window)
        {
            this.Window = window;
            WorkingDirectory = Directory.GetCurrentDirectory();
            PlaylistDirectory = WorkingDirectory + "/playlists";
            Directory.CreateDirectory(PlaylistDirectory);
            windowStack.Push(window);
        }

        /// <summary>
        /// shows a modal dialog and sets the correct dialog owner (topmost window)
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public bool? ShowDialog(Window dialog)
        {
            dialog.Owner = TopmostWindow;
            windowStack.Push(dialog);
            var res = dialog.ShowDialog();
            windowStack.Pop();
            return res;
        }
    }
}
