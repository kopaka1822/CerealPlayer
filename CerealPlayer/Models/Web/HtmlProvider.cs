using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CerealPlayer.Models.Web
{
    public class HtmlProvider
    {
        private readonly Browser browser;

        private readonly Dictionary<string, Task<string>> cachedJsWebsites = new Dictionary<string, Task<string>>();

        private readonly Dictionary<string, Task<string>> cachedWebsites = new Dictionary<string, Task<string>>();

        public HtmlProvider(int maxChromiumInstances)
        {
            browser= new Browser(maxChromiumInstances);
        }

        /// <summary>
        ///     gets the website from the cache or loads the website if it is not in the cache.
        ///     This version executes javascript on the website before returning the source
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public async Task<string> GetJsAsynch(string website)
        {
            if (cachedJsWebsites.TryGetValue(website, out var existingTask))
                return await existingTask;

            var newTask = browser.GetSourceAsynch(website);
            cachedJsWebsites.Add(website, newTask);
            return await newTask;
        }

        public void Dispose()
        {
            browser.Dispose();
        }

        public async Task<string> GetAsynch(string website)
        {
            if (cachedWebsites.TryGetValue(website, out var existingTask))
                return await existingTask;

            using (var wc = new WebClient())
            {
                var newTask = wc.DownloadStringTaskAsync(website);
                cachedWebsites.Add(website, newTask);
                return await newTask;
            }
        }

        /// <summary>
        ///     tests if a website exists. (removed a previous cache copy if existing)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAvailable(string website)
        {
            RemoveCached(website);
            try
            {
                var source = await GetAsynch(website);
                if (source.Length == 0) return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     tests if a website exists. (removed a previous cache copy if existing)
        ///     executes javascript to load the page (required by some websites)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAvailableJs(string website)
        {
            RemoveCachedJs(website);
            try
            {
                var source = await GetJsAsynch(website);
                if (source.Length == 0) return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     deletes a copy of the given website from the cache (the version that was executed with javascript)
        /// </summary>
        /// <param name="website"></param>
        public void RemoveCachedJs(string website)
        {
            cachedJsWebsites.Remove(website);
        }

        public void RemoveCached(string website)
        {
            cachedWebsites.Remove(website);
        }
    }
}