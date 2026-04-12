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

        /// <summary>
        /// Returns true when the syntax tree represents a Razor-generated file
        /// (.cshtml or .cshtml.g.cs). Framework analyzers that only target views
        /// should gate on this before doing any semantic work.
        /// </summary>
        internal static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the innermost <see cref="ClassDeclarationSyntax"/> containing
        /// the given node, or null if not inside a class.
        /// </summary>
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

        /// <summary>
        /// Returns true when the class inherits from <c>ReactiveValidator&lt;T&gt;</c>,
        /// walking the base-type chain via the semantic model. Handles qualified names,
        /// aliases, and indirect inheritance.
        /// </summary>
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

        /// <summary>
        /// Returns true when the <paramref name="type"/> is a closed generic
        /// whose unbound definition matches <paramref name="openGenericType"/>.
        /// Safe against null: returns false when either argument is null.
        /// </summary>
        internal static bool IsClosedGenericOf(ITypeSymbol? type, INamedTypeSymbol? openGenericType)
        {
            if (openGenericType == null) return false;
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;
            return SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, openGenericType);
        }

        /// <summary>
        /// Walks a fluent method-chain backwards: given <c>a.B().C()</c>, returns the
        /// <c>a.B()</c> invocation that <c>.C()</c> was called on.
        /// </summary>
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
