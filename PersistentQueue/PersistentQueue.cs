using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace PersistentQueue
{
    public class PersistentQueue : IDisposable
    {
        MetaData _meta = new MetaData();

        // MetaData
        long _headIndex;
        long _tailIndex;

        // Index pages
        static readonly long IndexItemsPerPage = 1024 * 1024;
        readonly long IndexItemSize;
        readonly long IndexPageSize;
        IPageFactory _indexPageFactory;

        // Data pages
        static readonly long DefaultDataPageSize = 128 * 1024 * 1024;
        static readonly long MinimumDataPageSize = 32 * 1024 * 1024;
        IPageFactory _dataPageFactory;


        long GetIndexPageIndex(long index)
        {
            return index / (IndexItemsPerPage + 1);
        }

        public PersistentQueue()
        {
            IndexItemSize = Marshal.SizeOf(typeof(IndexItem));
            IndexPageSize = IndexItemSize * IndexItemsPerPage;            
        }

        public void Enqueue(Stream itemData)
        {
            // Get indexPage for tailIndex
            var indexPage = _indexPageFactory.GetPage(GetIndexPageIndex(_tailIndex));

            // Get dataPageIndex and itemOffset from index
            var indexItemData = indexPage.GetBytes(0, IndexItemSize);
            IndexItem indexItem;
            using (var ms = new MemoryStream(indexItemData))
            {
                var formatter = new BinaryFormatter();
                indexItem = formatter.Deserialize(ms) as IndexItem;
            }

            // Get dataPage with dataPageIndex
            var dataPage = _dataPageFactory.GetPage(indexItem.DataPageIndex);

            // Get stream from itemOffset
            using (var writeStream = dataPage.GetWriteStream(indexItem.ItemOffset))
            {
                // Write data to write stream
                itemData.CopyTo(writeStream);
            }


            // Update index
        }

        public Stream Dequeue()
        {
            return null;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
