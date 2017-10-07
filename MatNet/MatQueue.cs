using System;
using System.Collections.Concurrent;

namespace MatNet
{

    public class MatQueue<T> where T : class
    {

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public int Count => queue.Count;

        public bool IsEmpty => queue.IsEmpty;

        public Action EnqueueAction;

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
            EnqueueAction?.Invoke();
        }

        public bool TryDequeue(out T res) => queue.TryDequeue(out res);

        public T Dequeue()
        {
            T res = null;
            TryDequeue(out res);
            return res;
        }

    }

}