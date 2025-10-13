using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Text;

namespace Viam.Serialization.Analyzer
{
    public static class StructMapperEmitter
    {
        private static readonly SymbolDisplayFormat FQ =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );

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

            var implementsAbstractBaseClass = symbol.BaseType?.ToDisplayString(FQ) != "global::System.ValueType" && (symbol.BaseType?.IsAbstract ?? false);

            var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : symbol.ContainingNamespace.ToDisplayString();

            var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var className = symbol.Name;

            sb.AppendLine($"{indent}#nullable enable");

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
            sb.AppendLine($"{indent}public partial {symbolType} {className} : global::Viam.Serialization.IStructMappable<{className}>");
            sb.AppendLine($"{indent}{{");

            // Generate FromProto(Struct) method
            sb.AppendLine($"{indent}    public{(implementsAbstractBaseClass ? " new" : "")} static {fullName} FromProto(global::Google.Protobuf.WellKnownTypes.Struct s)");
            sb.AppendLine($"{indent}    {{");

            if (symbol.IsAbstract)
            {
                sb.AppendLine(
                    $"{indent}        throw new InvalidOperationException(\"cannot deserialize abstract type\");");
            }
            else
            {
                foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var fieldName = GetJsonName(prop);
                    var propName = prop.Name;
                    var type = prop.Type;
                    var typeShape = TypeUtils.GetTypeShape(type, compilation);

                    sb.AppendLine($"{indent}            var {propName} = s.Fields.TryGetValue(\"{fieldName}\", out var {propName}_val)");

                    switch (typeShape)
                    {
                        case { IsString: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? {propName}_val.StringValue : default;");
                            break;
                        case { IsString: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.HasNullValue == false");
                            sb.AppendLine($"{indent}                    ? {propName}_val.StringValue");
                            sb.AppendLine($"{indent}                    : (string?)null");
                            sb.AppendLine($"{indent}                : (string?)null;");
                            break;
                        case { IsBoolean: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? {propName}_val.BoolValue : default;");
                            break;
                        case { IsBoolean: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.HasNullValue == false");
                            sb.AppendLine($"{indent}                    ? (bool?){propName}_val.BoolValue");
                            sb.AppendLine($"{indent}                    : (bool?)null");
                            sb.AppendLine($"{indent}                : (bool?)null;");
                            break;
                        case { IsNumeric: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.KindCase switch");
                            sb.AppendLine($"{indent}                {{");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                            sb.AppendLine($"{indent}                    _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}                }} : default;");
                            break;
                        case { IsNumeric: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue : default;");
                            break;
                        case { IsEnum: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.KindCase switch");
                            sb.AppendLine($"{indent}                {{");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>({propName}_val.StringValue),");
                            sb.AppendLine($"{indent}                    _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}                }} : default;");
                            break;
                        case { IsEnum: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? {propName}_val.KindCase switch");
                            sb.AppendLine($"{indent}                {{");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}){propName}_val.NumberValue,");
                            sb.AppendLine($"{indent}                    global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>({propName}_val.StringValue),");
                            sb.AppendLine($"{indent}                    _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}                }} : default;");
                            break;
                        case { IsDictionary: true }:
                            var named = (INamedTypeSymbol)typeShape.Type;
                            var dictKeyType = named.TypeArguments[0];
                            var dictValueType = named.TypeArguments[1];
                            sb.AppendLine($"{indent}                ? {propName}_val.StructValue.Fields.ToDictionary(k => k.Key, v => ({dictValueType.ToDisplayString(FQ)}){dictValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto(v.Value.StructValue)) : [];");

                            break;
                        case { IsClass: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? {typeShape.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto({propName}_val.StructValue)");
                            sb.AppendLine($"{indent}                : default;");
                            break;
                        case { IsClass: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.HasNullValue");
                            sb.AppendLine($"{indent}                    ? ({type.ToDisplayString(FQ)})null");
                            sb.AppendLine($"{indent}                    : ({type.ToDisplayString(FQ)}){typeShape.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto({propName}_val.StructValue)");
                            sb.AppendLine($"{indent}                : default;");
                            break;
                        case { IsStruct: true, IsNullable: false }:
                            sb.AppendLine($"{indent}                ? {typeShape.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto({propName}_val.StructValue)");
                            sb.AppendLine($"{indent}                : default;");
                            break;
                        case { IsStruct: true, IsNullable: true }:
                            sb.AppendLine($"{indent}                ? {propName}_val.HasNullValue");
                            sb.AppendLine($"{indent}                    ? ({type.ToDisplayString(FQ)})null");
                            sb.AppendLine($"{indent}                    : ({type.ToDisplayString(FQ)}){typeShape.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto({propName}_val.StructValue)");
                            sb.AppendLine($"{indent}                : default;");
                            break;
                        // Handle arrays (IsArray is true)
                        case { IsArray: true, Type: IArrayTypeSymbol arrayType }:
                            // Check element type
                            var elementType = arrayType.ElementType;
                            if (elementType.SpecialType == SpecialType.System_String)
                            {
                                sb.AppendLine($"{indent}            ? {propName}_val.ListValue.Values.Select(value => value.StringValue).ToArray() : default;");
                            }
                            else if (elementType.SpecialType == SpecialType.System_Boolean)
                            {
                                sb.AppendLine($"{indent}            ? {propName}_val.ListValue.Values.Select(value => value.BoolValue).ToArray() : default;");
                            }
                            else if (elementType.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
                                     or SpecialType.System_Int16 or SpecialType.System_UInt16
                                     or SpecialType.System_Int32
                                     or SpecialType.System_UInt32 or SpecialType.System_Int64
                                     or SpecialType.System_UInt64
                                     or SpecialType.System_Single or SpecialType.System_Double
                                     or SpecialType.System_Decimal)
                            {
                                sb.AppendLine($"{indent}            ? {propName}_val.ListValue.Values.Select(value => ({elementType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})value.NumberValue).ToArray() : default;");
                            }
                            else if (elementType.TypeKind is TypeKind.Class or TypeKind.Struct)
                            {
                                sb.AppendLine($"{indent}            ? {propName}_val.ListValue.Values.Select(v => {elementType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto(v.StructValue)).ToArray() : default;");
                            }
                            else
                            {
                                sb.AppendLine($"{indent}            // Unsupported array element type: {elementType.Name}");
                            }

                            break;
                        default:
                            {
                                sb.AppendLine($"{indent}                // Unsupported type: {typeShape.Type.Name}");
                                break;
                            }
                    }
                }

                sb.AppendLine($"{indent}        var result = new {fullName}");
                sb.AppendLine($"{indent}        {{");
                foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var fieldName = GetJsonName(prop);
                    var propName = prop.Name;
                    sb.AppendLine($"{indent}            {propName} = {propName},");
                }

                sb.AppendLine($"{indent}        }};");
                sb.AppendLine($"{indent}        return result;");
            }

            sb.AppendLine($"{indent}    }}");

            sb.AppendLine($"");

            // Generate FromProto(MapField<string, Value> method
            sb.AppendLine($"{indent}    public{(implementsAbstractBaseClass ? " new" : "")} static {fullName} FromProto(global::Google.Protobuf.Collections.MapField<string, global::Google.Protobuf.WellKnownTypes.Value> s)");
            sb.AppendLine($"{indent}    {{");
            if (symbol.IsAbstract)
            {
                sb.AppendLine($"{indent}        throw new InvalidOperationException(\"cannot deserialize abstract type\");");
            }
            else
            {
                sb.AppendLine($"{indent}        var result = new {fullName}()");
                sb.AppendLine($"{indent}        {{");
                foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var fieldName = GetJsonName(prop);
                    var propName = prop.Name;
                    var type = prop.Type;
                    var typeShape = TypeUtils.GetTypeShape(type, compilation);
                    switch (typeShape)
                    {
                        case { IsString: true }:
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].StringValue,");
                            break;
                        case { IsBoolean: true }:
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].BoolValue,");
                            break;
                        case { IsNumeric: true, IsNullable: true }:
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].KindCase switch");
                            sb.AppendLine($"{indent}            {{");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})s[\"{fieldName}\"].NumberValue,");
                            sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}            }},");
                            break;
                        case { IsNumeric: true, IsNullable: false }:
                            sb.AppendLine($"{indent}            {propName} = ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})s[\"{fieldName}\"].NumberValue,");
                            break;
                        case { IsEnum: true, IsNullable: true }:
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].KindCase switch");
                            sb.AppendLine($"{indent}            {{");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}?)null,");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})s[\"{fieldName}\"].NumberValue,");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>(s[\"{fieldName}\"].StringValue),");
                            sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}            }},");
                            break;
                        case { IsEnum: true, IsNullable: false }:
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].KindCase switch");
                            sb.AppendLine($"{indent}            {{");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})s[\"{fieldName}\"].NumberValue,");
                            sb.AppendLine($"{indent}                global::Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue => ({typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)})Enum.Parse<{typeShape.Type.WithNullableAnnotation(NullableAnnotation.None)}>(s[\"{fieldName}\"].StringValue),");
                            sb.AppendLine($"{indent}                _ => throw new InvalidOperationException(\"Unexpected kind for NullableShort\")");
                            sb.AppendLine($"{indent}            }},");
                            break;
                        case { IsDictionary: true }:
                            var dictKeyType = ((INamedTypeSymbol)typeShape.Type).TypeArguments[0];
                            var dictValueType = ((INamedTypeSymbol)typeShape.Type).TypeArguments[1];
                            sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].StructValue.Fields.ToDictionary(k => k.Key, v => ({dictValueType.ToDisplayString(FQ)}){dictValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto(v.Value.StructValue)),");

                            break;
                        // Handle arrays (IsArray is true)
                        case { IsArray: true, Type: IArrayTypeSymbol arrayType }:
                            // Check element type
                            var elementType = arrayType.ElementType;
                            if (elementType.SpecialType == SpecialType.System_String)
                            {
                                sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].ListValue.Values.Select(v => v.StringValue).ToArray(),");
                            }
                            else if (elementType.SpecialType == SpecialType.System_Boolean)
                            {
                                sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].ListValue.Values.Select(v => v.BoolValue).ToArray(),");
                            }
                            else if (elementType.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
                                     or SpecialType.System_Int16 or SpecialType.System_UInt16
                                     or SpecialType.System_Int32
                                     or SpecialType.System_UInt32 or SpecialType.System_Int64
                                     or SpecialType.System_UInt64
                                     or SpecialType.System_Single or SpecialType.System_Double
                                     or SpecialType.System_Decimal)
                            {
                                sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].ListValue.Values.Select(v => ({elementType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})v.NumberValue).ToArray()");
                            }
                            else if (elementType.TypeKind is TypeKind.Class or TypeKind.Struct)
                            {
                                sb.AppendLine($"{indent}            {propName} = s[\"{fieldName}\"].ListValue.Values.Select(v => {elementType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto(v.StructValue)).ToArray(),");
                            }
                            else
                            {
                                sb.AppendLine($"{indent}            // Unsupported array element type: {elementType.Name}");
                            }

                            break;

                        default:
                            {
                                sb.AppendLine(prop.Type.TypeKind is TypeKind.Class or TypeKind.Struct
                                    ? $"{indent}            {propName} = {typeShape.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.FromProto(s[\"{fieldName}\"].StructValue),"
                                    : $"{indent}            // Unsupported type: {typeShape.Type.Name}");
                                break;
                            }
                    }
                }

                sb.AppendLine($"{indent}        }};");
                sb.AppendLine($"{indent}        return result;");
            }

            sb.AppendLine($"{indent}    }}");

            sb.AppendLine($"");

            // Generate ToStruct method
            sb.AppendLine($"{indent}    public{(implementsAbstractBaseClass ? " new" : "")} global::Google.Protobuf.WellKnownTypes.Struct ToStruct()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var s = new global::Google.Protobuf.WellKnownTypes.Struct();");
            foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                GenerateToStructFieldSerializer(prop, compilation, sb, indent);
            }
            sb.AppendLine($"{indent}        return s;");
            sb.AppendLine($"{indent}    }}");

            sb.AppendLine($"");

            // Generate ToMapField method
            sb.AppendLine($"{indent}    public{(implementsAbstractBaseClass ? " new" : "")} global::Google.Protobuf.Collections.MapField<string, global::Google.Protobuf.WellKnownTypes.Value> ToMapField()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var s = new global::Google.Protobuf.Collections.MapField<string, global::Google.Protobuf.WellKnownTypes.Value>();");
            foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                GenerateToMapFieldFieldSerializer(prop, compilation, sb, indent);
            }
            sb.AppendLine($"{indent}        return s;");
            sb.AppendLine($"{indent}    }}");

            sb.AppendLine($"");

            // Generate implicit conversion operators
            sb.AppendLine($"{indent}    public static implicit operator global::Google.Protobuf.WellKnownTypes.Struct({fullName}? value) => value?.ToStruct() ?? new global::Google.Protobuf.WellKnownTypes.Struct();");
            sb.AppendLine($"");
            sb.AppendLine($"{indent}    public static implicit operator global::Google.Protobuf.Collections.MapField<string, global::Google.Protobuf.WellKnownTypes.Value>({fullName} value) => value.ToMapField();");
            sb.AppendLine($"");
            sb.AppendLine($"{indent}    public static implicit operator {fullName}(global::Google.Protobuf.WellKnownTypes.Struct value) => {fullName}.FromProto(value);");
            sb.AppendLine($"");
            sb.AppendLine($"{indent}    public static implicit operator {fullName}(global::Google.Protobuf.Collections.MapField<string, global::Google.Protobuf.WellKnownTypes.Value> value) => {fullName}.FromProto(value);");

            sb.AppendLine($"{indent}}}");


            // Close containing types
            foreach (var _ in containingTypes)
            {
                indent = indent.Substring(0, indent.Length - 4);
                sb.AppendLine($"{indent}}}");
            }

            if (namespaceName != null)
                sb.AppendLine("}");

            sb.AppendLine($"{indent}#nullable restore");
            return sb.ToString();
        }

        private static void GenerateToStructFieldSerializer(IPropertySymbol prop, Compilation compilation, StringBuilder sb, string indent)
        {
            var fieldName = GetJsonName(prop);
            var propName = prop.Name;
            var type = prop.Type;
            var typeShape = TypeUtils.GetTypeShape(type, compilation);
            switch (typeShape)
            {
                case { IsString: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName} ?? string.Empty);");
                    break;
                case { IsString: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName});");
                    break;
                case { IsBoolean: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForBool(this.{propName});");
                    break;
                case { IsBoolean: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForBool(this.{propName}.Value);");
                    break;
                case { IsNumeric: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForNumber((double)this.{propName});");
                    break;
                case { IsNumeric: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForNumber((double)this.{propName});");
                    break;
                case { IsEnum: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName}.ToString());");
                    break;
                case { IsEnum: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName}.ToString());");
                    break;
                case { IsClass: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}.ToStruct());");
                    break;
                case { IsClass: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null  ");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}?.ToStruct());");
                    break;
                case { IsStruct: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}.ToStruct());");
                    break;
                case { IsStruct: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = this.{propName} is null  ");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}?.ToStruct());");
                    break;
                // Handle arrays (IsArray is true)
                case { IsArray: true, Type: IArrayTypeSymbol arrayType }:
                    // Check element type
                    var elementType = arrayType.ElementType;
                    switch (elementType.SpecialType)
                    {
                        case SpecialType.System_String:
                            sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForString(v ?? string.Empty)).ToArray());");
                            break;
                        case SpecialType.System_Boolean:
                            sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForBool(v)).ToArray() ?? [];");
                            break;
                        case SpecialType.System_Byte or SpecialType.System_SByte
                            or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32
                            or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
                            or SpecialType.System_Single or SpecialType.System_Double
                            or SpecialType.System_Decimal:
                            sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForNumber(v)).ToArray() ?? [];");
                            break;
                        default:
                            if (elementType.TypeKind is TypeKind.Class or TypeKind.Struct)
                            {
                                sb.AppendLine($"{indent}        s.Fields[\"{fieldName}\"] = global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForStruct(v.ToStruct())).ToArray());");
                            }

                            break;
                    }
                    break;
                default:
                {
                    sb.AppendLine($"{indent}        // Unsupported type: {typeShape.Type.Name}");
                    break;
                }
            }
        }

        private static void GenerateToMapFieldFieldSerializer(IPropertySymbol prop, Compilation compilation, StringBuilder sb, string indent)
        {
            var fieldName = GetJsonName(prop);
            var propName = prop.Name;
            var type = prop.Type;
            var typeShape = TypeUtils.GetTypeShape(type, compilation);
            switch (typeShape)
            {
                case { IsString: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName} ?? string.Empty));");
                    break;
                case { IsString: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName}));");
                    break;
                case { IsBoolean: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForBool(this.{propName}));");
                    break;
                case { IsBoolean: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForBool(this.{propName}.Value));");
                    break;
                case { IsNumeric: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForNumber((double)this.{propName}));");
                    break;
                case { IsNumeric: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForNumber((double)this.{propName}));");
                    break;
                case { IsEnum: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName}.ToString()));");
                    break;
                case { IsEnum: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForString(this.{propName}.ToString()));");
                    break;
                case { IsClass: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}.ToStruct()));");
                    break;
                case { IsClass: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}?.ToStruct()));");
                    break;
                case { IsStruct: true, IsNullable: false }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}.ToStruct()));");
                    break;
                case { IsStruct: true, IsNullable: true }:
                    sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", this.{propName} is null");
                    sb.AppendLine($"{indent}            ? global::Google.Protobuf.WellKnownTypes.Value.ForNull()");
                    sb.AppendLine($"{indent}            : global::Google.Protobuf.WellKnownTypes.Value.ForStruct(this.{propName}?.ToStruct()));");
                    break;
                // Handle arrays (IsArray is true)
                case { IsArray: true, Type: IArrayTypeSymbol arrayType }:
                    // Check element type
                    var elementType = arrayType.ElementType;
                    switch (elementType.SpecialType)
                    {
                        case SpecialType.System_String:

                            sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForString(v ?? string.Empty)).ToArray() ?? []));");
                            break;
                        case SpecialType.System_Boolean:
                            sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForBool(v)).ToArray() ?? []));");
                            break;
                        case SpecialType.System_Byte or SpecialType.System_SByte
                            or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32
                            or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
                            or SpecialType.System_Single or SpecialType.System_Double
                            or SpecialType.System_Decimal:
                            sb.AppendLine($"{indent}        s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForNumber(v)).ToArray() ?? []));");
                            break;
                        default:
                            if (elementType.TypeKind is TypeKind.Class or TypeKind.Struct)
                            {
                                sb.AppendLine($"{indent}       s.Add(\"{fieldName}\", global::Google.Protobuf.WellKnownTypes.Value.ForList(this.{propName}?.Select(v => global::Google.Protobuf.WellKnownTypes.Value.ForStruct(v.ToStruct())).ToArray()));");
                            }
                            break;
                    }
                    break;
                default:
                {
                    sb.AppendLine($"{indent}        // Unsupported type: {typeShape.Type.Name}");
                    break;
                }
            }
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
