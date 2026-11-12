using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST010TestAssertsAPresence : DiagnosticAnalyzer
{
    public const string Id = "TEST010";

    private const string Title = "A test asserts what is present";
    private const string NUnitsIs = "NUnit.Framework.Is";
    private const string Not = "Not";
    private const string ToBeHidden = "ToBeHiddenAsync";
    private const string ToBeEmpty = "ToBeEmptyAsync";
    private const string ToHaveCount = "ToHaveCountAsync";
    private const string Empty = "Empty";
    private const string Null = "Null";

    private static readonly DiagnosticDescriptor OnlyAbsence = new(
        Id,
        Title,
        "'{0}' asserts only what is absent; assert what the page shows when the promise holds",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NoAssertion = new(
        Id,
        Title,
        "'{0}' asserts nothing; assert what the page shows when the promise holds",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(OnlyAbsence, NoAssertion);

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
        var assert = start.Compilation.GetTypeByMetadataName(TestCode.NUnitsAssert);
        var constraint = start.Compilation.GetTypeByMetadataName(TestCode.NUnitsConstraint);
        var nunitsIs = start.Compilation.GetTypeByMetadataName(NUnitsIs);
        if (test is null || assert is null || constraint is null || nunitsIs is null) return;

        start.RegisterOperationBlockAction(block => ReadATest(block, test, assert, constraint, nunitsIs));
    }

    private static void ReadATest(OperationBlockAnalysisContext context, INamedTypeSymbol test, INamedTypeSymbol assert, INamedTypeSymbol constraint, INamedTypeSymbol nunitsIs)
    {
        if (context.OwningSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test)) return;

        var written = context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf()).ToList();
        if (written.Any(operation => operation.Kind == OperationKind.Invalid)) return;

        var calls = written.OfType<IInvocationOperation>().ToList();
        var matchers = calls.Where(IsAMatcher).ToList();
        var checks = calls.Where(called => TestCode.IsAssertThat(called, assert)).ToList();
        var assertsAPresence = matchers.Any(matcher => !ExpectsAnAbsence(matcher)) || checks.Any(check => !ChecksAnAbsence(check, constraint, nunitsIs));
        if (assertsAPresence) return;

        var rule = matchers.Count + checks.Count == 0 ? NoAssertion : OnlyAbsence;
        context.ReportDiagnostic(Diagnostic.Create(rule, method.Locations[0], method.Name));
    }

    private static bool IsAMatcher(IInvocationOperation called)
        => called.Instance is not null && TestCode.IsOfPlaywright(called.TargetMethod) && Taken.Expecting.Contains(Taken.NameOf(called.TargetMethod));

    private static bool ExpectsAnAbsence(IInvocationOperation matcher)
        => matcher.Instance is IPropertyReferenceOperation { Property.Name: Not } || matcher.TargetMethod.Name switch
        {
            ToBeHidden or ToBeEmpty => true,
            ToHaveCount => matcher.Arguments.Any(given => given.Value.ConstantValue.Value is 0),
            _ => matcher.Arguments.Any(given => given.Value.ConstantValue.Value is ""),
        };

    private static bool ChecksAnAbsence(IInvocationOperation that, INamedTypeSymbol constraint, INamedTypeSymbol nunitsIs)
        => TestCode.ConstraintOf(that, constraint) is { } taken
           && TestCode.Written(taken) is IPropertyReferenceOperation { Property: { Name: Empty or Null } named }
           && SymbolEqualityComparer.Default.Equals(named.ContainingType, nunitsIs);
}
