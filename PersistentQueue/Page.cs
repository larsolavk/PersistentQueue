using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace PersistentQueue
{
    internal class Page : IPage, IDisposable
    {
        bool disposed = false;
        string _pageFile;
        MemoryMappedFile _mmf;

        public long Index { get; private set; }

        public Page(string pageFile, long pageSize, long pageIndex)
        {
            _pageFile = pageFile;
            Index = pageIndex;
            _mmf = MemoryMappedFile.CreateFromFile(pageFile, System.IO.FileMode.OpenOrCreate, null, pageSize, MemoryMappedFileAccess.ReadWrite);
        }

        public Stream GetReadStream(long position, long length)
        {
            return _mmf.CreateViewStream(position, length, MemoryMappedFileAccess.Read);
        }

        public Stream GetWriteStream(long position, long length)
        {
            return _mmf.CreateViewStream(position, length, MemoryMappedFileAccess.Write);
        }

        public void Delete()
        {
            Dispose();

            if (File.Exists(_pageFile))
                File.Delete(_pageFile);
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
                if (_mmf != null)
                    _mmf.Dispose();
            }
            disposed = true;
        }

        ~Page()
        {
            Dispose(false);
        }
    }
}
