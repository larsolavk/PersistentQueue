using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentQueue.Cache
{
    internal interface ICache<TKey, TValue> where TValue : IDisposable
    {
        void Add(TKey key, TValue value);
        bool TryGetValue(TKey key, out TValue value);
        TValue Get(TKey key);
        TValue this[TKey key] { get; set; }
        void Remove(TKey key);
        void RemoveAll();
        bool ContainsKey(TKey key);
    }
}
