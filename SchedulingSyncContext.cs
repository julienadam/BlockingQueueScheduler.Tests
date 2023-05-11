namespace BlockingQueueScheduler.Tests
{
    public class SchedulingSyncContext : IDisposable
    {
        private Thread[] _threads;
        public SynchronizationContext SynchronizationContext => _disposed ? throw new ObjectDisposedException(typeof(SchedulingSyncContext).FullName) : _queueSyncContext;
        private QueueSyncContext _queueSyncContext;
        private bool _disposed;
        private readonly object _lock = new object();

        public SchedulingSyncContext(int threadCount = 1, bool isBackground = false)
        {
            if (threadCount < 1)
                throw new ArgumentOutOfRangeException(nameof(threadCount));

            _queueSyncContext = new QueueSyncContext();
            _threads = new Thread[threadCount];
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(ExecuteCallbacks)
                {
                    IsBackground = isBackground
                };
                _threads[i].Start();
            }
        }

        private void ExecuteCallbacks()
        {

            while (true)
            {
                SendOrPostCallbackItem callback;
                try
                {
                    callback = _queueSyncContext.Receive();
                }
                catch (InterruptedException)
                {
                    return;
                }
                callback.Execute();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            _queueSyncContext.Unblock(_threads.Length);
            foreach (var thread in _threads)
                thread.Join();
            _queueSyncContext.Dispose();
        }
    }

}

