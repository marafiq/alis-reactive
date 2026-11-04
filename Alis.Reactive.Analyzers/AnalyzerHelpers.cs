using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alis.Reactive.Analyzers
{
    internal static class AnalyzerHelpers
    {
        internal static readonly ImmutableHashSet<string> HttpMethodNames =
            ImmutableHashSet.Create("Get", "Post", "Put", "Delete");

        internal static readonly ImmutableHashSet<string> ShippedAssemblies =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Alis.Reactive",
                "Alis.Reactive.Fusion",
                "Alis.Reactive.Native",
                "Alis.Reactive.NativeTagHelpers");

        internal static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("_cshtml.g.cs", StringComparison.OrdinalIgnoreCase);
        }

        internal static ClassDeclarationSyntax? FindContainingClass(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is ClassDeclarationSyntax classDecl)
                    return classDecl;
                current = current.Parent;
            }
            return null;
        }

        internal static bool InheritsFromReactiveValidator(
            ClassDeclarationSyntax classDecl,
            SemanticModel semanticModel,
            INamedTypeSymbol reactiveValidatorType,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetDeclaredSymbol(classDecl, cancellationToken) is not INamedTypeSymbol classSymbol)
                return false;

            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType
                    && SymbolEqualityComparer.Default.Equals(
                        baseType.ConstructedFrom, reactiveValidatorType))
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        internal static bool IsClosedGenericOf(ITypeSymbol? type, INamedTypeSymbol? openGenericType)
        {
            if (openGenericType == null) return false;
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;
            return SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, openGenericType);
        }

        internal static InvocationExpressionSyntax? GetReceiverInvocation(
            InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Expression is InvocationExpressionSyntax receiver)
                return receiver;
            return null;
        }
    }
}
