using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST008PageIsMatchedWhole : DiagnosticAnalyzer
{
    public const string Id = "TEST008";

    private const string Title = "A page is matched by its whole text, never by a part";
    private const string NetsRegex = "System.Text.RegularExpressions.Regex";

    private static readonly DiagnosticDescriptor InPart = new(
        Id,
        Title,
        "'{0}' matches a part of the text; expect the whole text with ToHaveTextAsync",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PassedAPattern = new(
        Id,
        Title,
        "'{0}' is passed a Regex, which matches a part; pass the whole text as a literal",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SetToAPattern = new(
        Id,
        Title,
        "'{0}' is set to a Regex, which matches a part; set the option that takes the whole text",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(InPart, PassedAPattern, SetToAPattern);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        if (start.Compilation.GetTypeByMetadataName(NetsRegex) is not { } regex) return;

        start.RegisterOperationAction(called => ReadCall(called, regex), OperationKind.Invocation);
        start.RegisterOperationAction(assigned => ReadAssignment(assigned, regex), OperationKind.SimpleAssignment);
    }

    private static void ReadCall(OperationAnalysisContext context, INamedTypeSymbol regex)
    {
        var called = (IInvocationOperation)context.Operation;
        var method = called.TargetMethod;
        if (!TestCode.IsOfPlaywright(method)) return;

        if (method.Name == Taken.PartialMatch)
            context.ReportDiagnostic(Diagnostic.Create(InPart, TestCode.NameOf(called), Taken.NameOf(method)));

        foreach (var pattern in called.Arguments.Where(argument => Holds(argument.Parameter?.Type, regex)))
            context.ReportDiagnostic(Diagnostic.Create(PassedAPattern, pattern.Value.Syntax.GetLocation(), Taken.NameOf(method)));
    }

    private static void ReadAssignment(OperationAnalysisContext context, INamedTypeSymbol regex)
    {
        var assigned = (ISimpleAssignmentOperation)context.Operation;
        if (assigned.Target is IPropertyReferenceOperation { Property: var option } target && TestCode.IsOfPlaywright(option) && Is(option.Type, regex))
            context.ReportDiagnostic(Diagnostic.Create(SetToAPattern, TestCode.NameOf(target), Taken.NameOf(option)));
    }

    private static bool Holds(ITypeSymbol? taken, INamedTypeSymbol regex)
        => Is(taken, regex) || (taken is INamedTypeSymbol { TypeArguments: var held } && held.Any(one => Is(one, regex)));

    private static bool Is(ITypeSymbol? type, INamedTypeSymbol regex) => SymbolEqualityComparer.Default.Equals(type, regex);
}
