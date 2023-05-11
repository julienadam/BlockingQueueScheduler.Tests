namespace BlockingQueueScheduler.Tests
{


    public sealed class QueueSyncContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingQueue<SendOrPostCallbackItem> _messageQueue = new BlockingQueue<SendOrPostCallbackItem>();

        public override SynchronizationContext CreateCopy() => this;

        public override void Post(SendOrPostCallback d, object state)
        {
            _messageQueue.Enqueue(new SendOrPostCallbackItem(ExecutionType.Post, d, state, null));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            using (var handle = new ManualResetEventSlim())
            {
                var callbackItem = new SendOrPostCallbackItem(ExecutionType.Send, d, state, handle);
                _messageQueue.Enqueue(callbackItem);
                handle.Wait();
                if (callbackItem.Exception != null)
                    throw callbackItem.Exception;
            }
        }

        public SendOrPostCallbackItem Receive()
        {
            var message = _messageQueue.Dequeue();
            if (message == null)
                throw new InterruptedException("Message queue was unblocked.");
            return message;
        }

        public void Unblock() => _messageQueue.Enqueue(null);

        public void Unblock(int count)
        {
            for (; count > 0; count--)
                _messageQueue.Enqueue(null);
        }

        public void Dispose() => _messageQueue.Dispose();
    }

}

