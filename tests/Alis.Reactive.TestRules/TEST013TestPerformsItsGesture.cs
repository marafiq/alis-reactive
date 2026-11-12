using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST013TestPerformsItsGesture : DiagnosticAnalyzer
{
    public const string Id = "TEST013";

    private const string WorldsGesture = "Alis.Reactive.Conformance.World.GestureAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "A test performs its gesture itself",
        "'{0}' performs no gesture itself; a test makes the gesture its promise names in its own body, so write it there",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Rule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        if (start.Compilation.GetTypeByMetadataName(TestCode.NUnitsTest) is not { } test) return;

        var gesture = start.Compilation.GetTypeByMetadataName(WorldsGesture);
        start.RegisterOperationBlockStartAction(body => ReadTest(body, test, gesture));
    }

    private static void ReadTest(OperationBlockStartAnalysisContext body, INamedTypeSymbol test, INamedTypeSymbol? gesture)
    {
        if (body.OwningSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test)) return;

        var settled = 0;
        body.RegisterOperationAction(
            called =>
            {
                if (IsAGesture(((IInvocationOperation)called.Operation).TargetMethod, gesture)) Interlocked.Exchange(ref settled, 1);
            },
            OperationKind.Invocation);
        body.RegisterOperationAction(_ => Interlocked.Exchange(ref settled, 1), OperationKind.Invalid);
        body.RegisterOperationBlockEndAction(end =>
        {
            if (Volatile.Read(ref settled) == 0) end.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
        });
    }

    private static bool IsAGesture(IMethodSymbol called, INamedTypeSymbol? gesture)
        => (TestCode.IsOfPlaywright(called) && Taken.Gestures.Contains(Taken.NameOf(called)))
           || called.GetAttributes().Any(applied => applied.AttributeClass is { } mark && SymbolEqualityComparer.Default.Equals(mark, gesture));
}
