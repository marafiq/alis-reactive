using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS034VendorDeclaration : DiagnosticAnalyzer
{
    public const string Id = "ALIS034";

    private const string Category = "Alis.Reactive.Analyzers";

    private static readonly DiagnosticDescriptor NameUsesItsCharacters = new(
        Id,
        "A vendor name uses its permitted characters",
        "Vendor name '{0}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PathNamesEveryStep = new(
        Id,
        "A vendor path names every step",
        "Path '{0}' contains an empty segment at index {1}. Use dot-separated property names without consecutive, leading, or trailing dots.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NameNamesOneVendor = new(
        Id,
        "A vendor name names one definition",
        "Two different vendors are named '{0}'. Give each vendor its own name.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor PositionBelongsToArguments = new(
        Id,
        "An argument at a position belongs to a listener or callback",
        "An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(NameUsesItsCharacters, PathNamesEveryStep, NameNamesOneVendor, PositionBelongsToArguments);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var vendor = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Vendors.ComponentVendor");
        var lookup = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Vendors.VendorLookup");
        var declaration = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.JsObject");
        var raised = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.Event`1");
        var callback = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.Callback`2");
        if (vendor is null || lookup is null || declaration is null || raised is null || callback is null) return;

        var byPlugin = lookup.GetMembers("ByPlugin").OfType<IMethodSymbol>().Single();
        var answers = declaration.GetMembers("Answer").OfType<IMethodSymbol>().ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var positional = declaration.GetMembers("Argument").Concat(declaration.GetMembers("MethodAt")).OfType<IMethodSymbol>()
            .ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var doors = new Dictionary<IMethodSymbol, VendorDoor>(SymbolEqualityComparer.Default);
        foreach (var member in new[] { "Define", "DefineCallbackProperties", "DefineReturnedUnsubscriber" })
        {
            foreach (var method in vendor.GetMembers(member).OfType<IMethodSymbol>()) doors.Add(method, new VendorDoor(method, lookup));
        }

        var definitions = new ConcurrentBag<Definition>();
        var positions = new ConcurrentBag<(INamedTypeSymbol Type, Location Location)>();
        var arguments = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
        start.RegisterSymbolAction(context =>
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!DerivesFrom(type, declaration)) return;
            for (var current = type; current is not null; current = current.BaseType)
            {
                foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (property.Type is not INamedTypeSymbol member) continue;
                    if (SymbolEqualityComparer.Default.Equals(member.OriginalDefinition, raised) || SymbolEqualityComparer.Default.Equals(member.OriginalDefinition, callback))
                        KeepArguments(member.TypeArguments[0], arguments);
                }
            }
        }, SymbolKind.NamedType);
        start.RegisterOperationAction(context =>
        {
            var call = (IInvocationOperation)context.Operation;
            var method = call.TargetMethod;
            if (doors.TryGetValue(method.OriginalDefinition, out var door)) ReadVendor(context, call, door, byPlugin, definitions);
            if (answers.Contains(method.OriginalDefinition)) KeepArguments(method.TypeArguments[0], arguments);
            if (positional.Contains(method.OriginalDefinition) && context.ContainingSymbol.ContainingType is { } owner)
                positions.Add((owner, MemberName(call)));
        }, OperationKind.Invocation);
        start.RegisterCompilationEndAction(context =>
        {
            foreach (var position in positions)
            {
                if (!arguments.ContainsKey(position.Type.OriginalDefinition))
                    context.ReportDiagnostic(Diagnostic.Create(PositionBelongsToArguments, position.Location));
            }

            var trees = context.Compilation.SyntaxTrees.Select((tree, index) => (tree, index)).ToDictionary(pair => pair.tree, pair => pair.index);
            var firstByName = new Dictionary<string, Definition>(StringComparer.Ordinal);
            foreach (var definition in definitions.OrderBy(item => trees[item.Location.SourceTree!]).ThenBy(item => item.Location.SourceSpan.Start))
            {
                if (!firstByName.TryGetValue(definition.Name, out var first)) firstByName.Add(definition.Name, definition);
                else if (!SymbolEqualityComparer.Default.Equals(first.Factory, definition.Factory) || !first.Data.SequenceEqual(definition.Data, StringComparer.Ordinal))
                    context.ReportDiagnostic(Diagnostic.Create(NameNamesOneVendor, definition.Location, definition.Name));
            }
        });
    }

    private static void ReadVendor(OperationAnalysisContext context, IInvocationOperation call, VendorDoor door, IMethodSymbol byPlugin, ConcurrentBag<Definition> definitions)
    {
        if (call.DescendantsAndSelf().Any(operation => operation is IInvalidOperation || operation.Type?.TypeKind == TypeKind.Error)) return;
        var name = call.Arguments.FirstOrDefault(argument => SymbolEqualityComparer.Default.Equals(argument.Parameter, door.Name));
        if (name is null) return;
        var nameIsKnown = name.Value.ConstantValue.HasValue;
        var text = nameIsKnown ? name.Value.ConstantValue.Value as string : null;
        if (nameIsKnown && !ValidName(text))
            context.ReportDiagnostic(Diagnostic.Create(NameUsesItsCharacters, name.Value.Syntax.GetLocation(), text));

        foreach (var argument in call.Arguments)
        {
            if (argument.Parameter is null || !door.Paths.Contains(argument.Parameter) || argument.Value.ConstantValue is not { HasValue: true, Value: string path } || path.Length == 0) continue;
            var steps = path.Split('.');
            var empty = Array.FindIndex(steps, step => step.Length == 0);
            if (empty >= 0) context.ReportDiagnostic(Diagnostic.Create(PathNamesEveryStep, argument.Value.Syntax.GetLocation(), path, empty));
        }

        if (!nameIsKnown || text is null || !ValidName(text)) return;
        var data = new List<string?>();
        foreach (var argument in call.Arguments.OrderBy(argument => argument.Parameter?.Ordinal))
        {
            if (SymbolEqualityComparer.Default.Equals(argument.Parameter, door.Name)) continue;
            if (door.Lookup is not null && SymbolEqualityComparer.Default.Equals(argument.Parameter, door.Lookup))
            {
                var value = argument.Value;
                while (value is IConversionOperation converted) value = converted.Operand;
                if (value is not IInvocationOperation found || !SymbolEqualityComparer.Default.Equals(found.TargetMethod.OriginalDefinition, byPlugin)) return;
                var plugin = found.Arguments[0].Value.ConstantValue;
                if (!plugin.HasValue || plugin.Value is not string pluginName) return;
                data.Add(pluginName);
                continue;
            }

            var constant = argument.Value.ConstantValue;
            if (!constant.HasValue || constant.Value is not (null or string)) return;
            data.Add(constant.Value as string);
        }

        definitions.Add(new Definition(text, call.TargetMethod.OriginalDefinition, data.ToImmutableArray(), name.Value.Syntax.GetLocation()));
    }

    private static bool ValidName(string? name)
        => !string.IsNullOrEmpty(name) && IsLetter(name![0]) && name.All(character => IsLetter(character) || character is >= '0' and <= '9' or '_' or '-');

    private static bool IsLetter(char character) => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol declaration)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, declaration)) return true;
        }

        return false;
    }

    private static void KeepArguments(ITypeSymbol type, ConcurrentDictionary<INamedTypeSymbol, byte> arguments)
    {
        for (var current = type as INamedTypeSymbol; current is not null; current = current.BaseType)
            arguments.TryAdd(current.OriginalDefinition, 0);
    }

    private static Location MemberName(IInvocationOperation call)
        => call.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax member } ? member.Name.GetLocation()
            : call.Syntax is InvocationExpressionSyntax invocation ? invocation.Expression.GetLocation() : call.Syntax.GetLocation();

    private sealed class VendorDoor
    {
        internal VendorDoor(IMethodSymbol method, INamedTypeSymbol lookup)
        {
            Name = method.Parameters[0];
            Lookup = SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, lookup) ? method.Parameters[1] : null;
            Paths = ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, method.Parameters[method.Parameters.Length - 1]);
            if (Lookup is null) Paths = Paths.Add(method.Parameters[1]);
        }

        internal IParameterSymbol Name { get; }

        internal IParameterSymbol? Lookup { get; }

        internal ImmutableHashSet<IParameterSymbol> Paths { get; }
    }

    private sealed class Definition
    {
        internal Definition(string name, IMethodSymbol factory, ImmutableArray<string?> data, Location location)
        {
            Name = name;
            Factory = factory;
            Data = data;
            Location = location;
        }

        internal string Name { get; }

        internal IMethodSymbol Factory { get; }

        internal ImmutableArray<string?> Data { get; }

        internal Location Location { get; }
    }
}
