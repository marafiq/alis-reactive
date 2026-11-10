using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

sealed record LambdaParameter(
    string File,
    int Line,
    string Slot,
    string Delegate,
    int Index,
    string Type,
    bool Framework,
    string Name,
    IReadOnlyList<string> Hides);

static class Lambdas
{
    public static IEnumerable<LambdaParameter> Read(Compilation compilation, string repository, Func<string, bool> judged)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var lambda in tree.GetRoot().DescendantNodes().OfType<LambdaExpressionSyntax>())
            {
                if (model.GetTypeInfo(lambda).ConvertedType is not INamedTypeSymbol converted) continue;
                if (converted.OriginalDefinition.ToDisplayString() == "System.Linq.Expressions.Expression<TDelegate>") continue;
                if (model.GetSymbolInfo(lambda).Symbol is not IMethodSymbol bound) continue;
                var place = lambda.GetLocation().GetMappedLineSpan();
                var file = Path.GetRelativePath(repository, Path.GetFullPath(place.Path));
                if (!judged(file)) continue;
                var door = Door(model, lambda);
                var shown = Delegate(converted);
                foreach (var parameter in bound.Parameters)
                    yield return new LambdaParameter(
                        file,
                        place.StartLinePosition.Line + 1,
                        door + "#" + parameter.Ordinal + ":" + Surface.ShortName(Surface.Definition(parameter.Type).ToDisplayString()),
                        shown,
                        parameter.Ordinal,
                        parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        Surface.IsFrameworkType(parameter.Type),
                        parameter.Name,
                        Hidden(model, lambda, bound, parameter.Name));
            }
        }
    }

    static string Door(SemanticModel model, LambdaExpressionSyntax lambda)
    {
        if (Argument(model.GetOperation(lambda)) is not { Parameter: { } parameter }) return "-";
        if (parameter.ContainingSymbol is not IMethodSymbol method) return "-";
        var extended = method.ReducedFrom is { } reduced ? reduced.Parameters[0].Type
            : method.IsExtensionMethod ? method.Parameters[0].Type
            : method.ContainingType.IsExtension ? method.ContainingType.ExtensionParameter!.Type
            : method.ContainingType;
        var receiver = Surface.Receiver(extended);
        return Surface.ShortName(receiver) + "." + method.Name + "(" + parameter.Name + ")";
    }

    static IArgumentOperation? Argument(IOperation? operation)
    {
        for (var current = operation; current is not null; current = current.Parent)
        {
            if (current is IArgumentOperation argument) return argument;
            if (current is not (IDelegateCreationOperation or IAnonymousFunctionOperation or IConversionOperation or IArrayInitializerOperation or IArrayCreationOperation)) return null;
        }
        return null;
    }

    static string Delegate(INamedTypeSymbol converted) =>
        converted.TypeArguments.Length == 0
            ? converted.Name
            : converted.Name + "<" + string.Join(", ", converted.TypeArguments.Select(argument =>
                Surface.Definition(argument).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))) + ">";

    static IReadOnlyList<string> Hidden(SemanticModel model, LambdaExpressionSyntax lambda, IMethodSymbol bound, string name) =>
        model.LookupSymbols(lambda.SpanStart, name: name)
            .Where(symbol => symbol is ILocalSymbol or IParameterSymbol or IRangeVariableSymbol)
            .Where(symbol => !SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, bound))
            .Select(symbol => symbol.Name)
            .Distinct()
            .ToList();
}
