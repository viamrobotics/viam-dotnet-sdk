using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Viam.Serialization.Analyzer
{
    [Generator]
    public class StructMapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidates = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => HasRequiredAttribute(node),
                    transform: static (ctx, _) => GetSymbol(ctx))
                .Where(static s => s is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(candidates.Collect());
            context.RegisterSourceOutput(compilationAndClasses, static (context, source) =>
            {
                var (compilation, classes) = source;
                foreach (var classSymbol in classes.Distinct(SymbolEqualityComparer.Default))
                {
                    var namedClassSymbol = classSymbol as INamedTypeSymbol;
                    if (namedClassSymbol is null || !IsPartial(namedClassSymbol))
                    {
                        ReportNotPartialDiagnostic(context, namedClassSymbol);
                        continue;
                    }

                    var code = StructMapperEmitter.Generate(namedClassSymbol, compilation);
                    var safeName = namedClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        .Replace("global::", "").Replace(".", "_");

                    context.AddSource($"{safeName}_StructMapper.g.cs", code);
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
                        .Any(attr => attr.Name.ToString() == "StructMappable");

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
                        .Any(attr => attr.Name.ToString() == "StructMappable");

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

        private static bool IsPartial(INamedTypeSymbol classSymbol)
        {
            return classSymbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<ClassDeclarationSyntax>()
                .All(decl => decl.Modifiers.Any(SyntaxKind.PartialKeyword));
        }

        private static void ReportNotPartialDiagnostic(SourceProductionContext context, INamedTypeSymbol? symbol)
        {
            if (symbol is null) return;

            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    id: "VIAMGEN002",
                    title: "StructMapper requires partial classes",
                    messageFormat: "Class '{0}' must be marked 'partial' to allow source generation.",
                    category: "StructMapper",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                symbol.Name));
        }
    }
}
