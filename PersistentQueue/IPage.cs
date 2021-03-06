﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersistentQueue
{
    internal interface IPage : IDisposable
    {
        long Index {get;}
        Stream GetWriteStream(long position, long length);
        Stream GetReadStream(long position, long length);
        void Delete();
        void DeleteFile(string filePath);
    }
}
