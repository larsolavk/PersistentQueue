﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace PersistentQueue
{
    internal class IndexItem
    {
        public long DataPageIndex { get; set; }
        public long ItemOffset { get; set; }
        public long ItemLength { get; set; }

        public void WriteToStream(Stream s)
        {
            using (var bw = new BinaryWriter(s))
            {
                bw.Write(DataPageIndex);
                bw.Write(ItemOffset);
                bw.Write(ItemLength);
                //bw.Flush();
            }
        }

        public static IndexItem ReadFromStream(Stream s)
        {
            IndexItem ret = null;
            using (var br = new BinaryReader(s))
            {
                ret = new IndexItem
                {
                    DataPageIndex = br.ReadInt64(),
                    ItemOffset = br.ReadInt64(),
                    ItemLength = br.ReadInt64()
                };
            }
            return ret;
        }

        public static long Size()
        {
            return 3 * sizeof(long);
        }

    }
}
