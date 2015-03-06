using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace PersistentQueue
{
    internal class MetaData
    {
        public long HeadIndex { get; set; }
        public long TailIndex { get; set; }

        public void WriteToStream(Stream s)
        {
            using (var bw = new BinaryWriter(s))
            {
                bw.Write(HeadIndex);
                bw.Write(TailIndex);
            }
        }

        public static MetaData ReadFromStream(Stream s)
        {
            MetaData ret = null;
            using (var br = new BinaryReader(s))
            {
                ret = new MetaData 
                {
                    HeadIndex = br.ReadInt64(),
                    TailIndex = br.ReadInt64()
                };
            }
            return ret;
        }

        public static long Size()
        {
            return 2 * sizeof(long);
        }
    }
}
