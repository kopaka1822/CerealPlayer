using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace CerealPlayer.Models.Web
{
    public class BrowserPage
    {
        // timeout reset for website loading
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private readonly ChromiumWebBrowser page;
        private string loadError = null;

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
        ///     Open the given url
        /// </summary>
        /// <param name="url">the url</param>
        /// <returns></returns>
        private void OpenUrl(string url)
        {
            try
            {
                page.LoadingStateChanged += PageLoadingStateChanged;
                page.LoadError += PageOnLoadError;
                loadError = null;
                Debug.Assert(page.IsBrowserInitialized);

                page.Load(url);

                manualResetEvent.WaitOne(TimeSpan.FromSeconds(60));
                manualResetEvent.Reset();
                if(loadError != null)
                    throw new Exception(loadError);

                // wait for ddos protection checks to be over
                bool isCloudflare = false;
                int numIterations = 0;
                do
                {
                    if (isCloudflare)
                    {
                        manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));
                        manualResetEvent.Reset();
                    }

                    // take a look at the source
                    var sourceTask = page.GetSourceAsync();
                    sourceTask.Wait();
                    var source = sourceTask.Result;

                    // check for cloudflare ddos protection
                    isCloudflare = source.Contains(">DDoS protection by Cloudflare</a>");
                } while (isCloudflare && ++numIterations < 10);
            }
            catch (ObjectDisposedException)
            {
                // happens on the manualResetEvent.Reset(); when a cancelation token has disposed the context
            }
            finally
            {
                page.LoadingStateChanged -= PageLoadingStateChanged;
                page.LoadError -= PageOnLoadError;
                page.Stop();
            }
        }

        private void PageOnLoadError(object sender, LoadErrorEventArgs e)
        {
            loadError = e.ErrorText;
            manualResetEvent.Set();
        }

        /// <summary>
        ///     Manage the IsLoading parameter
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