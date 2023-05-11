using System.Collections;

namespace BlockingQueueScheduler.Tests
{
    public sealed class BlockingQueue<T> : IDisposable, IEnumerable<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly Semaphore _pool = new Semaphore(0, int.MaxValue);
        private readonly object _lock = new object();

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
                _pool.Release();
            }
        }

        public T Dequeue()
        {
            _pool.WaitOne();
            lock (_lock)
            {
                return _queue.Dequeue();
            }
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_queue).GetEnumerator();
        }
    }

}

