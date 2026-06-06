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

        /// <summary>Identifies Razor-generated files that view-only analyzers should inspect.</summary>
        internal static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase);
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

        /// <summary>Checks direct or indirect <c>ReactiveValidator&lt;T&gt;</c> inheritance.</summary>
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

        /// <summary>Matches a closed generic type against its unbound generic definition.</summary>
        internal static bool IsClosedGenericOf(ITypeSymbol? type, INamedTypeSymbol? openGenericType)
        {
            if (openGenericType == null) return false;
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;
            return SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, openGenericType);
        }

        /// <summary>Returns the previous invocation in a fluent method chain.</summary>
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
