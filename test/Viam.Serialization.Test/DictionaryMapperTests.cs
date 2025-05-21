using System.Reflection;
using Viam.Core.Utils;

namespace Viam.Serialization.Test
{
    public class DictionaryMapperTests
    {
        [Fact]
        public void RoundTrip()
        {
            var sourceClass = new MyClass
            {
                RequiredMyEnum = MyEnum.Foo,
                NullableRequiredMyEnum = MyEnum.Bar,
                MyEnum = MyEnum.Foo,
                NullableMyEnum = null,
                Short = 1,
                UShort = 2,
                Int = 3,
                UInt = 4,
                Long = 5,
                ULong = 6,
                Float = 7.0f,
                Double = 8.0d,
                Decimal = 9.0m,
                String = "Hello, World!",
                Bool = true,
                SubClass = new MySubClass
                {
                    Name = "SubClassName"
                },
                RequiredMyStruct = default,
                RequiredMyStructNullable = null
            };
            var sourceDict = sourceClass.ToDictionary();
            Assert.IsType<string>(sourceDict["RequiredMyEnum"]);
            Assert.IsType<string>(sourceDict["NullableRequiredMyEnum"]);
            Assert.IsType<string>(sourceDict["MyEnum"]);
            Assert.Null(sourceDict["NullableMyEnum"]);
            Assert.IsType<short>(sourceDict["Short"]);
            Assert.IsType<ushort>(sourceDict["UShort"]);
            Assert.IsType<int>(sourceDict["Int"]);
            Assert.IsType<uint>(sourceDict["UInt"]);
            Assert.IsType<long>(sourceDict["Long"]);
            Assert.IsType<ulong>(sourceDict["ULong"]);
            Assert.IsType<float>(sourceDict["Float"]);
            Assert.IsType<double>(sourceDict["Double"]);
            Assert.IsType<string>(sourceDict["Decimal"]);
            Assert.IsType<string>(sourceDict["String"]);
            Assert.IsType<bool>(sourceDict["Bool"]);
            Assert.IsType<Dictionary<string, object?>>(sourceDict["SubClass"]);
            var sourceStruct = sourceDict.ToStruct();
            var destDict = sourceStruct.ToDictionary();
            var destClass = MyClass.FromDictionary(destDict);
            CheckProperties(sourceClass, destClass);
        }

        private void CheckProperties<T>(T sourceClass, T destClass)
        {
            if (sourceClass == null || destClass == null)
                throw new ArgumentNullException("Source or destination class is null");
            foreach (var prop in sourceClass.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.PropertyType is { IsClass: true, Namespace: "Viam.Serialization.Test" })
                    CheckProperties(prop.GetValue(sourceClass), prop.GetValue(destClass));
                else
                    Assert.Equal(prop.GetValue(sourceClass), prop.GetValue(destClass));
            }
        }
    }


    [GenerateDictionaryMapper]
    public partial class MyClass
    {
        public required MyEnum RequiredMyEnum { get; init; }
        public required MyEnum? NullableRequiredMyEnum { get; init; }
        public MyEnum MyEnum { get; init; }
        public MyEnum? NullableMyEnum { get; init; }
        public short Short { get; init; }
        public short? NullableShort { get; init; }
        public ushort UShort { get; init; }
        public ushort? NullableUShort { get; init; }
        public int Int { get; init; }
        public int? NullableInt { get; init; }
        public uint UInt { get; init; }
        public uint? NullableUInt { get; init; }
        public long Long { get; init; }
        public long? NullableLong { get; init; }
        public ulong ULong { get; init; }
        public ulong? NullableULong { get; init; }
        public float Float { get; init; }
        public float? NullableFloat { get; init; }
        public double Double { get; init; }
        public double? NullableDouble { get; init; }
        public decimal Decimal { get; init; }
        public decimal? NullableDecimal { get; init; }
        public string String { get; init; } = string.Empty;
        public string? NullableString { get; init; }
        public bool Bool { get; init; }
        public bool? NullableBool { get; init; }
        public required MySubClass SubClass { get; init; }
        public MyStruct MyStruct { get; init; } = new MyStruct();
        public MyStruct? NullableMyStruct { get; init; }
        public required MyStruct RequiredMyStruct { get; init; } = new MyStruct();
        public required MyStruct? RequiredMyStructNullable { get; init; } = new MyStruct();
    }

    [GenerateDictionaryMapper]
    public partial class MySubClass
    {
        public required string Name { get; init; }
    }

    [GenerateDictionaryMapper]
    public partial struct MyStruct
    {
        public short Short { get; init; }
    }

    public enum MyEnum
    {
        Foo,
        Bar
    }
}
