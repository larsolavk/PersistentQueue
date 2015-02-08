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
        bool disposed = false;
        // Folders
        readonly string QueuePath;
        static readonly string MetaPageFolder = "meta";
        static readonly string IndexPageFolder = "index";
        static readonly string DataPageFolder = "data";

        // MetaData
        MetaData _metaData;
        readonly long MetaDataItemSize;
        IPageFactory _metaPageFactory;

        // Tail info
        long _tailDataPageIndex;
        long _tailDataItemOffset;

        // Head info
        long _headDataPageIndex;

        // Cache keys
        static readonly string TailCacheKey = "_tail_";
        static readonly string HeadCacheKey = "_head_";

        // Index pages
        static readonly long IndexItemsPerPage = 50000;
        readonly long IndexItemSize;
        readonly long IndexPageSize;
        IPageFactory _indexPageFactory;

        // Data pages
        readonly long DataPageSize;
        static readonly long DefaultDataPageSize = 128 * 1024 * 1024;
        static readonly long MinimumDataPageSize = 32 * 1024 * 1024;
        IPageFactory _dataPageFactory;

        public PersistentQueue(string queuePath) : this(queuePath, DefaultDataPageSize) { }

        public PersistentQueue(string queuePath, long pageSize)
        {
            QueuePath = queuePath;
            DataPageSize = pageSize;

            MetaDataItemSize = MetaData.Size();
            IndexItemSize = IndexItem.Size();
            IndexPageSize = IndexItemSize * IndexItemsPerPage;

            Init();
        }

        void Init()
        {
            // Init page factories
            _metaPageFactory = new PageFactory(MetaDataItemSize, Path.Combine(QueuePath, MetaPageFolder)); // Page size = item size => only 1 item possible.
            _indexPageFactory = new PageFactory(IndexPageSize, Path.Combine(QueuePath, IndexPageFolder));
            _dataPageFactory = new PageFactory(DataPageSize, Path.Combine(QueuePath, DataPageFolder));

            InitializeMetaData();
        }

        void InitializeMetaData()
        {
            var metaPage = _metaPageFactory.GetPage(0);
            using (var readStream = metaPage.GetReadStream(0, MetaDataItemSize))
            {
                _metaData = MetaData.ReadFromStream(readStream);
            }

            // Update local data pointers from previusly persisted index item
            var prevTailIndexItem = GetPreviousIndexItem(_metaData.TailIndex, TailCacheKey);
            _tailDataPageIndex = prevTailIndexItem.DataPageIndex;
            _tailDataItemOffset = prevTailIndexItem.ItemOffset + prevTailIndexItem.ItemLength;

            var prevHeadIndexItem = GetPreviousIndexItem(_metaData.HeadIndex, HeadCacheKey);
            _headDataPageIndex = prevHeadIndexItem.DataPageIndex;
        }

        long GetIndexPageIndex(long index)
        {
            return index / IndexItemsPerPage;
        }
        long GetIndexItemOffset(long index)
        {
            return (index % IndexItemsPerPage) * IndexItemSize;
        }

        IndexItem GetPreviousIndexItem(long index, string cacheKey)
        {
            // TODO: Handle wrap situations => index == long.MaxValue
            if (index > 0)
                return GetIndexItem(index - 1, cacheKey);

            return GetIndexItem(index, cacheKey);
        }

        IndexItem GetIndexItem(long index, string cacheKey)
        {
            var indexPage = _indexPageFactory.GetPage(GetIndexPageIndex(index), cacheKey);
            var indexItemOffset = GetIndexItemOffset(index);

            using (var stream = indexPage.GetReadStream(indexItemOffset, IndexItemSize))
            {
                return IndexItem.ReadFromStream(stream);
            }
        }

        void PersistMetaData()
        {
            var metaPage = _metaPageFactory.GetPage(0);
            using (var writeStream = metaPage.GetWriteStream(0, MetaDataItemSize))
            {
                _metaData.WriteToStream(writeStream);
            }
        }

        public void Enqueue(Stream itemData)
        {
            // Throw or silently return if itemData is null?
            if (itemData == null)
                return;

            if (itemData.Length > DataPageSize)
                throw new ArgumentOutOfRangeException("Item data length is greater than queue data page size");

            if (_tailDataItemOffset + itemData.Length > DataPageSize)       // Not enough space in current page
            {
                if (_tailDataPageIndex == long.MaxValue)                    
                    _tailDataPageIndex = 0;
                else
                    _tailDataPageIndex++;

                _tailDataItemOffset = 0;
            }

            // Get data page
            var dataPage = _dataPageFactory.GetPage(_tailDataPageIndex, TailCacheKey);

            // Get write stream
            using (var writeStream = dataPage.GetWriteStream(_tailDataItemOffset, itemData.Length))
            {
                // Write data to write stream
                itemData.CopyTo(writeStream, 4*1024);
            }

            // Udate index
            // Get index page
            var indexPage = _indexPageFactory.GetPage(GetIndexPageIndex(_metaData.TailIndex), TailCacheKey);

            // Get write stream
            using (var writeStream = indexPage.GetWriteStream(GetIndexItemOffset(_metaData.TailIndex), IndexItemSize))
            {
                var indexItem = new IndexItem
                {
                    DataPageIndex = _tailDataPageIndex,
                    ItemOffset = _tailDataItemOffset,
                    ItemLength = itemData.Length
                };
                indexItem.WriteToStream(writeStream);
            }

            // Advance
            _tailDataItemOffset += itemData.Length;

            // Update meta data
            if (_metaData.TailIndex == long.MaxValue)
                _metaData.TailIndex = 0;
            else
                _metaData.TailIndex++;
            PersistMetaData();
        }

        public Stream Dequeue()
        {
            if (_metaData.HeadIndex == _metaData.TailIndex)     // Head cought up with tail. Queue is empty.
                return null;                                    // return null or Stream.Null?

            // Get index item for head index
            var indexItem = GetIndexItem(_metaData.HeadIndex, HeadCacheKey);

            // Delete previous data page if we are moving along to the next
            if (indexItem.DataPageIndex != _headDataPageIndex)
            {
                var prevPage = _dataPageFactory.GetPage(_headDataPageIndex, HeadCacheKey);
                prevPage.Delete();
                _headDataPageIndex = indexItem.DataPageIndex;
            }

            // Get data page
            var dataPage = _dataPageFactory.GetPage(indexItem.DataPageIndex, HeadCacheKey);

            // Get read stream
            MemoryStream memoryStream = new MemoryStream();
            using (var readStream = dataPage.GetReadStream(indexItem.ItemOffset, indexItem.ItemLength))
            {
                readStream.CopyTo(memoryStream, 4*1024);
                memoryStream.Position = 0;
            }

            // Update meta data
            if (_metaData.HeadIndex == long.MaxValue)
                _metaData.HeadIndex = 0;
            else
                _metaData.HeadIndex++;

            PersistMetaData();

            return memoryStream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_metaPageFactory != null)
                    _metaPageFactory.Dispose();

                if (_indexPageFactory != null)
                    _indexPageFactory.Dispose();

                if (_dataPageFactory != null)
                    _dataPageFactory.Dispose();
            }
            disposed = true;
        }

        ~PersistentQueue()
        {
            Dispose(false);
        }
    }
}
