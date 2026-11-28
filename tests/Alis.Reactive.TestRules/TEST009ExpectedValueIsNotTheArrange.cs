using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST009ExpectedValueIsNotTheArrange : DiagnosticAnalyzer
{
    public const string Id = "TEST009";

    private const string Fill = "FillAsync";
    private const string PressSequentially = "PressSequentiallyAsync";

    private static readonly DiagnosticDescriptor Arrange = new(
        Id,
        "An expected value is not what the test typed",
        "'{0}' expects \"{1}\", which it typed into the same locator; expect what the page makes of it",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Arrange);

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
        if (test is null) return;

        start.RegisterOperationBlockAction(block => ReadATest(block, test));
    }

    private static void ReadATest(OperationBlockAnalysisContext context, INamedTypeSymbol test)
    {
        if (context.OwningSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test)) return;

        var calls = context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf()).OfType<IInvocationOperation>()
            .Where(called => TestCode.IsOfPlaywright(called.TargetMethod))
            .ToList();
        var typed = calls.SelectMany(Typed).ToList();

        foreach (var matcher in calls.Where(called => Taken.Expecting.Contains(Taken.NameOf(called.TargetMethod))))
        {
            if (Expected(matcher.Instance) is not { } locator) continue;

            foreach (var expected in matcher.Arguments.SelectMany(given => Constants(given.Value)))
            {
                if (typed.Any(arranged => Equals(arranged.Text, expected.ConstantValue.Value) && IsTheSame(arranged.Into, locator)))
                    context.ReportDiagnostic(Diagnostic.Create(Arrange, expected.Syntax.GetLocation(), method.Name, expected.ConstantValue.Value));
            }
        }
    }

    private static IEnumerable<(IOperation Into, string Text)> Typed(IInvocationOperation called)
    {
        var member = called.TargetMethod;
        if (member.Name is not (Fill or PressSequentially) || !Taken.Gestures.Contains(Taken.NameOf(member)) || called.Instance is not { } into) yield break;

        var given = called.Arguments.FirstOrDefault(given => given.Parameter?.Ordinal == 0);
        if (given?.Value.ConstantValue.Value is string text) yield return (into, text);
    }

    private static IOperation? Expected(IOperation? reached) => reached switch
    {
        IPropertyReferenceOperation through => Expected(through.Instance),
        IInvocationOperation expect when Taken.Expecting.Contains(Taken.NameOf(expect.TargetMethod)) => expect.Arguments.FirstOrDefault()?.Value,
        _ => null,
    };

    private static IEnumerable<IOperation> Constants(IOperation given)
    {
        var value = TestCode.Written(given);
        return value switch
        {
            { ConstantValue.HasValue: true } => [value],
            ICollectionExpressionOperation listed => listed.Elements.SelectMany(Constants),
            IArrayCreationOperation { Initializer: { } listed } => listed.ElementValues.SelectMany(Constants),
            _ => [],
        };
    }

    private static bool IsTheSame(IOperation typedInto, IOperation expected) => (typedInto, expected) switch
    {
        (IInstanceReferenceOperation, IInstanceReferenceOperation) => true,
        (ILocalReferenceOperation one, ILocalReferenceOperation other) => SymbolEqualityComparer.Default.Equals(one.Local, other.Local),
        (IFieldReferenceOperation one, IFieldReferenceOperation other) => IsTheSame(one.Field, one.Instance, other.Field, other.Instance),
        (IPropertyReferenceOperation one, IPropertyReferenceOperation other) => IsTheSame(one.Property, one.Instance, other.Property, other.Instance),
        var (one, other) => SyntaxFactory.AreEquivalent(one.Syntax, other.Syntax),
    };

    private static bool IsTheSame(ISymbol member, IOperation? of, ISymbol otherMember, IOperation? ofOther)
        => SymbolEqualityComparer.Default.Equals(member, otherMember) && (of is null || ofOther is null || IsTheSame(of, ofOther));
}
