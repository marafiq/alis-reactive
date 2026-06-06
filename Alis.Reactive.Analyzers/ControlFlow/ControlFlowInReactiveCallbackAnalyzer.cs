using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.ControlFlow
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ControlFlowInReactiveCallbackAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableHashSet<SyntaxKind> AllowedStatementKinds =
            ImmutableHashSet.Create(
                SyntaxKind.ExpressionStatement,
                SyntaxKind.LocalDeclarationStatement
            );

        private static readonly ImmutableHashSet<SyntaxKind> FlaggedExpressionKinds =
            ImmutableHashSet.Create(
                SyntaxKind.ConditionalExpression,
                SyntaxKind.SwitchExpression
            );

        private static readonly ImmutableDictionary<SyntaxKind, string> Labels =
            new Dictionary<SyntaxKind, string>
            {
                [SyntaxKind.IfStatement] = "if/else",
                [SyntaxKind.SwitchStatement] = "switch",
                [SyntaxKind.ForStatement] = "for loop",
                [SyntaxKind.ForEachStatement] = "foreach loop",
                [SyntaxKind.WhileStatement] = "while loop",
                [SyntaxKind.DoStatement] = "do-while loop",
                [SyntaxKind.GotoStatement] = "goto",
                [SyntaxKind.LabeledStatement] = "goto label",
                [SyntaxKind.TryStatement] = "try/catch",
                [SyntaxKind.ThrowStatement] = "throw",
                [SyntaxKind.LockStatement] = "lock",
                [SyntaxKind.UsingStatement] = "using",
                [SyntaxKind.ReturnStatement] = "return",
                [SyntaxKind.LocalFunctionStatement] = "local function",
                [SyntaxKind.ConditionalExpression] = "ternary ?:",
                [SyntaxKind.SwitchExpression] = "switch expression",
            }.ToImmutableDictionary();

        private static readonly Regex PascalCaseRegex = new Regex(
            "([a-z])([A-Z])", RegexOptions.Compiled);

        public const string DiagnosticId = "ALIS004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Imperative C# in reactive callback",
            messageFormat: "'{0}' is not allowed inside a reactive callback \u2014 only DSL method calls and variable declarations are permitted",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Reactive callbacks build Reactive Plan entries at render time \u2014 they do not execute at runtime. " +
                         "Use p.When(...).Then(...).Else(...) for conditions.",
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationCtx =>
            {
                var pipelineBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.PipelineBuilder`1");
                var triggerBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.TriggerBuilder`1");

                if (pipelineBuilderType == null && triggerBuilderType == null)
                    return;

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeLambda(nodeCtx, pipelineBuilderType, triggerBuilderType),
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression);
            });
        }

        private static void AnalyzeLambda(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol? pipelineBuilderType,
            INamedTypeSymbol? triggerBuilderType)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (!AnalyzerHelpers.IsRazorGeneratedFile(lambda.SyntaxTree))
                return;

            if (!HasAnalyzedParameter(lambda, context.SemanticModel,
                pipelineBuilderType, triggerBuilderType, context.CancellationToken))
                return;

            if (lambda.Body is BlockSyntax block)
            {
                foreach (var statement in block.Statements)
                {
                    if (!AllowedStatementKinds.Contains(statement.Kind()))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, statement.GetLocation(), GetLabel(statement.Kind())));
                    }
                    else
                    {
                        ScanForFlaggedExpressions(statement, context);
                    }
                }
            }
            else
            {
                ScanForFlaggedExpressions(lambda.Body, context);
            }
        }

        private static void ScanForFlaggedExpressions(
            SyntaxNode root, SyntaxNodeAnalysisContext context)
        {
            foreach (var node in root.DescendantNodesAndSelf(ShouldDescendInto))
            {
                if (FlaggedExpressionKinds.Contains(node.Kind()))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(Rule, node.GetLocation(), GetLabel(node.Kind())));
                }
            }
        }

        private static bool ShouldDescendInto(SyntaxNode node)
        {
            return !(node is LambdaExpressionSyntax)
                && !(node is AnonymousMethodExpressionSyntax);
        }

        private static bool HasAnalyzedParameter(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            INamedTypeSymbol? pipelineBuilderType,
            INamedTypeSymbol? triggerBuilderType,
            System.Threading.CancellationToken cancellationToken)
        {
            if (lambda is SimpleLambdaExpressionSyntax simple)
            {
                var symbol = semanticModel.GetDeclaredSymbol(simple.Parameter, cancellationToken);
                return symbol != null && IsAnalyzedType(symbol.Type, pipelineBuilderType, triggerBuilderType);
            }

            if (lambda is ParenthesizedLambdaExpressionSyntax parens)
            {
                foreach (var param in parens.ParameterList.Parameters)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(param, cancellationToken);
                    if (symbol != null && IsAnalyzedType(symbol.Type, pipelineBuilderType, triggerBuilderType))
                        return true;
                }
            }

            return false;
        }

        private static bool IsAnalyzedType(
            ITypeSymbol? type,
            INamedTypeSymbol? pipelineBuilderType,
            INamedTypeSymbol? triggerBuilderType)
        {
            return AnalyzerHelpers.IsClosedGenericOf(type, pipelineBuilderType)
                || AnalyzerHelpers.IsClosedGenericOf(type, triggerBuilderType);
        }

        private static string GetLabel(SyntaxKind kind)
        {
            return Labels.TryGetValue(kind, out var label)
                ? label
                : FormatKindName(kind);
        }

        private static string FormatKindName(SyntaxKind kind)
        {
            var name = kind.ToString();
            var spaced = PascalCaseRegex.Replace(name, "$1 $2");
            return spaced.ToLowerInvariant();
        }
    }
}
