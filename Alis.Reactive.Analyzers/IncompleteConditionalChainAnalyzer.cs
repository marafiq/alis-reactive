using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Alis.Reactive.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IncompleteConditionalChainAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Incomplete conditional chain",
            messageFormat: "Incomplete conditional chain — call .Then() to complete the condition",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A conditional or guard chain was started but never completed with .Then(). " +
                         "The condition will not be included in the plan.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
        }

        private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (ExpressionStatementSyntax)context.Node;
            if (!IsRazorGeneratedFile(statement.SyntaxTree))
                return;

            var typeInfo = context.SemanticModel.GetTypeInfo(statement.Expression, context.CancellationToken);
            var type = typeInfo.Type;

            if (type == null) return;

            // Check if the return type is GuardBuilder<> or ConditionSourceBuilder<,>
            if (!IsDanglingBuilderType(type)) return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, statement.Expression.GetLocation()));
        }

        private static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDanglingBuilderType(ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;

            var original = named.ConstructedFrom;
            var fullName = original.ToDisplayString();

            return fullName == "Alis.Reactive.Builders.Conditions.GuardBuilder<TModel>"
                || fullName == "Alis.Reactive.Builders.Conditions.ConditionSourceBuilder<TModel, TProp>";
        }
    }
}
