using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace PersistentQueue
{
    internal class Page : IPage
    {
        MemoryMappedFile _mmf;


        public Page(string pageFile)
        {
            _mmf = MemoryMappedFile.CreateFromFile(pageFile, System.IO.FileMode.OpenOrCreate, null, 10, MemoryMappedFileAccess.ReadWrite);
        }

        public byte[] GetBytes(long position, long length)
        {
            throw new NotImplementedException();
        }

        public Stream GetReadStream(long position)
        {
            return _mmf.CreateViewStream(position, 0, MemoryMappedFileAccess.Read);
        }

        public Stream GetWriteStream(long position)
        {
            return _mmf.CreateViewStream(position, 0, MemoryMappedFileAccess.Write);
        }
    }
}
