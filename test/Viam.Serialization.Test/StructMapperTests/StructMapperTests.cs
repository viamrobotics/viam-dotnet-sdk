using System.Reflection;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Viam.Core.Utils;
using Xunit.Abstractions;

namespace Viam.Serialization.Test.StructMapperTests
{
    public class StructMapperTests(ITestOutputHelper output)
    {
        private void foo(Google.Protobuf.WellKnownTypes.Struct s)
        {
            if (s.Fields.TryGetValue("StringArray", out var StringArray_val))
            {
                StringArray_val.ListValue.Values.Select(x => x.StringValue);
            }
        }
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
            var s = sourceClass.ToStruct();
            //Assert.IsType<string>(s.Fields["RequiredMyEnum"]);
            //Assert.IsType<string>(s.Fields["NullableRequiredMyEnum"]);
            //Assert.IsType<string>(s.Fields["MyEnum"]);
            //Assert.Null(s.Fields["NullableMyEnum"]);
            //Assert.IsType<short>(s.Fields["Short"]);
            //Assert.IsType<ushort>(s.Fields["UShort"]);
            //Assert.IsType<int>(s.Fields["Int"]);
            //Assert.IsType<uint>(s.Fields["UInt"]);
            //Assert.IsType<long>(s.Fields["Long"]);
            //Assert.IsType<ulong>(s.Fields["ULong"]);
            //Assert.IsType<float>(s.Fields["Float"]);
            //Assert.IsType<double>(s.Fields["Double"]);
            //Assert.IsType<string>(s.Fields["Decimal"]);
            //Assert.IsType<string>(s.Fields["String"]);
            //Assert.IsType<bool>(s.Fields["Bool"]);
            //Assert.IsType<Dictionary<string, object?>>(s.Fields["SubClass"]);
            var sourceStruct = MyClass.FromProto(s);
            CheckProperties(sourceClass, sourceStruct);
        }

        private void CheckProperties<T>(T sourceClass, T destClass)
        {
            if (sourceClass == null || destClass == null)
                throw new ArgumentNullException("Source or destination class is null");
            foreach (var prop in sourceClass.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.PropertyType is { IsClass: true, Namespace: "Viam.Serialization.Test.StructMapperTests" })
                    CheckProperties(prop.GetValue(sourceClass), prop.GetValue(destClass));
                else
                {
                    output.WriteLine("Comparing {0}, expect: {1}, have: {2}", prop.Name, prop.GetValue(sourceClass) ?? "null", prop.GetValue(destClass) ?? "null");
                    Assert.Equal(prop.GetValue(sourceClass), prop.GetValue(destClass));
                }
            }
        }
    }

    [StructMappable]
    public partial class MyClass
    {
        public string[] StringArray { get; set; } = [];
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
