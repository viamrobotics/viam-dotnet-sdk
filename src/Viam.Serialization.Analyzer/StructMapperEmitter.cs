using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Text;

namespace Viam.Serialization.Analyzer
{
    public static class StructMapperEmitter
    {
        public static string Generate(INamedTypeSymbol symbol, Compilation compilation)
        {
            var symbolType = symbol.TypeKind switch
            {
                TypeKind.Class => "class",
                TypeKind.Struct => "struct",
                _ => throw new NotSupportedException($"Unsupported type kind: {symbol.TypeKind}")
            };
            var sb = new StringBuilder();
            var indent = "";

            var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : symbol.ContainingNamespace.ToDisplayString();

            var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var className = symbol.Name;

            if (namespaceName != null)
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
                indent = "    ";
            }

            // Outer class wrappers
            var containingTypes = new Stack<INamedTypeSymbol>();
            var current = symbol.ContainingType;
            while (current != null)
            {
                containingTypes.Push(current);
                current = current.ContainingType;
            }

            foreach (var type in containingTypes)
            {
                sb.AppendLine($"{indent}public partial class {type.Name}");
                sb.AppendLine($"{indent}{{");
                indent += "    ";
            }

            // Class definition
            sb.AppendLine($"{indent}public partial {symbolType} {className}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    public static {fullName} FromStruct(Google.Protobuf.WellKnownTypes.Struct s)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var result = new {fullName}();");

            foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                var fieldName = GetJsonName(prop);
                var propName = prop.Name;
                var type = prop.Type;
                var typeShape = TypeUtils.GetTypeShape(type, compilation);

                sb.AppendLine($"{indent}        if (s.Fields.TryGetValue(\"{ fieldName}\", out var {propName}_val))");
                sb.AppendLine($"{indent}        {{");

                switch (typeShape)
                {
                    case { IsString: true }:
                        sb.AppendLine($"{indent}            result.{propName} = {propName}_val.StringValue;");
                        break;
                    case { IsBoolean: true }:
                        sb.AppendLine($"{indent}            result.{propName} = {propName}_val.BoolValue;");
                        break;
                    case { IsNumeric: true, IsNullable: true }:
                        sb.AppendLine($"{indent}            result.{propName} = {propName}_val.KindCase switch");
                        sb.AppendLine($"{indent}            {{");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                        sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                        sb.AppendLine($"{indent}            }};");
                        break;
                    case { IsNumeric: true, IsNullable: false }:
                        sb.AppendLine($"{indent}            result.{propName} = ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue;");
                        break;
                    case { IsEnum: true, IsNullable: true }:
                        sb.AppendLine($"{indent}            result.{propName} = {propName}_val.KindCase switch");
                        sb.AppendLine($"{indent}            {{");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>({propName}_val.StringValue),");
                        sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                        sb.AppendLine($"{indent}            }};");
                        break;
                    case { IsEnum: true, IsNullable: false }:
                        sb.AppendLine($"{indent}            result.{propName} = {propName}_val.KindCase switch");
                        sb.AppendLine($"{indent}            {{");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                        sb.AppendLine($"{indent}                Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>({propName}_val.StringValue),");
                        sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                        sb.AppendLine($"{indent}            }};");
                        break;

                    // Handle arrays (IsArray is true)
                    case { IsArray: true, Type: IArrayTypeSymbol arrayType }:
                        // Check element type
                        var elementType = arrayType.ElementType;
                        if (elementType.SpecialType == SpecialType.System_String)
                        {
                            sb.AppendLine($"{indent}            result.{propName} = {propName}_val.ListValue.Values.Select(value => value.StringValue).ToArray();");
                        }
                        else if (elementType.SpecialType == SpecialType.System_Boolean)
                        {
                            sb.AppendLine($"{indent}            result.{propName} = {propName}_val.ListValue.Values.Select(value => value.BoolValue).ToArray();");
                        }
                        else if (elementType.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
                                 or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32
                                 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
                                 or SpecialType.System_Single or SpecialType.System_Double
                                 or SpecialType.System_Decimal)
                        {
                            sb.AppendLine(
                                $"{indent}            result.{propName} = {propName}_val.ListValue.Values.Select(value => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})value.NumberValue).ToArray();");
                        }
                        else if (elementType.TypeKind is TypeKind.Class or TypeKind.Struct)
                        {
                            sb.AppendLine($"{indent}            result.{propName} = {propName}_val.StructValues.Select(v => {elementType.Name}.FromStruct(v)).ToArray();");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}            // Unsupported array element type: {elementType.Name}");
                        }
                        break;

                    default:
                        {
                            sb.AppendLine(prop.Type.TypeKind is TypeKind.Class or TypeKind.Struct
                                ? $"{indent}            result.{propName} = {typeShape.Type.Name}.FromStruct({propName}_val.StructValue);"
                                : $"{indent}            // Unsupported type: {typeShape.Type.Name}");
                            break;
                        }
                }

                sb.AppendLine($"{indent}        }}");
            }

            sb.AppendLine($"{indent}        return result;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");

            // Close containing types
            foreach (var _ in containingTypes)
            {
                indent = indent.Substring(0, indent.Length - 4);
                sb.AppendLine($"{indent}}}");
            }

            if (namespaceName != null)
                sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GetJsonName(IPropertySymbol prop)
        {
            foreach (var attr in prop.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonPropertyNameAttribute"
                    && attr.ConstructorArguments.Length == 1
                    && attr.ConstructorArguments[0].Value is string jsonName)
                {
                    return jsonName;
                }
            }

            return prop.Name;
        }
    }
}
