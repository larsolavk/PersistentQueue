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
        readonly long PageSize;
        readonly string PageDir;

        Dictionary<string, IPage> _pageCache;

        // TODO: Remove pages from cache... When? How? Another cleaner thread/task?
        public PageFactory(long pageSize, string pageDirectory)
        {
            PageSize = pageSize;
            PageDir = pageDirectory;

            if (!Directory.Exists(PageDir))
                Directory.CreateDirectory(PageDir);

            // A simple cache using the page filename as key.
            _pageCache = new Dictionary<string, IPage>();
        }

        string GetFilePath(long index)
        {
            return Path.Combine(PageDir, String.Format("{0}-{1}{2}", PageFileName, index, PageFileSuffix));
        }

        public IPage GetPage(long index)
        {
            var filePath = GetFilePath(index);
            IPage page;

            if (!_pageCache.TryGetValue(filePath, out page))
            {
                page = _pageCache[filePath] = new Page(filePath, PageSize, index);
            }
            
            return page;
        }

        public void DeletePage(long index)
        {
            IPage page;
            var filePath = GetFilePath(index);
            
            // Lookup page in _pageCache.
            if (_pageCache.TryGetValue(filePath, out page))
            {
                // delete and remove from cache
                page.Delete();
                _pageCache.Remove(filePath);
            }
            else
            {
                // If not found in cache, delete the file directly.
                Page.DeleteFile(filePath);
            }
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
