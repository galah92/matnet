
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MatNet
{

    internal class MatSet<T> : IEnumerable<T> where T : class
    {

        private ConcurrentDictionary<T, byte> dict = new ConcurrentDictionary<T, byte>();

        public int Count => dict.Count;

        public void Add(T item) => dict.TryAdd(item, 0);

        public void Remove(T item) => dict.TryRemove(item, out _);

        public IEnumerator GetEnumerator() => dict.Keys.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => dict.Keys.GetEnumerator();
    }

    internal class MatQueue<T> where T : class
    {

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public void Enqueue(T item) => queue.Enqueue(item);

        public bool TryDequeue(out T res) => queue.TryDequeue(out res);

        public T Dequeue()
        {
            queue.TryDequeue(out T res);
            return res;
        }

    }

}
