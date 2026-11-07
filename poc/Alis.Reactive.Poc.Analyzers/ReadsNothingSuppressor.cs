using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReadsNothingSuppressor : DiagnosticSuppressor
{
    public const string PastANullHolderId = "ALIS026";
    public const string ValueOfANullId = "ALIS027";

    private const string Dereference = "CS8602";

    private static readonly SuppressionDescriptor PastANullHolder = new(PastANullHolderId, Dereference, "A data lambda reads its item in the browser, where a member read past a holder that holds null reads nothing, so the read needs no '!'");

    private static readonly SuppressionDescriptor ValueOfANull = new(ValueOfANullId, "CS8629", "A data lambda reads its item in the browser, where a nullable's Value, or a cast to it, reads nothing on a nullable that holds null, so the read needs no '!'");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = [PastANullHolder, ValueOfANull];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        var doors = DataDoors.Of(context.Compilation);
        if (doors is null) return;
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var dereferences = diagnostic.Id == Dereference;
            if (ReadsNothing(diagnostic, dereferences, doors, context)) context.ReportSuppression(Suppression.Create(dereferences ? PastANullHolder : ValueOfANull, diagnostic));
        }
    }

    private static bool ReadsNothing(Diagnostic diagnostic, bool dereferences, DataDoors doors, SuppressionAnalysisContext context)
    {
        if (diagnostic.Location.SourceTree is not { } tree) return false;
        var root = tree.GetRoot(context.CancellationToken);
        if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not ExpressionSyntax held) return false;
        var readsAValue = held.Parent is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Value" } access && access.Expression == held;
        if (!dereferences && !readsAValue && held is not CastExpressionSyntax) return false;
        var model = context.GetSemanticModel(tree);
        var items = doors.ItemsAround(held, model, context.CancellationToken);
        return items.Count > 0 && ReadsAnItem(held, items, model, context.CancellationToken);
    }

    private static bool ReadsAnItem(ExpressionSyntax held, List<IParameterSymbol> items, SemanticModel model, CancellationToken cancellation)
        => held.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(name =>
            model.GetSymbolInfo(name, cancellation).Symbol is IParameterSymbol parameter && items.Contains(parameter, SymbolEqualityComparer.Default));

    private enum Door
    {
        None,
        ReadsNothing,
        Stops,
        Predicate,
    }

    private sealed class DataDoors
    {
        private static readonly ImmutableHashSet<string> ReadingNothing = ["Count", "Where", "Select", "Any", "All", "Find"];
        private static readonly ImmutableHashSet<string> Stopping = ["Sum", "OrderBy", "OrderByDescending"];
        private static readonly ImmutableHashSet<string> Predicates = ["Any", "All", "Count"];
        private static readonly string[] PathReaders = ["Alis.Reactive.Components.ReadOnly`1", "Alis.Reactive.Components.MethodResult`1", "Alis.Reactive.Pipelines.ResponseBody`1"];

        private readonly INamedTypeSymbol _lists;
        private readonly ImmutableArray<INamedTypeSymbol> _pathReaders;
        private readonly INamedTypeSymbol _enumerable;

        private DataDoors(INamedTypeSymbol lists, ImmutableArray<INamedTypeSymbol> pathReaders, INamedTypeSymbol enumerable)
        {
            _lists = lists;
            _pathReaders = pathReaders;
            _enumerable = enumerable;
        }

        public static DataDoors? Of(Compilation compilation)
        {
            var lists = compilation.GetTypeByMetadataName("Alis.Reactive.Lists.DataLists");
            var pathReaders = PathReaders.Select(compilation.GetTypeByMetadataName).ToList();
            var enumerable = compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if (lists is null || enumerable is null || pathReaders.Any(reader => reader is null)) return null;
            return new DataDoors(lists, [.. pathReaders.Select(reader => reader!)], enumerable);
        }

        public List<IParameterSymbol> ItemsAround(SyntaxNode held, SemanticModel model, CancellationToken cancellation)
        {
            var items = new List<IParameterSymbol>();
            var innermost = true;
            for (var lambda = held.FirstAncestorOrSelf<LambdaExpressionSyntax>(); lambda is not null; lambda = lambda.Parent?.FirstAncestorOrSelf<LambdaExpressionSyntax>())
            {
                items.AddRange(ParametersOf(lambda, model, cancellation));
                switch (DoorOf(lambda, model, cancellation))
                {
                    case Door.ReadsNothing:
                        return items;
                    case Door.Stops:
                        return innermost ? [] : items;
                    case Door.Predicate:
                        innermost = false;
                        continue;
                    default:
                        return [];
                }
            }

            return [];
        }

        private Door DoorOf(LambdaExpressionSyntax lambda, SemanticModel model, CancellationToken cancellation)
        {
            if (lambda.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }) return Door.None;
            if (model.GetSymbolInfo(invocation, cancellation).Symbol is not IMethodSymbol called) return Door.None;
            var method = (called.ReducedFrom ?? called).OriginalDefinition;
            var owner = method.ContainingType;
            if (SymbolEqualityComparer.Default.Equals(owner, _lists)) return ListDoor(method.Name);
            var readsAPath = method.Name == "Read" && _pathReaders.Contains(owner, SymbolEqualityComparer.Default);
            if (readsAPath) return Door.ReadsNothing;
            var predicate = SymbolEqualityComparer.Default.Equals(owner, _enumerable) && Predicates.Contains(method.Name);
            return predicate ? Door.Predicate : Door.None;
        }

        private static Door ListDoor(string name)
        {
            if (ReadingNothing.Contains(name)) return Door.ReadsNothing;
            return Stopping.Contains(name) ? Door.Stops : Door.None;
        }

        private static IEnumerable<IParameterSymbol> ParametersOf(LambdaExpressionSyntax lambda, SemanticModel model, CancellationToken cancellation)
        {
            IEnumerable<ParameterSyntax> parameters = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => [simple.Parameter],
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters,
                _ => [],
            };
            foreach (var parameter in parameters)
            {
                if (model.GetDeclaredSymbol(parameter, cancellation) is { } symbol) yield return symbol;
            }
        }
    }
}
