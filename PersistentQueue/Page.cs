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
        MemoryMappedFile _mmf;

        public long Index { get; private set; }

        public Page(string pageFile, long pageSize, long pageIndex)
        {
            Index = pageIndex;
            _mmf = MemoryMappedFile.CreateFromFile(pageFile, System.IO.FileMode.OpenOrCreate, null, pageSize, MemoryMappedFileAccess.ReadWrite);
        }

        public byte[] GetBytes(long position, long length)
        {
            throw new NotImplementedException();
        }


        public Stream GetReadStream(long position, long length)
        {
            return _mmf.CreateViewStream(position, length, MemoryMappedFileAccess.Read);
        }

        public Stream GetReadStream(long position)
        {
            return _mmf.CreateViewStream(position, 0, MemoryMappedFileAccess.Read);
        }

        public Stream GetWriteStream(long position, long length)
        {
            return _mmf.CreateViewStream(position, length, MemoryMappedFileAccess.Write);
        }

        public Stream GetWriteStream(long position)
        {
            return _mmf.CreateViewStream(position, 0, MemoryMappedFileAccess.Write);
        }

        public void Dispose()
        {
            if (_mmf != null)
                _mmf.Dispose();
        }
    }
}
