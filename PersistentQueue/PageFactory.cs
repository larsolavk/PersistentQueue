using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersistentQueue.Cache;

namespace PersistentQueue
{
    internal class PageFactory : IPageFactory
    {
        bool disposed = false;
        static readonly string PageFileName = "page";
        static readonly string PageFileSuffix = ".dat";
        readonly long PageSize;
        readonly string PageDir;
        Cache<string, IPage> _pageCache;

        public PageFactory(long pageSize, string pageDirectory)
        {
            PageSize = pageSize;
            PageDir = pageDirectory;

            if (!Directory.Exists(PageDir))
                Directory.CreateDirectory(PageDir);

            // A simple cache using the page filename as key.
            _pageCache = new Cache<string, IPage>(10000);
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

        public void ReleasePage(long index)
        {
            var filePath = GetFilePath(index);
            _pageCache.Release(filePath);
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
                    _pageCache.RemoveAll();
            }
            disposed = true;
        }

        ~PageFactory()
        {
            Dispose(false);
        }
    }
}
