using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentQueue
{
    internal interface IPageFactory : IDisposable
    {
        IPage GetPage(long index);
        void ReleasePage(long index);
        void DeletePage(long index);
    }
}
