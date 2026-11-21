using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST006AssertThatReadsBesideThePage : DiagnosticAnalyzer
{
    public const string Id = "TEST006";

    private const string NUnitsAssert = "NUnit.Framework.Assert";
    private const string That = "That";
    private const string TheMark = "Alis.Reactive.Conformance.World.BesideThePageAttribute";

    private static readonly DiagnosticDescriptor OffThePagesSide = new(
        Id,
        "Assert.That reads what the browser reports beside the page",
        "Assert.That reads '{0}', which the browser does not report beside the page; assert a request, a console message or a download where it is read, and the page through Expect",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(OffThePagesSide);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        if (start.Compilation.GetTypeByMetadataName(NUnitsAssert) is not { } assert) return;

        var mark = start.Compilation.GetTypeByMetadataName(TheMark);
        start.RegisterOperationAction(called => ReadAssertion(called, assert, mark), OperationKind.Invocation);
    }

    private static void ReadAssertion(OperationAnalysisContext context, INamedTypeSymbol assert, INamedTypeSymbol? mark)
    {
        var called = (IInvocationOperation)context.Operation;
        var method = called.TargetMethod;
        if (method.Name != That || !SymbolEqualityComparer.Default.Equals(method.ContainingType, assert)) return;

        if (called.Arguments.FirstOrDefault(argument => argument.Parameter is { Ordinal: 0 }) is { } read && !IsBesideThePage(read.Value, mark))
            context.ReportDiagnostic(Diagnostic.Create(OffThePagesSide, read.Value.Syntax.GetLocation(), read.Value.Syntax.ToString()));
    }

    private static bool IsBesideThePage(IOperation? read, INamedTypeSymbol? mark) => read switch
    {
        IConversionOperation converted => IsBesideThePage(converted.Operand, mark),
        IAwaitOperation awaited => IsBesideThePage(awaited.Operation, mark),
        IPropertyReferenceOperation property => IsReported(property.Property, mark) || IsBesideThePage(property.Instance, mark),
        IInvocationOperation called => IsReported(called.TargetMethod, mark) || IsBesideThePage(ReceiverOf(called), mark),
        _ => false,
    };

    private static IOperation? ReceiverOf(IInvocationOperation called)
        => called.Instance ?? (called.TargetMethod.IsExtensionMethod ? called.Arguments.FirstOrDefault(argument => argument.Parameter is { Ordinal: 0 })?.Value : null);

    private static bool IsReported(ISymbol member, INamedTypeSymbol? mark)
        => (TestCode.IsOfPlaywright(member) && Taken.ReadBesideThePage.Contains(Taken.NameOf(member)))
           || member.GetAttributes().Any(applied => SymbolEqualityComparer.Default.Equals(applied.AttributeClass, mark));
}
