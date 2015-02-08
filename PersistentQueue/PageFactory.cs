using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersistentQueue
{
    internal class PageFactory : IPageFactory, IDisposable
    {
        bool disposed = false;
        static readonly string PageFileName = "page";
        static readonly string PageFileSuffix = ".dat";
        static readonly string DefaultCacheKey = "_default_";
        readonly long PageSize;
        readonly string PageDir;

        Dictionary<string, IPage> _cache;
        Dictionary<string, IPage> _pageCache;

        public PageFactory(long pageSize, string pageDirectory)
        {
            PageSize = pageSize;
            PageDir = pageDirectory;

            if (!Directory.Exists(PageDir))
                Directory.CreateDirectory(PageDir);

            // A simple cache that will will hold a number of pages based on a key. Typically one cached page for head and one for tail.
            _cache = new Dictionary<string, IPage>();     
  
            // A simple cache using the page filename as key.
            _pageCache = new Dictionary<string, IPage>();
        }

        string GetFilePath(long index)
        {
            return Path.Combine(PageDir, String.Format("{0}-{1}{2}", PageFileName, index, PageFileSuffix));
        }

        public IPage GetPage(long index, string cacheKey)
        {
            if (!_cache.ContainsKey(cacheKey) || _cache[cacheKey].Index != index)
            {
                var filePath = GetFilePath(index);

                // Dispose previously cached page
                if (_cache.ContainsKey(cacheKey))
                {
                    // Do we have a reference to this page in _pageCache as well? If so, remove it from pageCache
                    var pageCacheKey = _pageCache.SingleOrDefault(o => object.ReferenceEquals(_cache[cacheKey], o.Value)).Key;
                    if (pageCacheKey != null)
                        _pageCache.Remove(pageCacheKey);
                    
                    // Dispose page and remove it from _cache for my cacheKey
                    _cache[cacheKey].Dispose();
                    _cache.Remove(cacheKey);
                }

                // Open/create new page file and add it to cache
                // Check first if the page is in the pageCache
                if (!_pageCache.ContainsKey(filePath))
                    _pageCache[filePath] = new Page(GetFilePath(index), PageSize, index);

                _cache[cacheKey] = _pageCache[filePath];
            }
            return _cache[cacheKey];
        }

        public IPage GetPage(long index)
        {
            return GetPage(index, DefaultCacheKey);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_pageCache != null)
                {
                    foreach (var p in _pageCache)
                    {
                        if (p.Value != null)
                            p.Value.Dispose();
                    }
                }
            }
            disposed = true;
        }

        ~PageFactory()
        {
            Dispose(false);
        }
    }
}
