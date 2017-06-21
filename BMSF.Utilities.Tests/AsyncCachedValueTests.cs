namespace BMSF.Utilities.Tests
{
    using System.Threading.Tasks;
    using Xunit;

    public class AsyncCachedValueTests
    {
        [Fact]
        public async Task TestInvalidation()
        {
            var i = 0;
            var asyncCachedValueTest = new AsyncCachedValue<int>(() => Task.FromResult(i++));
            Assert.Equal(0, await asyncCachedValueTest.Get());
            Assert.Equal(0, await asyncCachedValueTest.Get());
            asyncCachedValueTest.Invalidate();
            Assert.Equal(1, await asyncCachedValueTest.Get());
            Assert.Equal(1, await asyncCachedValueTest.Get());
        }

        [Fact]
        public async Task TestReturnsDataProducedByFactoryAsync()
        {
            var asyncCachedValueTest = new AsyncCachedValue<int>(() => Task.FromResult(20));
            Assert.Equal(20, await asyncCachedValueTest.Get());
        }

        [Fact]
        public async Task TestReturnsSameObjectProvidedByFactoryAsync()
        {
            var obj = new object();
            var asyncCachedValueTest = new AsyncCachedValue<object>(() => Task.FromResult(obj));
            Assert.Same(obj, await asyncCachedValueTest.Get());
            Assert.Same(obj, await asyncCachedValueTest.Get());
            Assert.Same(obj, await asyncCachedValueTest.Get());
        }
    }
}