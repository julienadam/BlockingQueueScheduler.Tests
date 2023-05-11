using System.Text;
using Xunit.Abstractions;

namespace BlockingQueueScheduler.Tests
{
    public class SchedulerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SchedulerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SimpleTest()
        {
            var scheduler = new SchedulingSyncContext();
            SynchronizationContext.SetSynchronizationContext(scheduler.SynchronizationContext);
            var builder = new StringBuilder();
            WriteTest();
            Thread.Sleep(1000);
            _testOutputHelper.WriteLine(builder.ToString());
            scheduler.Dispose();

            async void WriteTest()
            {
                var t1 = Write1();
                var t2 = Write2();
                builder.AppendLine("WriteTest");
                await Task.WhenAll(t1, t2);
                await Write3();
            }
            async Task Write1()
            {
                // Yield the thread (runs the remainder of the async code as a callback on the SchedulingSyncContext thread)
                await Task.Yield();
                builder.AppendLine("Test1");
            }
            async Task Write2()
            {
                // Wait for 10 milliseconds before continuing (uses callbacks)
                await Task.Delay(10);
                builder.AppendLine("Test2");
            }
            async Task Write3()
            {
                // Run these two tasks on the thread pool (could come out in any order)
                var tA = Task.Run(() => builder.AppendLine("Write3 - 1"));
                var tB = Task.Run(() => builder.AppendLine("Write3 - 2"));
                // Wait for them to finish before continuing (uses callbacks)
                await Task.WhenAll(tA, tB);
            }
        }
    }

}

