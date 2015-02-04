using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PersistentQueue
{
    internal class IndexItem
    {
        public long DataPageIndex { get; set; }
        public long ItemOffset { get; set; }
        public long ItemLength { get; set; }
    }
}
