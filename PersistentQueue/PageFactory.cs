using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersistentQueue
{
    internal class PageFactory : IPageFactory
    {
        static readonly string PageFileName = "page";
        static readonly string PageFileSuffix = ".dat";
        static readonly string DefaultCacheKey = "_default_";
        readonly long PageSize;
        readonly string PageDir;

        Dictionary<string, IPage> _cache;

        public PageFactory(long pageSize, string pageDirectory)
        {
            PageSize = pageSize;
            PageDir = pageDirectory;

            if (!Directory.Exists(PageDir))
                Directory.CreateDirectory(PageDir);

            // A simple cache that will will hold a number of pages based on a key. Typically one cached page for head and one for tail.
            _cache = new Dictionary<string, IPage>();       
        }

        string GetFilePath(long index)
        {
            return Path.Combine(PageDir, String.Format("{0}-{1}{2}", PageFileName, index, PageFileSuffix));
        }

        public IPage GetPage(long index, string cacheKey)
        {
            if (!_cache.ContainsKey(cacheKey) || _cache[cacheKey].Index != index)
            {
                // Dispose previously cached page
                if (_cache.ContainsKey(cacheKey))
                    _cache[cacheKey].Dispose();

                // Open/create new page file and add it to cache
                _cache[cacheKey] = new Page(GetFilePath(index), PageSize, index);
            }

            return _cache[cacheKey];
        }

        public IPage GetPage(long index)
        {
            return GetPage(index, DefaultCacheKey);
        }

        public void DeletePage(long index)
        {
            throw new NotImplementedException();
        }
    }
}
