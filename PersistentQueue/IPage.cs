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
        byte[] GetBytes(long position, long length);
        Stream GetWriteStream(long position);
        Stream GetReadStream(long position);

    }
}
