using Microsoft.CodeAnalysis;

namespace Alis.Reactive.Poc.Analyzers;

internal sealed class Framework
{
    private Framework(INamedTypeSymbol jsObject, INamedTypeSymbol component, INamedTypeSymbol plugin, INamedTypeSymbol mirrors)
    {
        JsObject = jsObject;
        Component = component;
        Plugin = plugin;
        Mirrors = mirrors;
    }

    public INamedTypeSymbol JsObject { get; }

    public INamedTypeSymbol Component { get; }

    public INamedTypeSymbol Plugin { get; }

    public INamedTypeSymbol Mirrors { get; }

    public static Framework? Of(Compilation compilation)
    {
        var jsObject = compilation.GetTypeByMetadataName("Alis.Reactive.Components.JsObject");
        var component = compilation.GetTypeByMetadataName("Alis.Reactive.Components.Component");
        var plugin = compilation.GetTypeByMetadataName("Alis.Reactive.Components.Plugin");
        var mirrors = compilation.GetTypeByMetadataName("Alis.Reactive.Components.MirrorsAttribute");
        return jsObject is null || component is null || plugin is null || mirrors is null ? null : new Framework(jsObject, component, plugin, mirrors);
    }

    public static (string Module, string Name) Arguments(AttributeData attribute)
        => (attribute.ConstructorArguments[0].Value as string ?? "", attribute.ConstructorArguments[1].Value as string ?? "");

    public bool IsRoot(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract) return false;
        for (var basis = type.BaseType; basis is not null; basis = basis.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(basis, Component) || SymbolEqualityComparer.Default.Equals(basis, Plugin)) return true;
        }

        return false;
    }

    public AttributeData? MirrorsOf(INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, Mirrors)) return attribute;
        }

        return null;
    }
}
