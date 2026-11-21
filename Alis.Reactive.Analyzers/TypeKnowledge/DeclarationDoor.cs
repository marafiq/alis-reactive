using Microsoft.CodeAnalysis;

namespace Alis.Reactive.Analyzers.TypeKnowledge;

internal static class DeclarationDoor
{
    internal const string Declarations = "Alis.Reactive.Declarations.JsObject";

    private static readonly SymbolDisplayFormat ByItsTypeAndName = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    internal static bool Is(IMethodSymbol called, INamedTypeSymbol declarations)
        => called is { IsStatic: false, DeclaredAccessibility: Accessibility.Protected }
           && SymbolEqualityComparer.Default.Equals(called.ContainingType, declarations);

    internal static string Named(ISymbol written)
        => (written is IMethodSymbol { AssociatedSymbol: { } declared } ? declared : written).ToDisplayString(ByItsTypeAndName);
}
