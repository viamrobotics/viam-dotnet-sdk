using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viam.Serialization.Analyzer
{
    [Generator]
    public class DictionaryMapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => HasRequiredAttribute(node),
                    transform: static (context, _) => GetSymbol(context))
                .Where(static symbol => symbol is not null)
                .Collect()
                .Combine(context.CompilationProvider);


            context.RegisterSourceOutput(classDeclarations, (ctx, allSymbols) =>
            {
                var (classSymbols, compilation) = allSymbols;
                var validSymbols = classSymbols.OfType<INamedTypeSymbol>().Where(s => !s.IsAbstract);

                foreach (var symbol in validSymbols)
                {
                    var code = GenerateMapperCode(symbol, compilation);
                    ctx.AddSource($"{symbol.Name}_DictionaryMapper.g.cs", SourceText.From(code, Encoding.UTF8));
                }

                var polySymbols = classSymbols.OfType<INamedTypeSymbol>()
                    .Where(s => s.TypeKind == TypeKind.Class)
                    .Where(s => s.BaseType is { IsAbstract: true })
                    .GroupBy(s => s.BaseType!, SymbolEqualityComparer.Default);

                foreach (var group in polySymbols)
                {
                    if (group.Key == null) continue;
                    if (group.Key is not INamedTypeSymbol key)
                        throw new InvalidOperationException($"Base type '{group.Key.Name}' is not a valid named type symbol.");
                    var code = GeneratePolymorphicDispatcher(key, group);
                    ctx.AddSource($"{key.Name}_DictionaryMapper.g.cs", SourceText.From(code, Encoding.UTF8));
                }
            });
        }

        private static bool HasRequiredAttribute(SyntaxNode node)
        {
            switch (node)
            {
                case StructDeclarationSyntax structDecl:
                    {
                        var hasAttribute = structDecl.AttributeLists
                            .SelectMany(attrList => attrList.Attributes)
                            .Any(attr => attr.Name.ToString() == "GenerateDictionaryMapper");

                        if (!hasAttribute) return false;

                        if (!structDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                            throw new InvalidOperationException(
                                $"Class '{structDecl.Identifier.Text}' must be marked 'partial' to allow source generation.");

                        return true;
                    }
                case ClassDeclarationSyntax classDecl:
                    {
                        var hasAttribute = classDecl.AttributeLists
                            .SelectMany(attrList => attrList.Attributes)
                            .Any(attr => attr.Name.ToString() == "GenerateDictionaryMapper");

                        if (!hasAttribute) return false;

                        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                            throw new InvalidOperationException(
                                $"Class '{classDecl.Identifier.Text}' must be marked 'partial' to allow source generation.");

                        return true;
                    }
                default:
                    return false;
            }
        }

        private static INamedTypeSymbol? GetSymbol(GeneratorSyntaxContext context) => context.Node switch
        {
            ClassDeclarationSyntax classDecl => context.SemanticModel.GetDeclaredSymbol(classDecl),
            StructDeclarationSyntax structDecl => context.SemanticModel.GetDeclaredSymbol(structDecl),
            _ => null
        };

        private static string SanitizeName(string name) =>
            name.Replace(" ", "_").Replace("-", "_")
                .Replace("[", "_").Replace("]", "_")
                .Replace("<", "_").Replace(">", "_")
                .Replace("?", "_").Replace(":", "_")
                .Replace(",", "_").Replace(".", "_");

        private static string GenerateMapperCode(INamedTypeSymbol symbol, Compilation compilation)
        {
            var symbolName = symbol.Name;
            var symbolType = symbol.TypeKind switch
            {
                TypeKind.Class => "class",
                TypeKind.Struct => "struct",
                _ => throw new NotSupportedException($"Unsupported type kind: {symbol.TypeKind}")
            };
            var accessModifier = symbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.NotApplicable => "",
                _ => throw new NotSupportedException($"Unsupported accessibility: {symbol.DeclaredAccessibility}")
            }
            ;
            var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : symbol.ContainingNamespace.ToDisplayString();
            var hasAbstractBaseType = symbol is { BaseType.IsAbstract: true, TypeKind: TypeKind.Class };
            var properties = GetAllProperties(symbol).Select(p => PropertyInfoBase.Create(p, compilation)).ToArray();

            var propertyInitializers = new StringBuilder();
            var toDictionaryLogic = new StringBuilder();

            foreach (var property in properties)
            {
                propertyInitializers.AppendLine(property.Deserializer());
                toDictionaryLogic.AppendLine(property.Serializer());
            }

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("#pragma warning disable CS8600, CS8604 // nullable casting warnings");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            if (namespaceName != null)
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    // {symbol.TypeKind}");
            sb.AppendLine($"    {accessModifier} partial {symbolType} {symbolName}");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        public static {(hasAbstractBaseType ? "new " : "")}{symbolName} FromDictionary(IDictionary<string, object?> dictionary)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));");
            sb.AppendLine($"            return new {symbolName}");
            sb.AppendLine($"            {{");
            sb.AppendLine(propertyInitializers.ToString().TrimEnd(','));
            sb.AppendLine($"            }};");
            sb.AppendLine($"        }}");
            sb.AppendLine($"        public {(hasAbstractBaseType ? "override" : "")} Dictionary<string, object?> ToDictionary()");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            var dictionary = new Dictionary<string, object?>();");
            if (hasAbstractBaseType)
                sb.AppendLine($"            dictionary[\"__type__\"] = \"{symbolName}\";");
            sb.AppendLine(toDictionaryLogic.ToString());
            sb.AppendLine($"            return dictionary;");
            sb.AppendLine($"        }}");
            sb.AppendLine();
            sb.AppendLine($"        public static implicit operator Dictionary<string, object?>({symbolName} obj) => obj.ToDictionary();");
            sb.AppendLine($"    }}");
            if (namespaceName != null)
                sb.AppendLine("}");
            sb.AppendLine("#nullable restore");
            return sb.ToString();
        }

        private abstract class PropertyInfoBase(IPropertySymbol property)
        {
            private IPropertySymbol Property => property;
            private protected ITypeSymbol Type => property.Type;
            private protected virtual bool IsNullable => Type.NullableAnnotation == NullableAnnotation.Annotated;
            private protected virtual bool IsRequired => Property.IsRequired;
            private protected abstract string SerializedType { get; }

            private protected virtual ITypeSymbol AnnotatedActualType => Type switch
            {
                INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType => namedType.TypeArguments[0],
                _ => Type
            };

            private protected virtual ITypeSymbol ActualType => AnnotatedActualType.WithNullableAnnotation(NullableAnnotation.None);

            public string Name => property.Name;
            public abstract string Deserializer();
            public abstract string Serializer();

            public static PropertyInfoBase Create(IPropertySymbol property, Compilation compilation)
            {
                return TypeUtils.GetTypeShape(property.Type, compilation) switch
                {
                    { IsEnum: true } => new EnumTypePropertyInfo(property),
                    { IsString: true } => new StringTypePropertyInfo(property),
                    { IsDictionary: true } => new DictionaryTypePropertyInfo(property),
                    { IsEnumerable: true } => new ListTypePropertyInfo(property),
                    { IsArray: true } => new ArrayTypePropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: false, Type.SpecialType: SpecialType.System_Decimal } => new ValueTypeAsStringPropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: true, Type.SpecialType: SpecialType.System_Decimal } => new NullableValueTypeAsStringPropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: false, Type.SpecialType: SpecialType.System_DateTime } => new ValueTypeAsStringPropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: true, Type.SpecialType: SpecialType.System_DateTime } => new NullableValueTypeAsStringPropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: false } => new ValueTypePropertyInfo(property),
                    { IsBuiltInValueType: true, IsNullable: true } => new NullableValueTypePropertyInfo(property),
                    { IsUserDefinedStruct: true } => new StructTypePropertyInfo(property),
                    { TypeKind: TypeKind.Class } => new ReferenceTypePropertyInfo(property),
                    _ => throw new ArgumentOutOfRangeException(nameof(property.Type),
                        $"Unknown property type ({property.Type}) for property: {property.Name}. Type: {property.Type}, Original Definition: {property.Type.OriginalDefinition}, IsValueType: {property.Type.IsValueType} "),
                };
            }

            private protected string TypeInfo => $"Name: {Name}, Type.Name: {Type.Name}, Type.TypeKind: {Type.TypeKind}, Type.SpecialType: {Type.SpecialType}, Type.OriginalDefinition.Name: {Type.OriginalDefinition.Name}, Type.OriginalDefinition.TypeKind: {Type.OriginalDefinition.TypeKind}, Type.OriginalDefinition.SpecialType: {Type.OriginalDefinition.SpecialType}, AnnotatedActualType: {AnnotatedActualType}, IsNullable: {IsNullable}, IsRequired: {IsRequired}, AllInterfaces: {string.Join(":", Type.AllInterfaces.Select(x => x.Name))}, OriginalDefinition.AllInterfaces: {string.Join(":", Type.OriginalDefinition.AllInterfaces.Select(x => x.Name))}";
        }

        private class StringTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "string";
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // StringTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {Name.ToLower()}Raw
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // StringTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {Name.ToLower()}Raw
                                           : {(IsNullable ? "null" : "\"\"")},
                       """;

            public override string Serializer() => $"""            dictionary["{Name}"] = {Name};""";
        }

        private class ValueTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => AnnotatedActualType.Name;
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // ValueTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value)
                                               ? ({AnnotatedActualType})(Convert.ChangeType({Name.ToLower()}Value ?? throw new Exception("{Name}was not convertible to {AnnotatedActualType.Name}"), typeof({AnnotatedActualType})))
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // ValueTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value)
                                           ? ({AnnotatedActualType})(Convert.ChangeType({Name.ToLower()}Value ?? throw new Exception("{Name}was not convertible to {AnnotatedActualType.Name}"), typeof({AnnotatedActualType})))
                                           : {(IsNullable ? "null" : "default")},
                       """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name};""";
        }

        private class NullableValueTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => AnnotatedActualType.Name;
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // NullableValueTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value)
                                               ? {Name.ToLower()}Value != null
                                                   ? ({AnnotatedActualType})(Convert.ChangeType({Name.ToLower()}Value, typeof({AnnotatedActualType})))
                                                   : null
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                        // NullableValueTypePropertyInfo !Required Nullable {TypeInfo}
                                        {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value)
                                            ? {Name.ToLower()}Value != null
                                                ? ({AnnotatedActualType})(Convert.ChangeType({Name.ToLower()}Value, typeof({AnnotatedActualType})))
                                                : null
                                            : {(IsNullable ? "null" : "default")},
                        """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name};""";
        }

        private class ValueTypeAsStringPropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "string";
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // ValueTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? ({AnnotatedActualType})Convert.ChangeType({Name.ToLower()}Raw, typeof({AnnotatedActualType}))
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // ValueTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? ({AnnotatedActualType})Convert.ChangeType({Name.ToLower()}Raw, typeof({AnnotatedActualType}))
                                           : {(IsNullable ? "null" : "default")},
                       """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name}{(IsNullable?"?":"")}.ToString();""";
        }

        private class NullableValueTypeAsStringPropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "string";
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // NullableValueTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? ({AnnotatedActualType})Convert.ChangeType({Name.ToLower()}Raw, typeof({AnnotatedActualType}))
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // NullableValueTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? ({AnnotatedActualType})Convert.ChangeType({Name.ToLower()}Raw, typeof({AnnotatedActualType}))
                                           : {(IsNullable ? "null" : "default")},
                       """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name};""";
        }

        private class StructTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "IDictionary<string, object?>";
            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // StructTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // StructTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                           : {(IsNullable ? "null" : "default")},
                       """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name}{(IsNullable ? "?" : "")}.ToDictionary();""";
        }


        private class NullableStructValueTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "string";

            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // NullableStructValueTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // NullableStructValueTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                           : {(IsNullable ? "null" : "default")},
                       """;
            public override string Serializer() => $"""            dictionary["{Name}"] = {Name};""";
        }

        private class EnumTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "string";

            public override string Deserializer() =>
                IsRequired
                    ? $"""
                                       // EnumTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? ({AnnotatedActualType})Enum.Parse(typeof({ActualType}), {Name.ToLower()}Raw)
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // EnumTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? ({AnnotatedActualType})Enum.Parse(typeof({ActualType}), {Name.ToLower()}Raw)
                                           : {(IsNullable ? "null" : "default")},
                       """;

            public override string Serializer()
            {
                if (IsRequired)
                    return IsNullable
                        ? $"""
                                       if ({Name} == null) dictionary["{Name}"] = null;
                                       else dictionary["{Name}"] = Enum.GetName(typeof({ActualType}), {Name});
                           """
                        : $"""
                                       if ({Name} == null) throw new InvalidOperationException("{Name} is non-nullable but has null value.");
                                       else dictionary["{Name}"] = Enum.GetName(typeof({ActualType}), {Name});
                           """;
                return IsNullable
                    ? $"""            dictionary["{Name}"] = {Name} == null ? null : Enum.GetName(typeof({ActualType}), {Name});"""
                    : $"""
                                   if ({Name} == null) throw new InvalidOperationException("{Name} is non-nullable but has null value.");
                                   else dictionary["{Name}"] = Enum.GetName(typeof({ActualType}), {Name});
                       """;
            }
        }

        private class ReferenceTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "IDictionary<string, object?>";
            public override string Deserializer()
            {
                return IsRequired
                    ? $"""
                                       // ReferenceTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),    
                       """
                    : $"""
                                       // ReferenceTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {AnnotatedActualType}.FromDictionary({Name.ToLower()}Raw)
                                           : {(IsNullable ? "null" : "default")},
                       """;
            }

            public override string Serializer()
            {
                return $"""
                                    dictionary["{Name}"] = {Name} == null 
                                        ? {(IsNullable ? "null" : $"throw new InvalidOperationException(\"{Name} is non-nullable but has null value.\")")}
                                        : {Name}{(IsNullable ? "?" : "")}.ToDictionary();
                        """;
            }
        }

        private class ListTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "object[]";
            private ITypeSymbol AnnotatedElementType => ((INamedTypeSymbol)Type).TypeArguments[0];
            private ITypeSymbol ElementType => AnnotatedElementType.WithNullableAnnotation(NullableAnnotation.None);
            private bool IsElementTypeNullable => AnnotatedElementType.NullableAnnotation == NullableAnnotation.Annotated;
            private bool IsInterface => Type.TypeKind == TypeKind.Interface;
            public override string Deserializer()
            {
                return IsRequired
                    ? $"""
                                       // ListTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {(IsInterface ? "" : $"new {AnnotatedActualType}(")}{Name.ToLower()}Raw.Select(x => {(IsElementTypeNullable ? $"({AnnotatedElementType})" : "")}{ElementType}.FromDictionary((IDictionary<string, object?>)x)).ToArray(){(IsInterface ? "" : ")")}
                                               : {(IsNullable ? "null" : "[]")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // ListTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {(IsInterface ? "" : $"new {AnnotatedActualType}(")}{Name.ToLower()}Raw.Select(x => {(IsElementTypeNullable ? $"({AnnotatedElementType})" : "")}{ElementType}.FromDictionary((IDictionary<string, object?>)x)).ToArray(){(IsInterface ? "" : ")")}
                                           : {(IsNullable ? "null" : "[]")},
                       """;
            }

            public override string Serializer()
            {
                return $"""
                                    dictionary["{Name}"] = {Name} == null 
                                        ? {(IsNullable ? "null" : $"throw new InvalidOperationException(\"{Name} is non-nullable but has null value.\")")}
                                        : {Name}.Select(x => x{(IsElementTypeNullable ? "?" : "")}.ToDictionary()).ToArray();
                        """;
            }
        }

        private class DictionaryTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "IDictionary<string, object?>";
            private ITypeSymbol AnnotatedValueType => ((INamedTypeSymbol)Type).TypeArguments[1];
            private ITypeSymbol ValueType => AnnotatedValueType.WithNullableAnnotation(NullableAnnotation.None);
            private bool IsValueTypeNullable => AnnotatedValueType.NullableAnnotation == NullableAnnotation.Annotated;

            public override string Deserializer()
            {
                return IsRequired
                    ? $"""
                                       // DictionaryTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {Name.ToLower()}Raw.ToDictionary(x => x.Key, x => x.Value as IDictionary<string, object?>).ToDictionary(x => x.Key, x => {(IsValueTypeNullable ? $"({AnnotatedValueType})" : "")}{ValueType}.FromDictionary((IDictionary<string, object?>)x.Value))
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                       // DictionaryTypePropertyInfo !Required Nullable {TypeInfo}
                                       {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                           ? {Name.ToLower()}Raw.ToDictionary(x => x.Key, x => x.Value as IDictionary<string, object?>).Where(x => x.Value != null).ToDictionary(x => x.Key, x => {(IsValueTypeNullable ? $"({AnnotatedValueType})" : "")}{ValueType}.FromDictionary((IDictionary<string, object?>)x.Value))
                                           : {(IsNullable ? "null" : "[]")},
                       """;
            }

            public override string Serializer()
            {
                return $"""
                                    dictionary["{Name}"] = {Name} == null 
                                        ? {(IsNullable ? "null" : $"throw new InvalidOperationException(\"{Name} is non-nullable but has null value.\")")}
                                        : {Name}.ToDictionary(x => x.Key, x => x.Value{(IsValueTypeNullable ? "?" : "")}.ToDictionary());
                        """;
            }
        }

        private class ArrayTypePropertyInfo(IPropertySymbol property) : PropertyInfoBase(property)
        {
            private protected override string SerializedType => "object[]";
            private ITypeSymbol AnnotatedElementType => ((IArrayTypeSymbol)Type).ElementType;
            private ITypeSymbol ElementType => AnnotatedElementType.WithNullableAnnotation(NullableAnnotation.None);
            private bool IsElementTypeNullable => AnnotatedElementType.NullableAnnotation == NullableAnnotation.Annotated;

            public override string Deserializer()
            {
                return IsRequired
                    ? $"""
                                       // ArrayTypePropertyInfo Required !Nullable {TypeInfo}
                                       {Name} = dictionary.ContainsKey("{Name}")
                                           ? dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                               ? {Name.ToLower()}Raw.Select(x => {ElementType}.FromDictionary((IDictionary<string, object?>)x)).ToArray()
                                               : {(IsNullable ? "null" : $"throw new KeyNotFoundException(\"The required property '{Name}' is missing or invalid.\")")}
                                           : throw new KeyNotFoundException("The required property '{Name}' is missing or invalid."),
                       """
                    : $"""
                                        // ArrayTypePropertyInfo !Required Nullable {TypeInfo}
                                        {Name} = dictionary.TryGetValue("{Name}", out var {Name.ToLower()}Value) && {Name.ToLower()}Value is {SerializedType} {Name.ToLower()}Raw
                                            ? {Name.ToLower()}Raw.Select(x => {ElementType}.FromDictionary((IDictionary<string, object?>)x)).ToArray()
                                            : {(IsNullable ? "null" : "[]")},
                        """;
            }

            public override string Serializer()
            {
                return $"""
                                    dictionary["{Name}"] = {Name} == null 
                                        ? {(IsNullable ? "null" : $"throw new InvalidOperationException(\"{Name} is non-nullable but has null value.\")")}
                                        : {Name}.Select(x => x{(IsElementTypeNullable ? "?" : "")}.ToDictionary()).ToArray();
                        """;
            }
        }

        private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol classSymbol)
        {
            var current = classSymbol;
            while (current != null)
            {
                foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                    yield return member;
                current = current.BaseType;
            }
        }

        private static string GeneratePolymorphicDispatcher(INamedTypeSymbol baseType, IEnumerable<INamedTypeSymbol> derivedTypes)
        {
            var namespaceName = baseType.ContainingNamespace.IsGlobalNamespace
                ? null
                : baseType.ContainingNamespace.ToDisplayString();
            var baseName = baseType.Name;
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            if (namespaceName != null)
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }
            sb.AppendLine($"    public partial class {baseName}");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        public static {baseName} FromDictionary(IDictionary<string, object?> dictionary)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            if (!dictionary.TryGetValue(\"__type__\", out var typeObj) || typeObj is not string typeName)");
            sb.AppendLine($"                throw new ArgumentException(\"Missing or invalid __type__ discriminator\", nameof(dictionary));");
            sb.AppendLine($"            return typeName switch");
            sb.AppendLine($"            {{");
            foreach (var type in derivedTypes)
                sb.AppendLine($"                \"{type.Name}\" => {type.ToDisplayString()}.FromDictionary(dictionary),");
            sb.AppendLine($"                _ => throw new NotSupportedException($\"Unknown type discriminator: {{typeName}}\")");
            sb.AppendLine($"            }};");
            sb.AppendLine($"        }}");
            sb.AppendLine();
            sb.AppendLine($"        public abstract Dictionary<string, object?> ToDictionary();");
            sb.AppendLine();
            sb.AppendLine($"        public static implicit operator Dictionary<string, object?>({baseName} obj) => obj.ToDictionary();");
            sb.AppendLine($"    }}");
            if (namespaceName != null)
                sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
