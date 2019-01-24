using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CerealPlayer.Models
{
    public class AppModel
    {
        private readonly Stack<Window> windowStack = new Stack<Window>();

        public AppModel(MainWindow window)
        {
            Window = window;
            WorkingDirectory = Directory.GetCurrentDirectory();
            PlaylistDirectory = WorkingDirectory + "/playlists";
            Directory.CreateDirectory(PlaylistDirectory);
            windowStack.Push(window);
        }

        public MainWindow Window { get; }
        public string WorkingDirectory { get; }
        public string PlaylistDirectory { get; }
        public Window TopmostWindow => windowStack.Peek();

        /// <summary>
        ///     shows a modal dialog and sets the correct dialog owner (topmost window)
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