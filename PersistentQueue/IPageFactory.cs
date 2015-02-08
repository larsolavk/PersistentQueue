using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentQueue
{
    internal interface IPageFactory
    {
        IPage GetPage(long index);

        IPage GetPage(long index, string cacheKey);

        void Dispose();
    }
}
