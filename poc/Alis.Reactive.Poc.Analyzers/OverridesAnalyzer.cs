using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OverridesAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "ALIS025";

    private const string Title = "An override is written as the member it overrides";

    private static readonly DiagnosticDescriptor Named = Rule("'{0}' is written as {1}, and '{2}', which it overrides, as {3}: System.Text.Json writes the override under both names; give it the JsonPropertyName of the member it overrides, or none where that member has none");

    private static readonly DiagnosticDescriptor Ignored = Rule("'{0}' is {1}, and '{2}', which it overrides, is {3}: give the override the JsonIgnore of the member it overrides");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Named, Ignored];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var name = start.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonPropertyNameAttribute");
            var ignore = start.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonIgnoreAttribute");
            if (name is null || ignore is null) return;
            start.RegisterSymbolAction(symbol => Check(symbol, name, ignore), SymbolKind.Property);
        });
    }

    private static void Check(SymbolAnalysisContext context, INamedTypeSymbol name, INamedTypeSymbol ignore)
    {
        var property = (IPropertySymbol)context.Symbol;
        if (property.OverriddenProperty is not { } overridden) return;
        var location = property.Locations[0];
        var written = Shown(property);
        var declaration = Shown(overridden);
        var writtenAs = NameOf(property, name);
        var declaredAs = NameOf(overridden, name);
        if (writtenAs != declaredAs)
        {
            context.ReportDiagnostic(Diagnostic.Create(Named, location, written, ShownName(writtenAs), declaration, ShownName(declaredAs)));
            return;
        }

        var ignoredWhen = IgnoredWhen(property, ignore);
        var declaredIgnoredWhen = IgnoredWhen(overridden, ignore);
        if (ignoredWhen != declaredIgnoredWhen)
        {
            context.ReportDiagnostic(Diagnostic.Create(Ignored, location, written, ShownIgnore(ignoredWhen), declaration, ShownIgnore(declaredIgnoredWhen)));
        }
    }

    private static string? NameOf(IPropertySymbol property, INamedTypeSymbol name)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, name)) return attribute.ConstructorArguments[0].Value as string;
        }

        return null;
    }

    private static int IgnoredWhen(IPropertySymbol property, INamedTypeSymbol ignore)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, ignore)) continue;
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == "Condition" && argument.Value.Value is int condition) return condition;
            }

            return Always;
        }

        return Never;
    }

    private const int Never = 0;

    private const int Always = 1;

    private const int WhenWritingDefault = 2;

    private static string ShownName(string? name) => name is null ? "its C# name" : "'" + name + "'";

    private static string ShownIgnore(int condition) => condition switch
    {
        Never => "written",
        Always => "ignored",
        WhenWritingDefault => "ignored when it holds its default",
        _ => "ignored when it holds null",
    };

    private static string Shown(IPropertySymbol property) => property.ContainingType.Name + "." + property.Name;

    private static DiagnosticDescriptor Rule(string message)
        => new(Id, Title, message, Rules.Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
