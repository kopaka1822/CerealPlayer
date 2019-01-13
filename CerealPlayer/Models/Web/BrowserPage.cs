using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace CerealPlayer.Models.Web
{
    public class BrowserPage
    {
        private readonly ChromiumWebBrowser page;

        // timeout reset for website loading
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        public BrowserPage()
        {
            page = new ChromiumWebBrowser();
            SpinWait.SpinUntil(() => page.IsBrowserInitialized);
        }

        public void Dispose()
        {
            page.Dispose();
        }

        public async Task<string> GetSourceAsynch(string url)
        {
            await System.Threading.Tasks.Task.Run(() => OpenUrl(url));
            return await page.GetSourceAsync();
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
                page.LoadingStateChanged += PageLoadingStateChanged;
                Debug.Assert(page.IsBrowserInitialized);

                page.Load(url);

                manualResetEvent.WaitOne(TimeSpan.FromSeconds(60));
                manualResetEvent.Reset();
            }
            catch (ObjectDisposedException)
            {
                // happens on the manualResetEvent.Reset(); when a cancelation token has disposed the context
            }
            finally
            {
                page.LoadingStateChanged -= PageLoadingStateChanged;
                page.Stop();
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
