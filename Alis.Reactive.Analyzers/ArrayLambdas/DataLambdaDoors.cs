using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.ArrayLambdas;

internal sealed class DataLambdaDoors
{
    private const string Lists = "Alis.Reactive.ArrayLambdas.DataLists";
    private const string ReadOnlyProperties = "Alis.Reactive.Declarations.ReadOnly`1";
    private const string MethodResults = "Alis.Reactive.Declarations.MethodResult`1";
    private const string Linq = "System.Linq.Enumerable";
    private const string PathRead = "Read";
    private const string FieldSelected = "Select";

    private static readonly ImmutableArray<string> OperationsThatReadNothingPastNull = ImmutableArray.Create("Count", "Where", "Any", "All", "Find");
    private static readonly ImmutableArray<string> PredicatesThatNest = ImmutableArray.Create("Any", "All", "Count");

    private readonly ImmutableHashSet<ISymbol> _readNothingPastNull;
    private readonly ImmutableHashSet<ISymbol> _selectedFields;
    private readonly ImmutableHashSet<ISymbol> _stopPastNull;
    private readonly ImmutableHashSet<ISymbol> _nestedPredicates;

    private DataLambdaDoors(ImmutableHashSet<ISymbol> readNothingPastNull, ImmutableHashSet<ISymbol> selectedFields, ImmutableHashSet<ISymbol> stopPastNull, ImmutableHashSet<ISymbol> nestedPredicates)
    {
        _readNothingPastNull = readNothingPastNull;
        _selectedFields = selectedFields;
        _stopPastNull = stopPastNull;
        _nestedPredicates = nestedPredicates;
    }

    private enum Door
    {
        None,
        ReadsNothingPastNull,
        StopsPastNull,
        NestedPredicate,
    }

    internal static DataLambdaDoors? Of(Compilation compilation)
    {
        var lists = compilation.GetTypeByMetadataName(Lists);
        var properties = compilation.GetTypeByMetadataName(ReadOnlyProperties);
        var results = compilation.GetTypeByMetadataName(MethodResults);
        var linq = compilation.GetTypeByMetadataName(Linq);
        if (lists is null || properties is null || results is null || linq is null) return null;

        var paths = ParametersOf(properties, PathRead).Concat(ParametersOf(results, PathRead));
        var readNothingPastNull = Set(OperationsThatReadNothingPastNull.SelectMany(operation => ParametersOf(lists, operation)).Concat(paths));
        var selectedFields = Set(ParametersOf(lists, FieldSelected));
        var stopPastNull = Set(lists.GetMembers().OfType<IMethodSymbol>().SelectMany(operation => operation.Parameters)).Except(readNothingPastNull).Except(selectedFields);
        var nestedPredicates = Set(PredicatesThatNest.SelectMany(predicate => ParametersOf(linq, predicate)));
        return new DataLambdaDoors(readNothingPastNull, selectedFields, stopPastNull, nestedPredicates);
    }

    internal static IOperation? OperationAt(Diagnostic warning, SuppressionAnalysisContext context)
    {
        if (warning.Location.SourceTree is not { } tree) return null;
        var warned = tree.GetRoot(context.CancellationToken).FindNode(warning.Location.SourceSpan, getInnermostNodeForTie: true);
        return context.GetSemanticModel(tree).GetOperation(warned, context.CancellationToken);
    }

    internal bool ReadsNothingPastNull(IOperation read)
    {
        var items = ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default);
        var nestedInAPredicate = false;
        for (var lambda = Enclosing(read); lambda is not null; lambda = Enclosing(lambda))
        {
            items = items.Union(lambda.Symbol.Parameters);
            var door = DoorOf(lambda);
            if (door == Door.NestedPredicate)
            {
                nestedInAPredicate = true;
                continue;
            }

            var readsNothing = door == Door.ReadsNothingPastNull || (door == Door.StopsPastNull && nestedInAPredicate);
            return readsNothing && ReadsAnItem(read, items);
        }

        return false;
    }

    private Door DoorOf(IAnonymousFunctionOperation lambda)
    {
        if (ParameterTaking(lambda) is not { } parameter) return Door.None;
        if (_readNothingPastNull.Contains(parameter)) return Door.ReadsNothingPastNull;
        if (_selectedFields.Contains(parameter)) return HoldsNull(lambda.Symbol.ReturnType) ? Door.ReadsNothingPastNull : Door.StopsPastNull;
        if (_stopPastNull.Contains(parameter)) return Door.StopsPastNull;
        return _nestedPredicates.Contains(parameter) ? Door.NestedPredicate : Door.None;
    }

    private static ISymbol? ParameterTaking(IAnonymousFunctionOperation lambda)
    {
        var handed = lambda.Parent;
        while (handed is IDelegateCreationOperation or IConversionOperation) handed = handed.Parent;
        return handed is IArgumentOperation { Parameter: { } parameter } ? parameter.OriginalDefinition : null;
    }

    private static IAnonymousFunctionOperation? Enclosing(IOperation operation)
    {
        var outer = operation.Parent;
        while (outer is not null and not IAnonymousFunctionOperation) outer = outer.Parent;
        return outer as IAnonymousFunctionOperation;
    }

    private static bool ReadsAnItem(IOperation read, ImmutableHashSet<ISymbol> items)
        => read.DescendantsAndSelf().OfType<IParameterReferenceOperation>().Any(reference => items.Contains(reference.Parameter));

    private static bool HoldsNull(ITypeSymbol type)
        => type.IsReferenceType || type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

    private static IEnumerable<IParameterSymbol> ParametersOf(INamedTypeSymbol holder, string door)
        => holder.GetMembers(door).OfType<IMethodSymbol>().SelectMany(method => method.Parameters);

    private static ImmutableHashSet<ISymbol> Set(IEnumerable<IParameterSymbol> parameters)
        => ImmutableHashSet.CreateRange<ISymbol>(SymbolEqualityComparer.Default, parameters);
}
