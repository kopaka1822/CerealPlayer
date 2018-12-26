using System;
using System.Collections.Generic;
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
        /// <summary>
        /// The browser page
        /// </summary>
        private ChromiumWebBrowser Page { get; }

        // chromium does not manage timeouts, so we'll implement one
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private readonly RequestContext context;

        public Browser()
        {
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to     specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };

            //Autoshutdown when closing
            CefSharpSettings.ShutdownOnExit = true;

            //Perform dependency check to make sure all relevant resources are in our     output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            context = new RequestContext();
            Page = new ChromiumWebBrowser("", null, context);

            SpinWait.SpinUntil(() => Page.IsBrowserInitialized);
        }

        public void Dispose()
        {
            Page.Dispose();
            context.Dispose();
        }

        public async Task<string> GetSourceAsynch(string url)
        {
            await System.Threading.Tasks.Task.Run(() => OpenUrl(url));
            var source = await Page.GetSourceAsync();
            return source;
        }

        /// <summary>
        /// Open the given url
        /// </summary>
        /// <param name="url">the url</param>
        /// <returns></returns>
        private void OpenUrl(string url)
        {
            try
            {
                Page.LoadingStateChanged += PageLoadingStateChanged;
                if (!Page.IsBrowserInitialized)
                    throw new Exception("OpenUrl - browser not initialized!");
                
                Page.Load(url);

                //create a 60 sec timeout 
                bool isSignalled = manualResetEvent.WaitOne(TimeSpan.FromSeconds(60));
                manualResetEvent.Reset();

                //As the request may actually get an answer, we'll force stop when the timeout is passed
                if (!isSignalled)
                {
                    Page.Stop();
                }

            }
            catch (ObjectDisposedException)
            {
                //happens on the manualResetEvent.Reset(); when a cancelation token has disposed the context
            }
            finally
            {
                Page.LoadingStateChanged -= PageLoadingStateChanged;
            }
        }

        /// <summary>
        /// Manage the IsLoading parameter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            // Check to see if loading is complete - this event is called twice, one when loading starts
            // second time when it's finished
            if (!e.IsLoading)
            {
                manualResetEvent.Set();
            }
        }
    }
}
