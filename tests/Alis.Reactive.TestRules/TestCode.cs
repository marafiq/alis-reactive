using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

internal static class TestCode
{
    internal const string Category = "Alis.Reactive.TestRules";
    internal const string NUnitsTest = "NUnit.Framework.TestAttribute";
    internal const string NUnitsAssert = "NUnit.Framework.Assert";
    internal const string NUnitsConstraint = "NUnit.Framework.Constraints.IResolveConstraint";

    private const string Playwright = "Microsoft.Playwright";
    private const string Harness = "Harness";
    private const string That = "That";

    private static readonly char[] Separators = ['/', '\\'];

    internal static bool IsOfPlaywright(ISymbol symbol) => symbol.ContainingAssembly?.Name == Playwright;

    internal static bool IsInTheHarness(SyntaxTree tree)
    {
        var parts = tree.FilePath.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        return parts.Take(parts.Length - 1).Contains(Harness, StringComparer.Ordinal);
    }

    internal static bool IsATest(IMethodSymbol method, INamedTypeSymbol test)
        => method.GetAttributes().Any(applied => SymbolEqualityComparer.Default.Equals(applied.AttributeClass, test));

    internal static IOperation Written(IOperation value)
    {
        while (value is IConversionOperation { IsImplicit: true } converted) value = converted.Operand;
        return value;
    }

    internal static bool IsAssertThat(IInvocationOperation called, INamedTypeSymbol assert)
        => called.TargetMethod.Name == That && SymbolEqualityComparer.Default.Equals(called.TargetMethod.ContainingType, assert);

    internal static IOperation? ConstraintOf(IInvocationOperation that, INamedTypeSymbol constraint)
        => that.Arguments.FirstOrDefault(given => SymbolEqualityComparer.Default.Equals(given.Parameter?.Type, constraint))?.Value;

    internal static Location NameOf(IInvocationOperation called) => called.Syntax switch
    {
        InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax reached } => reached.Name.GetLocation(),
        InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax bound } => bound.Name.GetLocation(),
        InvocationExpressionSyntax written => written.Expression.GetLocation(),
        _ => called.Syntax.GetLocation(),
    };

    internal static Location NameOf(IMemberReferenceOperation read) => read.Syntax switch
    {
        MemberAccessExpressionSyntax reached => reached.Name.GetLocation(),
        MemberBindingExpressionSyntax bound => bound.Name.GetLocation(),
        _ => read.Syntax.GetLocation(),
    };
}
