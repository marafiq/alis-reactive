using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed record CellEnd(string Name, string? Gap, string File, int Line);

internal sealed record CellResult(string Position, string Source, string? Gap, IReadOnlyList<string> Errors);

internal sealed record CellTable(IReadOnlyList<CellEnd> Positions, IReadOnlyList<CellEnd> Sources, IReadOnlyList<CellResult> Results);

internal static class CellMatrix
{
    private const string Namespace = "Cells";
    private const string Hole = "value";

    private sealed record End(CellEnd Described, IMethodSymbol Method, MethodDeclarationSyntax Syntax, SemanticModel Model);

    public static CSharpCompilation Planted(CSharpCompilation compilation, string plant)
    {
        var stem = Path.GetFileName(plant).Split('.')[0];
        var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        var planted = CSharpSyntaxTree.ParseText(File.ReadAllText(plant), options, Path.GetFullPath(plant));
        var replaced = compilation.SyntaxTrees.FirstOrDefault(tree => Path.GetFileNameWithoutExtension(tree.FilePath) == stem);
        return replaced is null ? compilation.AddSyntaxTrees(planted) : compilation.ReplaceSyntaxTree(replaced, planted);
    }

    public static CellTable Read(CSharpCompilation compilation, string repository)
    {
        var positions = Ends(compilation, "Positions", repository);
        var sources = Ends(compilation, "Sources", repository);
        var usings = compilation.SyntaxTrees
            .SelectMany(tree => tree.GetCompilationUnitRoot().Usings)
            .Select(directive => directive.ToString())
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();
        var pairs = new List<(End Position, End Source, string Method)>();
        var methods = new List<string>();
        foreach (var position in positions)
        {
            foreach (var source in sources)
            {
                if (position.Described.Gap is not null || source.Described.Gap is not null) continue;
                var name = $"Cell_{position.Described.Name}_{source.Described.Name}";
                pairs.Add((position, source, name));
                methods.Add(Cell(position, source, name));
            }
        }
        var text = string.Join("\n", usings) + $"\n\nnamespace {Namespace};\n\npublic static class GeneratedCells\n{{\n" + string.Join("\n\n", methods) + "\n}\n";
        var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        var generated = CSharpSyntaxTree.ParseText(text, options, "generated-cells.cs");
        var withCells = compilation.AddSyntaxTrees(generated);
        var errors = withCells.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        var outside = errors.Where(error => error.Location.SourceTree != generated).ToList();
        if (outside.Count > 0)
            throw new InvalidOperationException("the cells broke a tree outside them: " + outside[0]);
        var root = generated.GetCompilationUnitRoot();
        var spans = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToDictionary(method => method.Identifier.Text, method => method.Span);
        var unplaced = errors.Where(error => !spans.Values.Any(span => span.Contains(error.Location.SourceSpan))).ToList();
        if (unplaced.Count > 0)
            throw new InvalidOperationException("the generated cells hold an error outside every cell: " + unplaced[0]);
        var compiled = pairs.ToDictionary(
            pair => (pair.Position.Described.Name, pair.Source.Described.Name),
            pair => errors.Where(error => spans[pair.Method].Contains(error.Location.SourceSpan)).Select(error => error.Id).Distinct().Order(StringComparer.Ordinal).ToList());
        var results = new List<CellResult>();
        foreach (var position in positions)
        {
            foreach (var source in sources)
            {
                var gaps = new[] { position.Described.Gap, source.Described.Gap }.OfType<string>().Distinct().ToList();
                results.Add(gaps.Count > 0
                    ? new CellResult(position.Described.Name, source.Described.Name, string.Join("; ", gaps), [])
                    : new CellResult(position.Described.Name, source.Described.Name, null, compiled[(position.Described.Name, source.Described.Name)]));
            }
        }
        return new CellTable(positions.Select(end => end.Described).ToList(), sources.Select(end => end.Described).ToList(), results);
    }

    private static List<End> Ends(CSharpCompilation compilation, string type, string repository)
    {
        var symbol = compilation.GetTypeByMetadataName($"{Namespace}.{type}") ?? throw new InvalidOperationException($"spec/api/Cells declares no {Namespace}.{type}");
        return symbol.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public)
            .Select(method =>
            {
                var syntax = (MethodDeclarationSyntax)method.DeclaringSyntaxReferences.Single().GetSyntax();
                var model = compilation.GetSemanticModel(syntax.SyntaxTree);
                var body = syntax.ExpressionBody?.Expression ?? throw new InvalidOperationException($"{type}.{method.Name} is no expression-bodied method");
                var line = syntax.GetLocation().GetLineSpan();
                var file = Path.GetRelativePath(repository, line.Path).Replace('\\', '/');
                return new End(new CellEnd(method.Name, Gap(model, body), file, line.StartLinePosition.Line + 1), method, syntax, model);
            })
            .OrderBy(end => end.Described.File, StringComparer.Ordinal)
            .ThenBy(end => end.Syntax.SpanStart)
            .ToList();
    }

    private static string? Gap(SemanticModel model, ExpressionSyntax body)
        => body is InvocationExpressionSyntax invocation
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol called
        && called.ContainingType.ToDisplayString() == $"{Namespace}.NoDoor"
            ? model.GetConstantValue(invocation.ArgumentList.Arguments[0].Expression).Value as string ?? called.Name
            : null;

    private static string Cell(End position, End source, string name)
    {
        var hole = position.Method.Parameters.SingleOrDefault(parameter => parameter.Name == Hole)
            ?? throw new InvalidOperationException($"Positions.{position.Method.Name} has no parameter named {Hole}");
        var renamed = source.Method.Parameters.ToDictionary(parameter => parameter.Name, parameter => "from" + char.ToUpperInvariant(parameter.Name[0]) + parameter.Name[1..]);
        var sourceExpression = Rewrite(source.Model, source.Syntax.ExpressionBody!.Expression, symbol =>
            symbol is IParameterSymbol parameter && SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, source.Method) ? SyntaxFactory.IdentifierName(renamed[parameter.Name]) : null);
        var filled = Rewrite(position.Model, position.Syntax.ExpressionBody!.Expression, symbol =>
            SymbolEqualityComparer.Default.Equals(symbol, hole) ? SyntaxFactory.ParenthesizedExpression(sourceExpression) : null);
        var parameters = position.Syntax.ParameterList.Parameters
            .Where(parameter => parameter.Identifier.Text != Hole)
            .Select(parameter => parameter.ToString())
            .Concat(source.Syntax.ParameterList.Parameters.Select(parameter =>
                parameter.Type + " " + renamed[parameter.Identifier.Text]));
        return $"    public static void {name}({string.Join(", ", parameters)}) => {filled.NormalizeWhitespace()};";
    }

    private static ExpressionSyntax Rewrite(SemanticModel model, ExpressionSyntax expression, Func<ISymbol, ExpressionSyntax?> replacement)
    {
        var replacements = expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
            .Select(identifier => (identifier, symbol: model.GetSymbolInfo(identifier).Symbol))
            .Where(found => found.symbol is not null)
            .Select(found => (found.identifier, with: replacement(found.symbol!)))
            .Where(found => found.with is not null)
            .ToDictionary(found => (SyntaxNode)found.identifier, found => (SyntaxNode)found.with!);
        return expression.ReplaceNodes(replacements.Keys, (original, _) => replacements[original]);
    }
}
