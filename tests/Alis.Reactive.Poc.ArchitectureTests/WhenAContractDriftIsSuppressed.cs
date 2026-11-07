using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.Poc.ArchitectureTests;

[TestFixture]
public sealed class WhenAContractDriftIsSuppressed
{
    private const string Category = "Alis.Reactive.Contract";
    private const string Purpose = "Drift on purpose: ";

    [Test]
    public void each_suppression_in_the_app_names_a_browser_test_that_exists()
    {
        var tests = Sources("tests", "Alis.Reactive.Poc.PlaywrightTests")
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Select(method => method.Identifier.Text)
            .ToHashSet(StringComparer.Ordinal);
        var justifications = Sources("tests", "Alis.Reactive.Poc.App")
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>())
            .Where(attribute => attribute.Name.ToString() == "SuppressMessage" && Text(attribute.ArgumentList!.Arguments[0]) == Category)
            .Select(attribute => Text(attribute.ArgumentList!.Arguments.Single(argument => argument.NameEquals?.Name.Identifier.Text == "Justification")))
            .ToList();

        var unnamed = justifications.Where(justification => !justification.StartsWith(Purpose, StringComparison.Ordinal)).ToList();
        var missing = justifications
            .Where(justification => justification.StartsWith(Purpose, StringComparison.Ordinal))
            .Select(justification => justification[Purpose.Length..].Split(' ')[0])
            .Where(test => !tests.Contains(test))
            .ToList();

        Assert.That(justifications, Has.Count.EqualTo(17), "The app suppresses each drift a browser test arranges on purpose, and no other.");
        Assert.That(unnamed, Is.Empty, "A suppression in the app says \"" + Purpose + "\" and then the test it serves.");
        Assert.That(missing, Is.Empty, "A suppression names a browser test the Playwright project declares.");
    }

    private static string Text(AttributeArgumentSyntax argument) => argument.Expression is LiteralExpressionSyntax literal ? literal.Token.ValueText : argument.Expression.ToString();

    private static IEnumerable<SyntaxTree> Sources(params string[] folder)
        => Directory.EnumerateFiles(Path.Combine([RepositoryRoot(), .. folder]), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "poc", "build.sh"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("The repository holds poc/build.sh.");
    }
}
