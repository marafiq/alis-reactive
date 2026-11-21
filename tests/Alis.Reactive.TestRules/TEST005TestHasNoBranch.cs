using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST005TestHasNoBranch : DiagnosticAnalyzer
{
    public const string Id = "TEST005";

    private const string Title = "A test has no branch and no loop";
    private const string Choice = "?:";
    private const string MemberIfPresent = "?.";
    private const string ElementIfPresent = "?[]";

    private static readonly DiagnosticDescriptor Branch = new(
        Id,
        Title,
        "'{0}' branches at '{1}'; a test has no conditional, so write the one path its promise names",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Loop = new(
        Id,
        Title,
        "'{0}' loops at '{1}'; a test has no loop, so write each step its promise names",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Branch, Loop);

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

        start.RegisterOperationAction(
            written => Read(written, test),
            OperationKind.Conditional,
            OperationKind.Switch,
            OperationKind.SwitchExpression,
            OperationKind.Loop,
            OperationKind.Try,
            OperationKind.Branch,
            OperationKind.Coalesce,
            OperationKind.CoalesceAssignment,
            OperationKind.ConditionalAccess,
            OperationKind.Binary);
    }

    private static void Read(OperationAnalysisContext context, INamedTypeSymbol test)
    {
        if (Written(context.Operation) is not (var at, var said)) return;
        if (context.ContainingSymbol is not IMethodSymbol method || !TestCode.IsATest(method, test) || !IsInTheBody(context.Operation)) return;

        context.ReportDiagnostic(Diagnostic.Create(context.Operation is ILoopOperation ? Loop : Branch, at.GetLocation(), method.Name, said));
    }

    private static (SyntaxToken At, string Said)? Written(IOperation operation) => operation switch
    {
        IConditionalOperation { Syntax: IfStatementSyntax written } => Keyword(written.IfKeyword),
        IConditionalOperation { Syntax: ConditionalExpressionSyntax written } => (written.QuestionToken, Choice),
        ISwitchOperation { Syntax: SwitchStatementSyntax written } => Keyword(written.SwitchKeyword),
        ISwitchExpressionOperation { Syntax: SwitchExpressionSyntax written } => Keyword(written.SwitchKeyword),
        ILoopOperation { Syntax: ForStatementSyntax written } => Keyword(written.ForKeyword),
        ILoopOperation { Syntax: CommonForEachStatementSyntax written } => Keyword(written.ForEachKeyword),
        ILoopOperation { Syntax: WhileStatementSyntax written } => Keyword(written.WhileKeyword),
        ILoopOperation { Syntax: DoStatementSyntax written } => Keyword(written.DoKeyword),
        ITryOperation { Syntax: TryStatementSyntax written } => Keyword(written.TryKeyword),
        IBranchOperation { Syntax: GotoStatementSyntax written } => Keyword(written.GotoKeyword),
        ICoalesceOperation { Syntax: BinaryExpressionSyntax written } => Keyword(written.OperatorToken),
        ICoalesceAssignmentOperation { Syntax: AssignmentExpressionSyntax written } => Keyword(written.OperatorToken),
        IConditionalAccessOperation { Syntax: ConditionalAccessExpressionSyntax written }
            => (written.OperatorToken, written.OperatorToken.GetNextToken().IsKind(SyntaxKind.OpenBracketToken) ? ElementIfPresent : MemberIfPresent),
        IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalAnd or BinaryOperatorKind.ConditionalOr, Syntax: BinaryExpressionSyntax written }
            => Keyword(written.OperatorToken),
        _ => null,
    };

    private static (SyntaxToken At, string Said) Keyword(SyntaxToken written) => (written, written.Text);

    private static bool IsInTheBody(IOperation operation)
    {
        var outermost = operation;
        while (outermost.Parent is { } parent) outermost = parent;
        return outermost.Kind == OperationKind.MethodBody;
    }
}
