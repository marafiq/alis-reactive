using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST002TestTakesTheListedMembersOfPlaywright : DiagnosticAnalyzer
{
    public const string Id = "TEST002";

    private const string Title = "A browser test takes only the listed members of Playwright";
    private const char OpensAnId = '#';

    private static readonly ImmutableArray<string> ReadBehindThePage = ImmutableArray.Create("ej2_instances", "window.alis");

    private static readonly ImmutableHashSet<string> ATestTakes =
        Taken.Gestures.Union(Taken.Finding).Union(Taken.Expecting).Union(Taken.BesideThePage).Union(Taken.Ways);

    private static readonly DiagnosticDescriptor Member = new(
        Id,
        Title,
        "'{0}' is outside the members a test takes of Playwright; find an element by its role, label, text or id, act by a gesture and expect through Expect",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OfTheHarness = new(
        Id,
        Title,
        "'{0}' is the harness's to take; leave it to a file under Harness",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Selector = new(
        Id,
        Title,
        "'{0}' is no single id; a locator takes '#' and one id, or the element is found by its role, label or text",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Named = new(
        Id,
        Title,
        "This text names '{0}'; a test reads no component's object and no global of the runtime, so expect what the page shows",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Member, OfTheHarness, Selector, Named);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        start.RegisterOperationAction(ReadText, OperationKind.Literal);
        start.RegisterOperationAction(ReadCall, OperationKind.Invocation);
        start.RegisterOperationAction(ReadMember, OperationKind.PropertyReference, OperationKind.EventReference, OperationKind.MethodReference);
    }

    private static void ReadCall(OperationAnalysisContext context)
    {
        var called = (IInvocationOperation)context.Operation;
        var method = called.TargetMethod;
        if (!IsRead(method) || method.Name is Taken.WaitOnTime or Taken.PartialMatch) return;

        if (RefusalOf(method, called.Syntax.SyntaxTree) is { } refusal)
            context.ReportDiagnostic(Diagnostic.Create(refusal, TestCode.NameOf(called), Taken.NameOf(method)));
        else if (Taken.TakingASelector.Contains(Taken.NameOf(method)))
            ReadSelector(context, called);
    }

    private static void ReadSelector(OperationAnalysisContext context, IInvocationOperation called)
    {
        var selector = called.Arguments.FirstOrDefault(argument => argument.Parameter is { Ordinal: 0 });
        if (selector?.Value.ConstantValue is { HasValue: true, Value: string written } && !IsOneId(written))
            context.ReportDiagnostic(Diagnostic.Create(Selector, selector.Value.Syntax.GetLocation(), written));
    }

    private static void ReadMember(OperationAnalysisContext context)
    {
        var read = (IMemberReferenceOperation)context.Operation;
        if (!IsRead(read.Member)) return;

        if (RefusalOf(read.Member, read.Syntax.SyntaxTree) is { } refusal)
            context.ReportDiagnostic(Diagnostic.Create(refusal, TestCode.NameOf(read), Taken.NameOf(read.Member)));
    }

    private static void ReadText(OperationAnalysisContext context)
    {
        if (context.Operation.ConstantValue is not { HasValue: true, Value: string text }) return;

        foreach (var name in ReadBehindThePage.Where(text.Contains))
            context.ReportDiagnostic(Diagnostic.Create(Named, context.Operation.Syntax.GetLocation(), name));
    }

    private static bool IsRead(ISymbol member)
        => TestCode.IsOfPlaywright(member) && member.ContainingType is { TypeKind: TypeKind.Interface } or { IsStatic: true };

    private static DiagnosticDescriptor? RefusalOf(ISymbol member, SyntaxTree writtenIn)
    {
        var name = Taken.NameOf(member);
        if (ATestTakes.Contains(name)) return null;
        if (!Taken.ByTheHarness.Contains(name)) return Member;
        return TestCode.IsInTheHarness(writtenIn) ? null : OfTheHarness;
    }

    private static bool IsOneId(string selector)
        => selector.Length > 1 && selector[0] == OpensAnId && selector.Skip(1).All(part => char.IsLetterOrDigit(part) || part is '_' or '-');
}
