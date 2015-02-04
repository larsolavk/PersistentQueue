using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentQueue
{
    internal class MetaData
    {
        public long HeadIndex { get; set; }
        public long TailIndex { get; set; }
    }
}
