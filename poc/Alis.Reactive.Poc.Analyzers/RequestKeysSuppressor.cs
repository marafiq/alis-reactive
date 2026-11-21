using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequestKeysSuppressor : DiagnosticSuppressor
{
    public const string KeyPastANullHolderId = "ALIS036";

    private const string Dereference = "CS8602";

    private static readonly SuppressionDescriptor KeyPastANullHolder = new(KeyPastANullHolderId, Dereference, "A request's member lambda names the key it sends by its C# path and is never run, so a member past a holder that holds null needs no '!'");

    private static readonly (string Type, string Method)[] KeyDoors =
    [
        ("Alis.Reactive.Pipelines.QueryParts`1", "Query"),
        ("Alis.Reactive.Pipelines.CondensedRequests", "Set"),
    ];

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = [KeyPastANullHolder];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var doors = KeyDoors
            .Select(door => (Type: context.Compilation.GetTypeByMetadataName(door.Type), door.Method))
            .Where(door => door.Type is not null)
            .Select(door => (Type: door.Type!, door.Method))
            .ToList();
        if (doors.Count == 0) return;
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (NamesAKey(diagnostic, doors, context)) context.ReportSuppression(Suppression.Create(KeyPastANullHolder, diagnostic));
        }
    }

    private static bool NamesAKey(Diagnostic diagnostic, List<(INamedTypeSymbol Type, string Method)> doors, SuppressionAnalysisContext context)
    {
        if (diagnostic.Location.SourceTree is not { } tree) return false;
        var held = tree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (held.FirstAncestorOrSelf<LambdaExpressionSyntax>() is not SimpleLambdaExpressionSyntax lambda) return false;
        if (lambda.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }) return false;
        var model = context.GetSemanticModel(tree);
        if (model.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol called) return false;
        var method = (called.ReducedFrom ?? called).OriginalDefinition;
        var isAKeyDoor = doors.Any(door => method.Name == door.Method && SymbolEqualityComparer.Default.Equals(method.ContainingType, door.Type));
        if (!isAKeyDoor) return false;
        var key = model.GetDeclaredSymbol(lambda.Parameter, context.CancellationToken);
        return held.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(name =>
            SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name, context.CancellationToken).Symbol, key));
    }
}
