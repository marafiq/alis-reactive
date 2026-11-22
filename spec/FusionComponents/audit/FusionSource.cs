using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class FusionSource
{
    private static string At(SyntaxNode node) => node.SyntaxTree.FilePath + ":" + (node.GetLocation().GetLineSpan().StartLinePosition.Line + 1);

    private static string Name(ClassDeclarationSyntax type) => type.Identifier.ValueText + type.TypeParameterList;

    private static string Literal(ExpressionSyntax expression) => expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression)
        ? literal.Token.ValueText : throw new InvalidOperationException("Expected a string literal at " + At(expression));

    private static string[] Attribute(MemberDeclarationSyntax declaration, string name) =>
        declaration.AttributeLists.SelectMany(list => list.Attributes).Where(attribute => attribute.Name.ToString() == name)
            .SelectMany(attribute => attribute.ArgumentList!.Arguments).Select(argument => Literal(argument.Expression)).ToArray();

    private static object Member(MemberDeclarationSyntax member)
    {
        var method = member as MethodDeclarationSyntax;
        var property = member as PropertyDeclarationSyntax;
        var name = method?.Identifier.ValueText ?? property!.Identifier.ValueText;
        var expression = method?.ExpressionBody?.Expression ?? property?.ExpressionBody?.Expression;
        var call = expression as InvocationExpressionSyntax;
        var operation = call?.Expression is SimpleNameSyntax simple ? simple.Identifier.ValueText : expression is null && property?.AccessorList is not null ? "JSON" :
            call?.Expression is MemberAccessExpressionSyntax && name == "Files" ? "projection" : throw new InvalidOperationException("Unknown declaration at " + At(member));
        var alias = call?.ArgumentList.Arguments.SingleOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == "name");
        var js = alias is null ? Attribute(member, "JsonPropertyName").SingleOrDefault() ?? JsonNamingPolicy.CamelCase.ConvertName(name) : Literal(alias.Expression);
        var result = method?.ReturnType ?? property!.Type;
        var args = call?.ArgumentList.Arguments.FirstOrDefault(arg => arg.NameColon is null || arg.NameColon.Name.Identifier.ValueText == "arguments");
        var emittedArguments = args?.Expression is CollectionExpressionSyntax collection && collection.Elements.All(element => element is ExpressionElementSyntax)
            ? collection.Elements.Select(element => element.ToString()).ToArray() : args is null ? Array.Empty<string>() : null;
        return new {
            name, js, @operator = operation, at = At(member), source = member.ToString(), emittedArguments,
            result = result.ToString(), arguments = method?.ParameterList.Parameters.Select(p => p.Type + " " + p.Identifier.ValueText).ToArray(),
            generics = method?.TypeParameterList?.Parameters.Select(p => p.Identifier.ValueText).ToArray() ?? Array.Empty<string>()
        };
    }

    private static object Slice(string file)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file);
        var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(string.Join("\n", errors.Select(d => d.ToString())));
        var types = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Select(type => {
            var name = string.Join(".", type.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().Reverse().Select(Name));
            var mirrors = Attribute(type, "Mirrors");
            var members = new List<object>();
            foreach (var member in type.Members.Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))) {
                if (member is BaseTypeDeclarationSyntax || member is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword)) continue;
                if (member is PropertyDeclarationSyntax prop && (prop.Identifier.ValueText == "DefaultId" || prop.Identifier.ValueText == "Vendor")) continue;
                if (member is not (MethodDeclarationSyntax or PropertyDeclarationSyntax)) throw new InvalidOperationException("Unexpected member at " + At(member));
                members.Add(Member(member));
            }
            return new {symbol = name, at = At(type), mirror = mirrors.Length == 0 ? null : mirrors, members};
        }).ToArray();
        return new {name = Path.GetFileNameWithoutExtension(file), area = Directory.GetParent(file)!.Parent!.Name, types};
    }

    private static void Main(string[] files) => Console.WriteLine(JsonSerializer.Serialize(files.Select(Slice)));
}
