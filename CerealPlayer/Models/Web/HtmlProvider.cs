using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Models.Web
{
    public class HtmlProvider
    {
        private readonly Browser browser = new Browser();

        private readonly Dictionary<string, Task<string>> cachedWebsites = new Dictionary<string, Task<string>>();

        /// <summary>
        /// gets the website from the cache or loads the website if it is not in the cache
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public async Task<string> GetAsynch(string website)
        {
            if (cachedWebsites.TryGetValue(website, out var existingTask))
                return await existingTask;

            var newTask = browser.GetSourceAsynch(website);
            cachedWebsites.Add(website, newTask);
            return await newTask;
        }

        /// <summary>
        /// deletes a copy of the given website from the cache
        /// </summary>
        /// <param name="website"></param>
        public void RemoveCached(string website)
        {
            cachedWebsites.Remove(website);
        }
    }
}
