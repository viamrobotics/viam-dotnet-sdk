using Microsoft.CodeAnalysis;
using System.Linq;

namespace Viam.Serialization.Analyzer
{
    internal class TypeUtils
    {
        public static TypeShape GetTypeShape(ITypeSymbol type, Compilation compilation)
        {
            var isNullable = type.NullableAnnotation == NullableAnnotation.Annotated;

            if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments.IsEmpty: false } nullable)
                type = nullable.TypeArguments[0];

            var isEnum = type.TypeKind == TypeKind.Enum;
            var isStruct = type.TypeKind == TypeKind.Struct;

            var specialType = type.SpecialType;
            var isBuiltInValueType = specialType != SpecialType.None || type.Name switch
            {
                "DateTime" or "TimeSpan" or "Guid" when type.ContainingNamespace.ToDisplayString() == "System" => true,
                _ => false
            };

            var isUserDefinedStruct = isStruct && !isBuiltInValueType && !isEnum;

            var iEnumerableSym = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            var iDictionarySym = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");

            var named = type as INamedTypeSymbol;

            var isEnumerable = named is not null &&
                                (named.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iEnumerableSym)) ||
                                 SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, iEnumerableSym));

            var isDictionary = named is not null &&
                                (named.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iDictionarySym)) ||
                                 SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, iDictionarySym));

            var isNumeric = type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
                or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32
                or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 
                or SpecialType.System_Decimal;

            return new TypeShape(
                IsNullable: isNullable,
                IsEnum: isEnum,
                IsString: type.SpecialType == SpecialType.System_String,
                IsNumeric: isNumeric,
                IsBoolean: type.SpecialType is SpecialType.System_Boolean,
                IsUserDefinedStruct: isUserDefinedStruct,
                IsBuiltInValueType: isBuiltInValueType,
                IsDictionary: isDictionary,
                IsEnumerable: isEnumerable,
                IsArray: type is IArrayTypeSymbol,
                TypeKind: type.TypeKind,
                Type: type
            );
        }


        public readonly record struct TypeShape(
            bool IsNullable,
            bool IsEnum,
            bool IsString,
            bool IsNumeric,
            bool IsBoolean,
            bool IsUserDefinedStruct,
            bool IsBuiltInValueType,
            bool IsDictionary,
            bool IsEnumerable,
            bool IsArray,
            TypeKind TypeKind,
            ITypeSymbol Type
        );
    }
}
