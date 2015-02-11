using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PersistentQueue.Cache
{
    internal class Cache<TKey, TValue> : ICache<TKey, TValue> where TValue : IDisposable
    {
        Dictionary<TKey, TValue> _items;
        Dictionary<TKey, TTLValue> _ttlDict;
        static readonly int DefaultTtl = 10 * 1000;
        readonly int Ttl;
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public Cache() : this(DefaultTtl)
        {
        }

        public Cache(int ttlMillis)
        {
            Ttl = ttlMillis;
            _items = new Dictionary<TKey, TValue>();
            _ttlDict = new Dictionary<TKey, TTLValue>();
            Task.Factory.StartNew(() => CleanupLoop(ttlMillis / 2, _cts.Token), _cts.Token);
        }

        ~Cache()
        {
            if (_cts != null)
                _cts.Cancel();
        }

        public void Add(TKey key, TValue value)
        {
            try
            {
                _lock.EnterWriteLock();
                _items.Add(key, value);
                
                _ttlDict.Add(key, new TTLValue 
                { 
                    LastAccessTimestamp = DateTime.Now.Ticks,
                    RefCount = 1
                });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = Get(key);
            if (value != null)
                return true;

            return false;
        }

        public TValue Get(TKey key)
        {
            try
            {
                _lock.EnterReadLock();

                TTLValue ttl;
                if(_ttlDict.TryGetValue(key, out ttl))
                {
                    Interlocked.Exchange(ref ttl.LastAccessTimestamp, DateTime.Now.Ticks);
                    Interlocked.Increment(ref ttl.RefCount);
                }

                TValue item;
                if (_items.TryGetValue(key, out item))
                    return item;

                return default(TValue);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Release(TKey key)
        {
            try
            {
                _lock.EnterReadLock();

                TTLValue ttl;
                if (_ttlDict.TryGetValue(key, out ttl))
                    Interlocked.Decrement(ref ttl.RefCount);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public void Remove(TKey key)
        {
            try
            {
                _lock.EnterWriteLock();

                TValue item;
                if (_items.TryGetValue(key, out item))
                    item.Dispose();

                _items.Remove(key);
                _ttlDict.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveAll()
        {
            try
            {
                _lock.EnterWriteLock();

                foreach (var item in _items.Values.ToArray())
                {
                    if (item != null)
                        item.Dispose();
                }

                _items.Clear();
                _ttlDict.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            try
            {
                _lock.EnterReadLock();
                return _items.ContainsKey(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        void RemoveOldItems()
        {
            int count = 0;
            try
            {
                _lock.EnterUpgradeableReadLock();

                var keysToRemove = _ttlDict
                    .Where(i => i.Value != null 
                        && i.Value.RefCount <= 0 
                        && i.Value.LastAccessTimestamp + Ttl < DateTime.Now.Ticks)
                    .Select(i => i.Key)
                    .ToArray();

                if (keysToRemove.Length > 0)
                {
                    try
                    {
                        _lock.EnterWriteLock();
                        foreach (var key in keysToRemove)
                        {
                            TValue item;
                            if (_items.TryGetValue(key, out item))
                                item.Dispose();

                            _items.Remove(key);
                            _ttlDict.Remove(key);

                            count++;
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        async void CleanupLoop(int pollingMillis, CancellationToken ct)
        {
            while (true)
            {
                RemoveOldItems();
                await Task.Delay(pollingMillis);
            }
        }
    }
}
