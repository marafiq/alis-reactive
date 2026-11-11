using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAnInternalIsExposed
{
    private const string ThePublicSurface = "a test and a consumer go through the public surface, and a behavior no public door reaches is given one";
    private const string Attribute = "Attribute";
    private const string AssemblyAttributeItem = "AssemblyAttribute";
    private const string Include = "Include";

    private static readonly string[] FriendAttributes = ["InternalsVisibleTo", "IgnoresAccessChecksTo"];
    private static readonly string[] ProjectFiles = [".csproj", ".props", ".targets"];

    private sealed record Written(string File, string Text);

    [Test]
    public void no_assembly_opens_its_internals_to_another()
    {
        var sources = Tracked(file => file.EndsWith(".cs", StringComparison.Ordinal));
        var projects = Tracked(file => ProjectFiles.Contains(Path.GetExtension(file)));

        Assert.That(sources, Is.Not.Empty, "No tracked C# was found");
        Assert.That(projects, Is.Not.Empty, "No tracked project file was found");
        Assert.That(Problems(sources, projects), Is.Empty, "An internal is exposed: " + ThePublicSurface + ".");
    }

    [RefusedCase]
    public void a_friend_assembly_named_in_csharp_is_refused_at_its_line()
        => Assert.That(
            Problems(
                [
                    new("Ward/Friends.cs", "using System.Runtime.CompilerServices;\n\n[assembly: InternalsVisibleTo(\"Ward.Tests\")]\n"),
                    new("Ward/MoreFriends.cs", "[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute(\"Ward.ModuleTests\")]\n"),
                    new("Ward.Tests/Reach.cs", "using System.Runtime.CompilerServices;\n[assembly: IgnoresAccessChecksTo(\"Ward\")]\n"),
                ],
                []),
            Is.EqualTo(new[]
            {
                "Ward/Friends.cs:3: InternalsVisibleTo opens the assembly's internals; " + ThePublicSurface,
                "Ward/MoreFriends.cs:1: InternalsVisibleTo opens the assembly's internals; " + ThePublicSurface,
                "Ward.Tests/Reach.cs:2: IgnoresAccessChecksTo opens the assembly's internals; " + ThePublicSurface,
            }));

    [RefusedCase]
    public void a_friend_assembly_named_in_a_project_file_is_refused_at_its_line()
        => Assert.That(
            Problems(
                [],
                [
                    new("Ward/Ward.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <InternalsVisibleTo Include=\"Ward.Tests\" />\n  </ItemGroup>\n</Project>\n"),
                    new("Directory.Build.props", "<Project>\n  <ItemGroup>\n    <AssemblyAttribute Include=\"System.Runtime.CompilerServices.InternalsVisibleToAttribute\">\n      <_Parameter1>Ward.Tests</_Parameter1>\n    </AssemblyAttribute>\n  </ItemGroup>\n</Project>\n"),
                ]),
            Is.EqualTo(new[]
            {
                "Ward/Ward.csproj:3: InternalsVisibleTo opens the assembly's internals; " + ThePublicSurface,
                "Directory.Build.props:3: InternalsVisibleTo opens the assembly's internals; " + ThePublicSurface,
            }));

    [AcceptedCase]
    public void another_attribute_a_word_in_text_and_a_reference_are_accepted()
        => Assert.That(
            Problems(
                [new("Ward/Nurse.cs", "using System;\n\n[assembly: CLSCompliant(false)]\n\nnamespace Ward;\n\ninternal static class Nurse\n{\n    internal const string Refused = \"InternalsVisibleTo\";\n}\n")],
                [new("Ward.Tests/Ward.Tests.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <ProjectReference Include=\"..\\Ward\\Ward.csproj\" />\n    <AssemblyAttribute Include=\"System.CLSCompliantAttribute\">\n      <_Parameter1>false</_Parameter1>\n    </AssemblyAttribute>\n  </ItemGroup>\n</Project>\n")]),
            Is.Empty);

    private static List<Written> Tracked(Func<string, bool> read)
        => Repository.TrackedFiles.Where(read).Select(file => new Written(file, File.ReadAllText(Path.Combine(Repository.Root, file)))).ToList();

    private static List<string> Problems(IReadOnlyList<Written> sources, IReadOnlyList<Written> projects)
        => sources.SelectMany(InCSharp).Concat(projects.SelectMany(InAProjectFile)).ToList();

    private static IEnumerable<string> InCSharp(Written source)
    {
        var tree = CSharpSyntaxTree.ParseText(source.Text, new CSharpParseOptions(LanguageVersion.CSharp14), source.File);
        foreach (var applied in tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>())
        {
            if (FriendOf(NameOf(applied.Name)) is { } friend)
                yield return Refusal(source.File, tree.GetLineSpan(applied.Span).StartLinePosition.Line + 1, friend);
        }
    }

    private static IEnumerable<string> InAProjectFile(Written project)
    {
        foreach (var element in XDocument.Parse(project.Text, LoadOptions.SetLineInfo).Descendants())
        {
            var named = element.Name.LocalName == AssemblyAttributeItem
                ? (element.Attribute(Include)?.Value ?? "").Split('.')[^1]
                : element.Name.LocalName;
            if (FriendOf(named) is { } friend)
                yield return Refusal(project.File, ((IXmlLineInfo)element).LineNumber, friend);
        }
    }

    private static string NameOf(NameSyntax name) => name switch
    {
        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
        AliasQualifiedNameSyntax aliased => aliased.Name.Identifier.ValueText,
        SimpleNameSyntax simple => simple.Identifier.ValueText,
        _ => name.ToString(),
    };

    private static string? FriendOf(string attribute)
        => FriendAttributes.FirstOrDefault(friend => attribute == friend || attribute == friend + Attribute);

    private static string Refusal(string file, int line, string friend) => $"{file}:{line}: {friend} opens the assembly's internals; {ThePublicSurface}";
}
