using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PersistentQueue.Cache
{
    internal class TTLValue
    {
        public long LastAccessTimestamp;
        public long RefCount;
    }
}
