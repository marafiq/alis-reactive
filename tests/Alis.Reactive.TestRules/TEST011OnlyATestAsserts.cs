using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST011OnlyATestAsserts : DiagnosticAnalyzer
{
    public const string Id = "TEST011";

    private const string NUnitsAssert = "NUnit.Framework.Assert";
    private const string PlaywrightsAssertions = "Microsoft.Playwright.Assertions";
    private const string Expect = "Expect";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Only a test method asserts",
        "'{0}' asserts with '{1}' and is no test; a helper builds a page or performs a gesture, so move the assertion into the test it proves",
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

        var assert = start.Compilation.GetTypeByMetadataName(NUnitsAssert);
        var assertions = start.Compilation.GetTypeByMetadataName(PlaywrightsAssertions);
        start.RegisterOperationAction(called => Read(called, test, assert, assertions), OperationKind.Invocation);
    }

    private static void Read(OperationAnalysisContext context, INamedTypeSymbol test, INamedTypeSymbol? assert, INamedTypeSymbol? assertions)
    {
        var called = (IInvocationOperation)context.Operation;
        var method = called.TargetMethod;
        var holder = method.ContainingType.OriginalDefinition;
        var asserted = SymbolEqualityComparer.Default.Equals(holder, assert) ? holder.Name + "." + method.Name
            : method.Name == Expect && SymbolEqualityComparer.Default.Equals(holder, assertions) ? Expect
            : null;
        if (asserted is null) return;

        if (context.ContainingSymbol is IMethodSymbol around && TestCode.IsATest(around, test)) return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, TestCode.NameOf(called), Named(context.ContainingSymbol), asserted));
    }

    private static string Named(ISymbol member) => member switch
    {
        IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructor => constructor.ContainingType.Name,
        IMethodSymbol { AssociatedSymbol: { } property } => property.Name,
        _ => member.Name,
    };
}
