using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using System.Data.SqlTypes;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace Viam.Serialization.Generators
{
    [Generator]
    public class DictionaryMapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all classes with the [ReadingsDictionaryMapper] attribute
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsClassWithAttribute(node),
                    transform: static (context, _) => GetClassSymbol(context))
                .Where(static symbol => symbol is not null);

            // Generate code for each class
            context.RegisterSourceOutput(classDeclarations, (context, classSymbol) =>
            {
                if (classSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    var generatedCode = GenerateMapperCode(namedTypeSymbol);
                    context.AddSource($"{namedTypeSymbol.Name}_DictionaryMapper.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                }
            });
        }

        private static bool IsClassWithAttribute(SyntaxNode node)
        {
            if (node is not ClassDeclarationSyntax classDecl)
                return false;
            var hasAttribute = classDecl.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .Any(attr => attr.Name.ToString() == nameof(DictionaryMapperAttribute).Replace("Attribute", ""));
            if (!hasAttribute)
                return false;
            var isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

            if (!isPartial)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "VIAMGEN003",
                        title: "DictionaryMapper requires partial classes",
                        messageFormat: "Class '{0}' must be marked 'partial' to allow source generation.",
                        category: "DictionaryMapper",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    classDecl.Identifier.GetLocation(),
                    classDecl.Identifier.Text);

                // Report the diagnostic
                throw new InvalidOperationException($"Class '{classDecl.Identifier.Text}' must be marked 'partial' to allow source generation.");
            }

            return true;
        }

        private static INamedTypeSymbol? GetClassSymbol(GeneratorSyntaxContext context)
        {
            // Get the semantic model and symbol for the class
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            return context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        }

        private static string SanitizeName(string name)
        {
            // Sanitize the name to be a valid C# identifier
            return name
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace("[", "_")
                .Replace("]", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("?", "_")
                .Replace(":", "_")
                .Replace(",", "_")
                .Replace(".", "_");
        }

        private static string GenerateMapperCode(INamedTypeSymbol classSymbol)
        {
            //System.Diagnostics.Debugger.Launch();
            var className = classSymbol.Name;
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();

            var properties = GetAllProperties(classSymbol);

            var propertyInitializers = new StringBuilder();
            var toDictionaryLogic = new StringBuilder();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyType = property.Type;
                var isNullable = propertyType.NullableAnnotation == NullableAnnotation.Annotated;
                var underlyingType = isNullable && propertyType is INamedTypeSymbol namedTypeSymbol
                    ? namedTypeSymbol.ConstructedFrom.ToDisplayString() ?? propertyType.ToDisplayString()
                    : propertyType.ToDisplayString();

                if (IsComplexType(propertyType))
                {
                    // Handle embedded types
                    propertyInitializers.AppendLine($@"                {propertyName} = dictionary.TryGetValue(""{propertyName}"", out var {propertyName.ToLower()}Value) && {propertyName.ToLower()}Value is Dictionary<string, object?> {propertyName.ToLower()}Dict");
                    propertyInitializers.AppendLine($@"                    ? {underlyingType}.FromDictionary({propertyName.ToLower()}Dict)");
                    propertyInitializers.AppendLine($@"                    : null,");

                    toDictionaryLogic.AppendLine($@"            dictionary[""{propertyName}""] = {propertyName} != null");
                    toDictionaryLogic.AppendLine($@"                ? {propertyName}.ToDictionary()");
                    toDictionaryLogic.AppendLine($@"                : null;");
                }
                else
                {
                    // Handle primitive types
                    if (isNullable)
                    {
                        propertyInitializers.AppendLine($@"                {propertyName} = dictionary.TryGetValue(""{propertyName}"", out var {propertyName.ToLower()}Value) && {propertyName.ToLower()}Value is {underlyingType} {propertyName.ToLower()}{SanitizeName(underlyingType)}value");
                        propertyInitializers.AppendLine($@"                    ? ({propertyType.ToDisplayString()}){propertyName.ToLower()}{SanitizeName(underlyingType)}value");
                        propertyInitializers.AppendLine($@"                    : default,");
                    }
                    else
                    {
                        propertyInitializers.AppendLine($@"                {propertyName} = dictionary.TryGetValue(""{propertyName}"", out var {propertyName.ToLower()}Value) && {propertyName.ToLower()}Value is {underlyingType} {propertyName.ToLower()}{SanitizeName(underlyingType)}value");
                        propertyInitializers.AppendLine($@"                    ? ({propertyType.ToDisplayString()}){propertyName.ToLower()}{SanitizeName(underlyingType)}value");
                        propertyInitializers.AppendLine($@"                    : throw new KeyNotFoundException(""The required property '{propertyName}' is missing or has an invalid type.""),");
                    }
                    toDictionaryLogic.AppendLine($@"            dictionary[""{propertyName}""] = {propertyName};");
                }
            }
            var sb = new StringBuilder();
            sb.AppendLine($"// <auto-generated />");
            sb.AppendLine($"#nullable enable");
            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            if (namespaceName != null)
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }
            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static {className} FromDictionary(Dictionary<string, object?> dictionary)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));");
            sb.AppendLine($"            return new {className}");
            sb.AppendLine($"            {{");
            sb.AppendLine($"{propertyInitializers.ToString().TrimEnd(',')}");
            sb.AppendLine($"            }};");
            sb.AppendLine($"        }}");
            sb.AppendLine($"        public IDictionary<string, object?> ToDictionary()");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            var dictionary = new Dictionary<string, object?>();");
            sb.AppendLine(toDictionaryLogic.ToString());
            sb.AppendLine($"            return dictionary;");
            sb.AppendLine($"        }}");
            sb.AppendLine($"    }}");
            if (namespaceName != null)
            {
                sb.AppendLine("}");
            }
            sb.AppendLine($"#nullable restore");
            // Return the generated code as a string
            return sb.ToString();
        }

        private static bool IsComplexType(ITypeSymbol typeSymbol)
        {
            // Check if the type is a class and not a primitive, string, or collection
            return typeSymbol.TypeKind == TypeKind.Class &&
                   typeSymbol.SpecialType == SpecialType.None &&
                   typeSymbol.Name != "String";
        }

        private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol classSymbol)
        {
            var currentSymbol = classSymbol;
            while (currentSymbol != null)
            {
                foreach (var member in currentSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    yield return member;
                }
                currentSymbol = currentSymbol.BaseType;
            }
        }
    }
}
