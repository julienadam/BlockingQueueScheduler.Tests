namespace BlockingQueueScheduler.Tests
{
    public enum ExecutionType
    {
        Send,
        Post
    }

    public class SendOrPostCallbackItem
    {
        public SendOrPostCallbackItem(ExecutionType executionType, SendOrPostCallback callback, object state, ManualResetEventSlim signalComplete)
        {
            ExecutionType = executionType;
            Callback = callback;
            State = state;
            SignalComplete = signalComplete;
        }

        private ExecutionType ExecutionType { get; }
        private SendOrPostCallback Callback { get; }
        private object State { get; }
        private ManualResetEventSlim SignalComplete { get; }
        public Exception Exception { get; private set; }

        public void Execute()
        {
            switch (ExecutionType)
            {
                case ExecutionType.Post:
                    Callback(State);
                    break;
                case ExecutionType.Send:
                    try
                    {
                        Callback(State);
                    }
                    catch (Exception e)
                    {
                        Exception = e;
                    }
                    SignalComplete.Set();
                    break;
                default:
                    throw new ArgumentException($"{nameof(ExecutionType)} is not a valid value.");
            }
        }
    }

}

