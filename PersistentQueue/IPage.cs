using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersistentQueue
{
    internal interface IPage
    {
        long Index {get;}
        Stream GetWriteStream(long position, long length);
        Stream GetReadStream(long position, long length);
        void Dispose();
    }
}
