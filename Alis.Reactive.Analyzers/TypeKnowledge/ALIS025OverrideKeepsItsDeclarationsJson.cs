using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.TypeKnowledge;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS025OverrideKeepsItsDeclarationsJson : DiagnosticAnalyzer
{
    public const string Id = "ALIS025";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string Title = "An override keeps the JSON name and the ignore of the member it overrides";
    private const string NameAttribute = "System.Text.Json.Serialization.JsonPropertyNameAttribute";
    private const string IgnoreAttribute = "System.Text.Json.Serialization.JsonIgnoreAttribute";
    private const string IgnoreCondition = "Condition";
    private const string IgnoredAlways = "Always";
    private const string NoName = "no JsonPropertyName";
    private const string NoIgnore = "no JsonIgnore";

    private static readonly DiagnosticDescriptor Renamed = new(
        Id,
        Title,
        "The override '{0}' carries {1} where '{2}', the member it overrides, carries {3}; an override keeps the JSON name of the member it overrides, so give it {3}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor IgnoredOtherwise = new(
        Id,
        Title,
        "The override '{0}' carries {1} where '{2}', the member it overrides, carries {3}; an override keeps the ignore of the member it overrides, so give it {3}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Renamed, IgnoredOtherwise);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var name = start.Compilation.GetTypeByMetadataName(NameAttribute);
        var ignore = start.Compilation.GetTypeByMetadataName(IgnoreAttribute);
        if (name is null || ignore is null) return;

        start.RegisterSymbolAction(property => Checked(property, name, ignore), SymbolKind.Property);
    }

    private static void Checked(SymbolAnalysisContext context, INamedTypeSymbol name, INamedTypeSymbol ignore)
    {
        var property = (IPropertySymbol)context.Symbol;
        if (property.PartialDefinitionPart is not null) return;
        if (property.OverriddenProperty is not { } declared) return;
        if (!AttributesCompile(property) || !AttributesCompile(declared)) return;

        var writtenName = NameOf(property, name);
        var declaredName = NameOf(declared, name);
        if (writtenName != declaredName) Report(context, Renamed, property, declared, writtenName, declaredName);

        var writtenIgnore = IgnoreOf(property, ignore);
        var declaredIgnore = IgnoreOf(declared, ignore);
        if (writtenIgnore != declaredIgnore) Report(context, IgnoredOtherwise, property, declared, writtenIgnore, declaredIgnore);
    }

    private static bool AttributesCompile(IPropertySymbol property)
        => property.GetAttributes().All(attribute => attribute.AttributeConstructor is not null);

    private static string NameOf(IPropertySymbol property, INamedTypeSymbol name)
    {
        var named = AttributeOf(property, name);
        return named is null ? NoName : $"[JsonPropertyName(\"{named.ConstructorArguments[0].Value}\")]";
    }

    private static string IgnoreOf(IPropertySymbol property, INamedTypeSymbol ignore)
    {
        var ignored = AttributeOf(property, ignore);
        if (ignored is null) return NoIgnore;

        var condition = ignored.NamedArguments.Where(argument => argument.Key == IgnoreCondition).Select(argument => ConditionNameOf(argument.Value)).FirstOrDefault() ?? IgnoredAlways;
        return condition == IgnoredAlways ? "[JsonIgnore]" : $"[JsonIgnore(Condition = JsonIgnoreCondition.{condition})]";
    }

    private static string ConditionNameOf(TypedConstant condition)
        => condition.Type?.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(member => member.HasConstantValue && Equals(member.ConstantValue, condition.Value))?.Name
           ?? Convert.ToString(condition.Value, CultureInfo.InvariantCulture);

    private static AttributeData? AttributeOf(IPropertySymbol property, INamedTypeSymbol attribute)
        => property.GetAttributes().FirstOrDefault(written => SymbolEqualityComparer.Default.Equals(written.AttributeClass, attribute));

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor rule, IPropertySymbol property, IPropertySymbol declared, string written, string declaredText)
        => context.ReportDiagnostic(Diagnostic.Create(rule, property.Locations[0], $"{property.ContainingType.Name}.{property.Name}", written, $"{declared.ContainingType.Name}.{declared.Name}", declaredText));
}
