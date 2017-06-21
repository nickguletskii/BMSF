namespace BMSF.Utilities.Tests
{
    using System.Linq;
    using Xunit;

    public class DataObjectExtensionsTests
    {
        [Theory]
        [InlineData(nameof(ITestInterface.PublicInterfacePropertyWithGetter))]
        [InlineData(nameof(ITestInterface.PublicInterfacePropertyWithGetterAndSetter))]
        [InlineData(nameof(ITestInterfaceInheritsOther.PublicSecondLevelInterfacePropertyWithGetter))]
        [InlineData(nameof(ITestInterfaceInheritsOther.PublicSecondLevelInterfacePropertyWithGetterAndSetter))]
        [InlineData(nameof(TestAbstractClass.PublicAbstractClassAbstractPropertyWithGetter))]
        [InlineData(nameof(TestAbstractClass.PublicAbstractClassAbstractPropertyWithGetterAndSetter))]
        [InlineData(nameof(TestAbstractClass.PublicAbstractClassPropertyWithGetter))]
        [InlineData(nameof(TestAbstractClass.PublicAbstractClassPropertyWithGetterAndSetter))]
        [InlineData(nameof(IOtherTestInterface.PublicOtherInterfacePropertyWithGetter))]
        [InlineData(nameof(IOtherTestInterface.PublicOtherInterfacePropertyWithGetterAndSetter))]
        [InlineData(nameof(TestClass.PublicClassPropertyWithGetter))]
        [InlineData(nameof(TestClass.PublicClassPropertyWithGetterAndSetter))]
        public void TestGetPublicPropertiesGivesAllPropertiesOnClass(string name)
        {
            var properties = typeof(TestClass).GetPublicProperties();
            Assert.True(properties.Any(x => x.Name == name));
        }

        [Theory]
        [InlineData(nameof(ITestInterface.PublicInterfacePropertyWithGetter))]
        [InlineData(nameof(ITestInterface.PublicInterfacePropertyWithGetterAndSetter))]
        [InlineData(nameof(ITestInterfaceInheritsOther.PublicSecondLevelInterfacePropertyWithGetter))]
        [InlineData(nameof(ITestInterfaceInheritsOther.PublicSecondLevelInterfacePropertyWithGetterAndSetter))]
        public void TestGetPublicPropertiesGivesAllPropertiesOnInterface(string name)
        {
            var properties = typeof(ITestInterfaceInheritsOther).GetPublicProperties();
            Assert.True(properties.Any(x => x.Name == name));
        }

        private interface ITestInterface
        {
            int PublicInterfacePropertyWithGetter { get; }
            int PublicInterfacePropertyWithGetterAndSetter { get; set; }
        }


        private interface IOtherTestInterface
        {
            int PublicOtherInterfacePropertyWithGetter { get; }
            int PublicOtherInterfacePropertyWithGetterAndSetter { get; set; }
        }


        private interface ITestInterfaceInheritsOther : ITestInterface
        {
            int PublicSecondLevelInterfacePropertyWithGetter { get; }
            int PublicSecondLevelInterfacePropertyWithGetterAndSetter { get; set; }
        }

        private abstract class TestAbstractClass : ITestInterfaceInheritsOther
        {
            public abstract int PublicAbstractClassAbstractPropertyWithGetter { get; }
            public abstract int PublicAbstractClassAbstractPropertyWithGetterAndSetter { get; set; }

            protected abstract int ProtectedAbstractClassAbstractPropertyWithGetter { get; }
            protected abstract int ProtectedAbstractClassAbstractPropertyWithGetterAndSetter { get; set; }

            public int PublicAbstractClassPropertyWithGetter { get; }
            public int PublicAbstractClassPropertyWithGetterAndSetter { get; set; }

            protected abstract int ProtectedAbstractClassPropertyWithGetter { get; }
            protected abstract int ProtectedAbstractClassPropertyWithGetterAndSetter { get; set; }

            private int PrivateAbstractClassPropertyWithGetter { get; }
            private int PrivateAbstractClassPropertyWithGetterAndSetter { get; set; }

            public int PublicInterfacePropertyWithGetter { get; }
            public int PublicInterfacePropertyWithGetterAndSetter { get; set; }

            public int PublicSecondLevelInterfacePropertyWithGetter { get; }
            public int PublicSecondLevelInterfacePropertyWithGetterAndSetter { get; set; }
        }

        private class TestClass : TestAbstractClass, IOtherTestInterface
        {
            public override int PublicAbstractClassAbstractPropertyWithGetter { get; }
            public override int PublicAbstractClassAbstractPropertyWithGetterAndSetter { get; set; }

            protected override int ProtectedAbstractClassAbstractPropertyWithGetter { get; }
            protected override int ProtectedAbstractClassAbstractPropertyWithGetterAndSetter { get; set; }

            protected override int ProtectedAbstractClassPropertyWithGetter { get; }
            protected override int ProtectedAbstractClassPropertyWithGetterAndSetter { get; set; }

            public int PublicClassPropertyWithGetter { get; }
            public int PublicClassPropertyWithGetterAndSetter { get; set; }

            public int PublicClassPropertyWithGetterAndPrivateSetter { get; private set; }
            public int PublicClassPropertyWithGetterAndProtectedSetter { get; protected set; }

            protected int ProtectedClassPropertyWithGetter { get; }
            protected int ProtectedClassPropertyWithGetterAndSetter { get; set; }

            private int PrivateClassPropertyWithGetter { get; }
            private int PrivateClassPropertyWithGetterAndSetter { get; set; }

            public int PublicOtherInterfacePropertyWithGetter { get; }
            public int PublicOtherInterfacePropertyWithGetterAndSetter { get; set; }
        }

        public class TestDataClass
        {
            public string Data { get; set; }
            public string Data2 { get; set; }
        }

        [Fact]
        public void TestCopyProperties()
        {
            var data = new TestDataClass
            {
                Data = "test"
            };
            var newData = new TestDataClass();
            Assert.NotEqual(data.Data, newData.Data);
            data.CopyPropertiesInto(newData);
            Assert.Equal(data.Data, newData.Data);
        }

        [Fact]
        public void TestCopyPropertiesFilter()
        {
            var data = new TestDataClass
            {
                Data = "test",
                Data2 = "test2"
            };
            var newData = new TestDataClass();
            Assert.NotEqual(data.Data, newData.Data);
            Assert.NotEqual(data.Data2, newData.Data2);
            data.CopyPropertiesInto(newData, (from, to) => from.Name == "Data");
            Assert.Equal(data.Data, newData.Data);
            Assert.Null(newData.Data2);
        }

        [Fact]
        public void TestCopyPropertiesIntoNew()
        {
            var data = new TestDataClass
            {
                Data = "test"
            };
            var newData = data.CopyPropertiesIntoNew();
            Assert.NotSame(data, newData);
            Assert.Equal(data.Data, newData.Data);
        }

        [Fact]
        public void TestGetPublicPropertiesGivesOnlyPublicProperties()
        {
            var properties = typeof(TestClass).GetPublicProperties();
            Assert.True(properties.All(x => x.GetMethod.IsPublic));
            Assert.False(properties.Any(x => x.Name.StartsWith("Private")));
            Assert.False(properties.Any(x => x.Name.StartsWith("Protected")));
        }
    }
}