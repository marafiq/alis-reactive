using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST003TestWaitsForNoTime : DiagnosticAnalyzer
{
    public const string Id = "TEST003";

    private const string Title = "A test waits for the state it names, never for time";
    private const string Delay = "Delay";
    private const string Sleep = "Sleep";
    private const string SpinWait = "SpinWait";
    private const string Timeout = "Timeout";

    private static readonly DiagnosticDescriptor Wait = new(
        Id,
        Title,
        "'{0}' waits on time; wait for the state the promise names",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ChosenWait = new(
        Id,
        Title,
        "'{0}' sets its own Timeout; every wait is the harness's, set from measurement, so remove it",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Wait, ChosenWait);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var task = start.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var thread = start.Compilation.GetTypeByMetadataName("System.Threading.Thread");
        var spinWait = start.Compilation.GetTypeByMetadataName("System.Threading.SpinWait");
        if (task is null || thread is null || spinWait is null) return;

        start.RegisterOperationAction(called => ReadCall(called, task, thread, spinWait), OperationKind.Invocation);
        start.RegisterOperationAction(ReadAssignment, OperationKind.SimpleAssignment);
    }

    private static void ReadCall(OperationAnalysisContext context, INamedTypeSymbol task, INamedTypeSymbol thread, INamedTypeSymbol spinWait)
    {
        var called = (IInvocationOperation)context.Operation;
        var method = called.TargetMethod;
        var holder = method.ContainingType;
        var waits = (method.Name == Taken.WaitOnTime && TestCode.IsOfPlaywright(method))
                    || (method.Name == Delay && Is(holder, task))
                    || (method.Name is Sleep or SpinWait && Is(holder, thread))
                    || Is(holder, spinWait);
        if (waits) context.ReportDiagnostic(Diagnostic.Create(Wait, TestCode.NameOf(called), holder.Name + "." + method.Name));
    }

    private static void ReadAssignment(OperationAnalysisContext context)
    {
        var assigned = (ISimpleAssignmentOperation)context.Operation;
        if (assigned.Target is not IPropertyReferenceOperation { Property: { Name: Timeout } property } target) return;
        if (!TestCode.IsOfPlaywright(property) || TestCode.IsInTheHarness(context.Operation.Syntax.SyntaxTree)) return;

        var written = target.Syntax is MemberAccessExpressionSyntax reached ? reached.Name.GetLocation() : target.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostic.Create(ChosenWait, written, property.ContainingType.Name));
    }

    private static bool Is(INamedTypeSymbol holder, INamedTypeSymbol type) => SymbolEqualityComparer.Default.Equals(holder.OriginalDefinition, type);
}
