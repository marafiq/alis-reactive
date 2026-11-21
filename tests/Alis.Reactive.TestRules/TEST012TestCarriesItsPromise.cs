using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST012TestCarriesItsPromise : DiagnosticAnalyzer
{
    public const string Id = "TEST012";

    private const string Title = "A test carries the one promise it proves";
    private const string World = "Alis.Reactive.Conformance.World.";
    private const string StillBeingWritten = "build_property.TestsAreStillBeingWritten";
    private const string When = "When";

    private static readonly DiagnosticDescriptor NoMark = new(
        Id,
        Title,
        "'{0}' carries no [FailsFor]; a test names the one promise it proves",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NoConstant = new(
        Id,
        Title,
        "'{0}' names its promise by a value of its own; [FailsFor] takes a constant of Promises, which the spec generates",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Fixture = new(
        Id,
        Title,
        "'{0}' holds a test and is not named When and its situation; name the fixture for the situation its tests share",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Gate = new(
        Id,
        Title,
        "'{0}' never arms {1}, which its promise holds; arm the gate before the gesture and release it once it is held",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Twice = new(
        Id,
        Title,
        "'{0}' proves {1}, which '{2}' proves already; a promise has one test",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor NoTest = new(
        Id,
        Title,
        "No test carries {0}; every test of the spec is written from its promise",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(NoMark, NoConstant, Fixture, Gate, Twice, NoTest);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var test = start.Compilation.GetTypeByMetadataName(TestCode.NUnitsTest);
        var failsFor = start.Compilation.GetTypeByMetadataName(World + "FailsForAttribute");
        var promises = start.Compilation.GetTypeByMetadataName(World + "Promises");
        if (test is null || failsFor is null || promises is null) return;

        var holds = start.Compilation.GetTypeByMetadataName(World + "HoldsAttribute");
        var constants = promises.GetTypeMembers()
            .SelectMany(group => group.GetMembers().OfType<IFieldSymbol>())
            .Where(constant => constant.IsConst && constant.ConstantValue is string)
            .ToImmutableArray();
        var taken = new ConcurrentBag<(IFieldSymbol Promise, IMethodSymbol Test, Location Mark)>();

        start.RegisterSymbolAction(read => ReadTest(read, test, failsFor), SymbolKind.Method);
        start.RegisterSymbolAction(read => ReadFixture(read, test), SymbolKind.NamedType);
        start.RegisterOperationAction(read => ReadMark(read, test, failsFor, promises, taken), OperationKind.Attribute);
        if (holds is not null) start.RegisterOperationBlockAction(read => ReadGates(read, test, failsFor, holds, constants));
        start.RegisterCompilationEndAction(read => ReadEveryTest(read, promises, constants, taken));
    }

    private static void ReadTest(SymbolAnalysisContext context, INamedTypeSymbol test, INamedTypeSymbol failsFor)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (!TestCode.IsATest(method, test)) return;

        if (!method.GetAttributes().Any(applied => Is(applied.AttributeClass, failsFor)))
            context.ReportDiagnostic(Diagnostic.Create(NoMark, method.Locations[0], method.Name));
    }

    private static void ReadFixture(SymbolAnalysisContext context, INamedTypeSymbol test)
    {
        var fixture = (INamedTypeSymbol)context.Symbol;
        if (!fixture.GetMembers().OfType<IMethodSymbol>().Any(method => TestCode.IsATest(method, test))) return;

        var name = fixture.Name;
        var namedForItsSituation = name.Length > When.Length && name.StartsWith(When, StringComparison.Ordinal) && char.IsUpper(name[When.Length]);
        if (!namedForItsSituation) context.ReportDiagnostic(Diagnostic.Create(Fixture, fixture.Locations[0], name));
    }

    private static void ReadMark(
        OperationAnalysisContext context, INamedTypeSymbol test, INamedTypeSymbol failsFor, INamedTypeSymbol promises,
        ConcurrentBag<(IFieldSymbol Promise, IMethodSymbol Test, Location Mark)> taken)
    {
        var applied = (IAttributeOperation)context.Operation;
        if (applied.Operation is not IObjectCreationOperation created || !Is(created.Constructor?.ContainingType, failsFor)) return;
        if (context.ContainingSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test)) return;

        var named = Unwrapped(created.Arguments[0].Value);
        if (named is IFieldReferenceOperation { Field: { IsConst: true } promise } && IsOf(promise, promises))
        {
            taken.Add((promise, method, applied.Syntax.GetLocation()));
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(NoConstant, named.Syntax.GetLocation(), method.Name));
    }

    private static void ReadGates(
        OperationBlockAnalysisContext context, INamedTypeSymbol test, INamedTypeSymbol failsFor, INamedTypeSymbol holds, ImmutableArray<IFieldSymbol> constants)
    {
        if (context.OwningSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test)) return;

        var carried = method.GetAttributes().Where(applied => Is(applied.AttributeClass, failsFor)).ToList();
        if (carried.Count != 1 || carried[0].ConstructorArguments.Length != 1 || carried[0].ConstructorArguments[0].Value is not string value) return;

        var promise = constants.FirstOrDefault(constant => (string)constant.ConstantValue! == value);
        if (promise is null) return;

        var named = new HashSet<IFieldSymbol>(
            context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf()).OfType<IFieldReferenceOperation>().Select(read => read.Field),
            SymbolEqualityComparer.Default);
        foreach (var gate in GatesOf(promise, holds).Where(gate => !named.Contains(gate)))
            context.ReportDiagnostic(Diagnostic.Create(Gate, method.Locations[0], method.Name, gate.ContainingType.Name + "." + gate.Name));
    }

    private static void ReadEveryTest(
        CompilationAnalysisContext context, INamedTypeSymbol promises, ImmutableArray<IFieldSymbol> constants,
        ConcurrentBag<(IFieldSymbol Promise, IMethodSymbol Test, Location Mark)> taken)
    {
        var byPromise = taken.GroupBy(carried => carried.Promise, (IEqualityComparer<IFieldSymbol>)SymbolEqualityComparer.Default)
            .ToDictionary(tests => tests.Key, tests => tests.OrderBy(carried => carried.Mark.SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(carried => carried.Mark.SourceSpan.Start).ToList(), (IEqualityComparer<IFieldSymbol>)SymbolEqualityComparer.Default);

        foreach (var tests in byPromise.Values.Where(tests => tests.Count > 1))
        {
            foreach (var later in tests.Skip(1))
                context.ReportDiagnostic(Diagnostic.Create(Twice, later.Mark, later.Test.Name, NameOf(later.Promise, promises), tests[0].Test.Name));
        }

        var written = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(StillBeingWritten, out var still) && still.Trim() == "true";
        if (written) return;

        foreach (var promise in constants.Where(constant => !byPromise.ContainsKey(constant)).Select(constant => NameOf(constant, promises)).OrderBy(name => name, StringComparer.Ordinal))
            context.ReportDiagnostic(Diagnostic.Create(NoTest, Location.None, promise));
    }

    private static IEnumerable<IFieldSymbol> GatesOf(IFieldSymbol promise, INamedTypeSymbol holds)
    {
        foreach (var mark in promise.GetAttributes().Where(applied => Is(applied.AttributeClass, holds) && applied.ConstructorArguments.Length == 1))
        {
            var held = mark.ConstructorArguments[0];
            if (held.Kind != TypedConstantKind.Enum || held.Type is not INamedTypeSymbol gates) continue;

            var gate = gates.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(member => member.HasConstantValue && Equals(member.ConstantValue, held.Value));
            if (gate is not null) yield return gate;
        }
    }

    private static IOperation Unwrapped(IOperation value)
    {
        while (value is IConversionOperation { IsImplicit: true } converted) value = converted.Operand;
        return value;
    }

    private static bool IsOf(IFieldSymbol constant, INamedTypeSymbol promises)
    {
        for (var holder = constant.ContainingType?.ContainingType; holder is not null; holder = holder.ContainingType)
            if (Is(holder, promises)) return true;
        return false;
    }

    private static string NameOf(IFieldSymbol promise, INamedTypeSymbol promises) => promises.Name + "." + promise.ContainingType.Name + "." + promise.Name;

    private static bool Is(ITypeSymbol? type, INamedTypeSymbol expected) => SymbolEqualityComparer.Default.Equals(type, expected);
}
