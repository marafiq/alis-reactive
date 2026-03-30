using System;
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
        // ── DATA (OCP: extend by adding one line) ─────────────────

        private static readonly ImmutableHashSet<string> AnalyzedParameterTypes =
            ImmutableHashSet.Create(
                "Alis.Reactive.Builders.PipelineBuilder<TModel>",
                "Alis.Reactive.Builders.TriggerBuilder<TModel>"
            );

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

        // ── DIAGNOSTIC ────────────────────────────────────────────

        public const string DiagnosticId = "ALIS004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Imperative C# in reactive callback",
            messageFormat: "'{0}' is not allowed inside a reactive callback \u2014 only DSL method calls and variable declarations are permitted",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Reactive callbacks build descriptors at render time \u2014 they do not execute at runtime. " +
                         "Use p.When(...).Then(...).Else(...) for conditions.",
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        // ── PLUMBING ──────────────────────────────────────────────

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeLambda,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        // ── VALIDATION ────────────────────────────────────────────

        private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (!IsRazorGeneratedFile(lambda.SyntaxTree))
                return;

            if (!HasAnalyzedParameter(lambda, context.SemanticModel, context.CancellationToken))
                return;

            var body = lambda.Body;

            if (body is BlockSyntax block)
            {
                // Block-bodied: check each direct child statement
                foreach (var statement in block.Statements)
                {
                    if (!AllowedStatementKinds.Contains(statement.Kind()))
                    {
                        // Disallowed statement — report it, skip expression scan inside
                        // (avoids double-reporting ternary/switch-expr nested in flagged statements)
                        var label = GetLabel(statement.Kind());
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, statement.GetLocation(), label));
                    }
                    else
                    {
                        // Allowed statement — scan its descendants for flagged expressions
                        ScanForFlaggedExpressions(statement, context);
                    }
                }
            }
            else
            {
                // Expression-bodied: scan the entire body for flagged expressions
                ScanForFlaggedExpressions(body, context);
            }
        }

        private static void ScanForFlaggedExpressions(
            SyntaxNode root, SyntaxNodeAnalysisContext context)
        {
            foreach (var node in root.DescendantNodesAndSelf(ShouldDescendInto))
            {
                if (FlaggedExpressionKinds.Contains(node.Kind()))
                {
                    var label = GetLabel(node.Kind());
                    context.ReportDiagnostic(
                        Diagnostic.Create(Rule, node.GetLocation(), label));
                }
            }
        }

        private static bool ShouldDescendInto(SyntaxNode node)
        {
            // Do not descend into nested lambdas or anonymous methods —
            // they are analyzed independently if their parameter matches
            return !(node is LambdaExpressionSyntax)
                && !(node is AnonymousMethodExpressionSyntax);
        }

        private static bool HasAnalyzedParameter(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (lambda is SimpleLambdaExpressionSyntax simple)
            {
                var symbol = semanticModel.GetDeclaredSymbol(simple.Parameter, cancellationToken);
                return symbol != null && IsAnalyzedType(symbol.Type);
            }

            if (lambda is ParenthesizedLambdaExpressionSyntax parens)
            {
                foreach (var param in parens.ParameterList.Parameters)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(param, cancellationToken);
                    if (symbol != null && IsAnalyzedType(symbol.Type))
                        return true;
                }
            }

            return false;
        }

        private static bool IsAnalyzedType(ITypeSymbol? type)
        {
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;
            return AnalyzedParameterTypes.Contains(named.ConstructedFrom.ToDisplayString());
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

        private static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
