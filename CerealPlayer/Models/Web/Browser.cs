using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace CerealPlayer.Models.Web
{
    public class Browser
    {
        private static readonly int MaxPages = 6;
        private ConcurrentStack<BrowserPage> pages = new ConcurrentStack<BrowserPage>();

        public Browser()
        {
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to     specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };

            CefSharpSettings.ShutdownOnExit = false;

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            // add some emtpy pages to the stack. the pages will be created if needed
            for(int i = 0; i < MaxPages; ++i)
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
            Cef.Shutdown();
        }
    }
}
