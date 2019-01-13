using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Models.Task;
using CerealPlayer.Utility;

namespace CerealPlayer.Models.Hoster.Tasks
{
    public class YoutubeDlDownloader : ISubTask
    {
        private readonly Models models;
        private readonly VideoTaskModel parent;
        private readonly string website;
        private Process process;
        private long videoLength = 0;
        private string videoLengthString = "";
        private DateTime lastProgess = DateTime.Now;
        private readonly TimeSpan progressCooldown = TimeSpan.FromSeconds(5);

        public YoutubeDlDownloader(Models models, VideoTaskModel parent, string website)
        {
            this.models = models;
            this.parent = parent;
            this.website = website;
        }

        public async void Start()
        {
            try
            {
                parent.Description = "downloading " + website;
                await System.Threading.Tasks.Task.Run(() => DownloadThread());
            }
            catch (Exception e)
            {
                parent.SetError(e.Message);
            }
        }

        void DownloadThread()
        {
            process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "deps/youtube-dl.exe",
                    Arguments = "--newline -o \"" + parent.Video.FileLocation + "\" " + website
                }
            };

            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;

            if (!process.Start())
                throw new Exception("could not start deps/youtube-dl.exe");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process = null;
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data.StartsWith("frame="))
            {
                var now = DateTime.Now;
                if ((now - lastProgess) < progressCooldown)
                {
                    return;
                }

                lastProgess = now;
                // extract current duration
                var idx = args.Data.IndexOf("time=", StringComparison.Ordinal);
                var str = StringUtil.SubstringUntil(args.Data, idx + "time=".Length, ' ');
                var ts = TimeSpan.Parse(str);
                var curTicks = ts.Ticks;

                parent.Progress = (int) ((curTicks * 100) / videoLength);
                parent.Description = ts.ToString(@"hh\:mm\:ss") + "/" + videoLengthString;
                return;
            }

            if (args.Data.StartsWith("  Duration:"))
            {
                // extract duration
                var str = StringUtil.SubstringUntil(args.Data, " Duration:".Length + 2, ',');
                var ts = TimeSpan.Parse(str);
                videoLength = ts.Ticks;
                videoLengthString = ts.ToString(@"hh\:mm\:ss");
                return;
            }

            if(!args.Data.StartsWith("ERROR"))
                return;

            process?.Kill();
            // set error
            throw new Exception(args.Data);
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            parent.Description = args.Data;
        }

        public void Stop()
        {
            try
            {
                process?.Kill();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
