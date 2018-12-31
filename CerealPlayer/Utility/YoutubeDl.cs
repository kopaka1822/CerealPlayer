using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Utility
{
    public static class YoutubeDl
    {
        public static string GetDownloadUrl(string website)
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "deps/youtube-dl.exe",
                    Arguments = "--get-url " + website
                }
            };

            if (!p.Start())
                throw new Exception("could not start deps/youtube-dl.exe");

            var cout = p.StandardOutput.ReadToEnd();
            var cerr = p.StandardError.ReadToEnd();

            p.WaitForExit();
            if (!String.IsNullOrEmpty(cerr))
                throw new Exception(cerr.Trim());

            return cout.Trim();
        }

        public static async Task<string> GetDownloadUrlAsynch(string website)
        {
            return await System.Threading.Tasks.Task.Run(() => GetDownloadUrl(website));
        }
    }
}
