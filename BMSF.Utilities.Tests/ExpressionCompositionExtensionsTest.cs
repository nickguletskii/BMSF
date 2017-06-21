namespace BMSF.Utilities.Tests
{
    using Xunit;

    public class ExpressionCompositionExtensionsTest
    {
        [Fact]
        public void ComposesExpressionsTest()
        {
            var composed = ExpressionCompositionExtensions.Compose<int, int, int>(x => x * 2, x => x * 3);
            Assert.Equal(30, composed.Compile().Invoke(5));
            Assert.Equal(6, composed.Compile().Invoke(1));
        }
    }
}