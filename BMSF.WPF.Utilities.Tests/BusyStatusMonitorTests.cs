namespace BMSF.WPF.Utilities.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Microsoft.Reactive.Testing;
    using ReactiveUI;
    using Xunit;

    public class BusyStatusMonitorTests
    {
        [Fact]
        public async Task TestCommandSubscription()
        {
            var testScheduler = new TestScheduler();
            using (var busyStatusMonitor = new BusyStatusMonitor(testScheduler))
            {
                Assert.False(busyStatusMonitor.IsBusy);
                var command = ReactiveCommand.Create(() => { }, null, Scheduler.Immediate);
                busyStatusMonitor.AddCommand(command, "command");

                var list = new List<bool>();
                using (busyStatusMonitor.Subscribe(x => list.Add(x)))
                {
                    await command.Execute();

                    testScheduler.AdvanceBy(2);
                    Assert.Equal(true, list.Last());
                    testScheduler.AdvanceBy(1);
                    Assert.Equal(false, list.Last());
                }
            }
        }

        [Fact]
        public void TestIsBusy()
        {
            using (var busyStatusMonitor = new BusyStatusMonitor(Scheduler.Immediate))
            {
                Assert.False(busyStatusMonitor.IsBusy);
                using (busyStatusMonitor.ReportStatus(""))
                {
                    Assert.True(busyStatusMonitor.IsBusy);
                    using (busyStatusMonitor.ReportStatus(""))
                    {
                        Assert.True(busyStatusMonitor.IsBusy);
                    }
                    Assert.True(busyStatusMonitor.IsBusy);
                }
                Assert.False(busyStatusMonitor.IsBusy);
            }
        }

        [Fact]
        public void TestStatusText()
        {
            using (var busyStatusMonitor = new BusyStatusMonitor(Scheduler.Immediate))
            {
                Assert.Null(busyStatusMonitor.StatusText);
                using (busyStatusMonitor.ReportStatus("test1"))
                {
                    Assert.Equal("test1", busyStatusMonitor.StatusText);
                    using (busyStatusMonitor.ReportStatus("test2"))
                    {
                        Assert.Equal("test1\ntest2", busyStatusMonitor.StatusText);
                    }
                    Assert.Equal("test1", busyStatusMonitor.StatusText);
                }
                Assert.Null(busyStatusMonitor.StatusText);
            }
        }

        [Fact]
        public void TestSubscribe()
        {
            using (var busyStatusMonitor = new BusyStatusMonitor(Scheduler.Immediate))
            {
                var isBusy = false;
                busyStatusMonitor.Subscribe(x => { isBusy = x; });
                Assert.False(isBusy);
                using (busyStatusMonitor.ReportStatus(""))
                {
                    Assert.True(isBusy);
                    using (busyStatusMonitor.ReportStatus(""))
                    {
                        Assert.True(isBusy);
                    }
                    Assert.True(isBusy);
                }
                Assert.False(isBusy);
            }
        }
    }
}
