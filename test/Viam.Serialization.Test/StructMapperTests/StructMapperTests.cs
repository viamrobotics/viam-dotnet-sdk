using System.Reflection;

namespace Viam.Serialization.Test.StructMapperTests
{
    public class StructMapperTests
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
                RequiredUShort = 3,
                UShort = 2,
                RequiredNullableUShort = 4,
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
            //var sourceDict = sourceClass.ToStruct();
            //Assert.IsType<string>(sourceDict["RequiredMyEnum"]);
            //Assert.IsType<string>(sourceDict["NullableRequiredMyEnum"]);
            //Assert.IsType<string>(sourceDict["MyEnum"]);
            //Assert.Null(sourceDict["NullableMyEnum"]);
            //Assert.IsType<short>(sourceDict["Short"]);
            //Assert.IsType<ushort>(sourceDict["UShort"]);
            //Assert.IsType<int>(sourceDict["Int"]);
            //Assert.IsType<uint>(sourceDict["UInt"]);
            //Assert.IsType<long>(sourceDict["Long"]);
            //Assert.IsType<ulong>(sourceDict["ULong"]);
            //Assert.IsType<float>(sourceDict["Float"]);
            //Assert.IsType<double>(sourceDict["Double"]);
            //Assert.IsType<string>(sourceDict["Decimal"]);
            //Assert.IsType<string>(sourceDict["String"]);
            //Assert.IsType<bool>(sourceDict["Bool"]);
            //Assert.IsType<Dictionary<string, object?>>(sourceDict["SubClass"]);
            //var sourceStruct = sourceDict.ToStruct();
            //var destDict = sourceStruct.ToDictionary();
            //var destClass = MyClass.FromDictionary(destDict);
            //CheckProperties(sourceClass, destClass);
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

    [StructMappable]
    public partial class MyClass
    {
        public MyEnum RequiredMyEnum { get; set; }
        public MyEnum? NullableRequiredMyEnum { get; set; }
        public MyEnum MyEnum { get; set; }
        public MyEnum? NullableMyEnum { get; set; }
        public short Short { get; set; }
        public short? NullableShort { get; set; }
        public ushort UShort { get; set; }
        public ushort RequiredUShort { get; set; }
        public ushort? NullableUShort { get; set; }
        public ushort? RequiredNullableUShort { get; set; }
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public uint UInt { get; set; }
        public uint? NullableUInt { get; set; }
        public long Long { get; set; }
        public long? NullableLong { get; set; }
        public ulong ULong { get; set; }
        public ulong? NullableULong { get; set; }
        public float Float { get; set; }
        public float? NullableFloat { get; set; }
        public double Double { get; set; }
        public double? NullableDouble { get; set; }
        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }
        public string String { get; set; } = string.Empty;
        public string? NullableString { get; set; }
        public bool Bool { get; set; }
        public bool? NullableBool { get; set; }
        public MySubClass SubClass { get; set; }
        public MyStruct MyStruct { get; set; } = new MyStruct();
        public MyStruct? NullableMyStruct { get; set; }
        public MyStruct RequiredMyStruct { get; set; } = new MyStruct();
        public MyStruct? RequiredMyStructNullable { get; set; } = new MyStruct();
    }

    [StructMappable]
    public partial class MySubClass
    {
        public string Name { get; set; }
    }

    [StructMappable]
    public partial struct MyStruct
    {
        public short Short { get; set; }
    }

    public enum MyEnum
    {
        Foo,
        Bar
    }
}
