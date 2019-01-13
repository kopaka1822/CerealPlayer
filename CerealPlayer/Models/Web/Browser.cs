using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using CerealPlayer.Properties;

namespace CerealPlayer.Models.Web
{
    public class Browser
    {
        private readonly ConcurrentStack<BrowserPage> pages = new ConcurrentStack<BrowserPage>();

        public Browser()
        {
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to     specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CefSharp\\Cache"),
            };

            CefSharpSettings.ShutdownOnExit = false;

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, true, null);

            // add some emtpy pages to the stack. the pages will be created if needed
            var maxPages = Settings.Default.MaxChromiumInstances;
            Debug.Assert(maxPages > 0);
            for (var i = 0; i < maxPages; ++i)
                pages.Push(null);
        }

        public async Task<string> GetSourceAsynch(string url)
        {
            BrowserPage page = null;

            await System.Threading.Tasks.Task.Run(() =>
            {
                // get page
                SpinWait.SpinUntil(() => pages.TryPop(out page));
                if (page == null)
                {
                    // create browser instance
                    page = new BrowserPage();
                }
            });

            Debug.Assert(page != null);
            var res = await page.GetSourceAsynch(url);
            pages.Push(page);
            return res;
        }

        public void Dispose()
        {
            foreach (var browserPage in pages)
            {
                browserPage?.Dispose();
            }

            Cef.Shutdown();
        }
    }
}