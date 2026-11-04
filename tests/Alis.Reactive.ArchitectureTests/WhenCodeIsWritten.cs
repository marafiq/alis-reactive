using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenCodeIsWritten
{
    private const string NameItInstead = "Code is the truth: name the thing what it is, and delete the comment.";

    private static readonly CSharpParseOptions[] Targets =
    [
        new CSharpParseOptions(LanguageVersion.CSharp14, DocumentationMode.Parse).WithPreprocessorSymbols("NET6_0_OR_GREATER"),
        new CSharpParseOptions(LanguageVersion.CSharp14, DocumentationMode.Parse).WithPreprocessorSymbols("NET48"),
    ];

    private static readonly Regex RazorComment = new(@"@\*[\s\S]*?\*@|<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex WebFormsComment = new(@"<%--[\s\S]*?--%>|<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex HeredocStart = new(@"<<-?\s*['""]?(\w+)['""]?", RegexOptions.Compiled);
    private static readonly Regex DependabotPinnedVersion = new(@"uses:\s*\S+@[0-9a-f]{40}\s+#\s*v[\d.]+\s*$", RegexOptions.Compiled);
    private static readonly string[] XmlBuildFiles = [".csproj", ".props", ".targets", ".slnx", ".config"];

    [Test]
    public void csharp_carries_no_comment_and_an_xml_doc_only_on_the_public_surface_of_a_shipped_package()
    {
        var shippedPackages = Repository.TrackedFiles
            .Where(file => file.Split('/') is [_, "PublicAPI", ..])
            .Select(file => file[..file.IndexOf('/')])
            .ToHashSet(StringComparer.Ordinal);
        var sources = TrackedFilesEndingIn(".cs");
        var commented = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var target in Targets)
            foreach (var package in sources.GroupBy(file => shippedPackages.FirstOrDefault(name => file.StartsWith(name + "/", StringComparison.Ordinal))))
            {
                var trees = package.Select(file => CSharpSyntaxTree.ParseText(Read(file), target, file)).ToList();
                var compilation = package.Key == null ? null : CSharpCompilation.Create(package.Key, trees);
                foreach (var tree in trees)
                {
                    var model = compilation?.GetSemanticModel(tree);
                    foreach (var trivia in tree.GetRoot().DescendantTrivia())
                    {
                        var isComment = trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
                        var isXmlDoc = trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
                        if (isComment || (isXmlDoc && !DocumentsThePublicSurface(trivia, model)))
                            commented.Add($"{tree.FilePath}:{tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1}");
                    }
                }
            }

        Assert.That(shippedPackages, Is.Not.Empty, "No folder holding a PublicAPI folder was found");
        Assert.That(sources, Is.Not.Empty, "No tracked C# was found");
        AssertNone(commented, "An XML doc is IntelliSense for the members a shipped package's PublicAPI text lists, and nothing else is a comment. " + NameItInstead);
    }

    [Test]
    public void typescript_and_javascript_carry_no_comment()
    {
        Assert.That(Scripts.Tracked.Code, Is.Not.Empty, "No tracked TypeScript or JavaScript was read");
        AssertNone(Scripts.Tracked.Comments, NameItInstead);
    }

    [Test]
    public void views_stylesheets_and_build_files_carry_no_comment()
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var view in TrackedFilesEndingIn(".cshtml"))
            foreach (Match comment in RazorComment.Matches(Read(view)))
                commented.Add($"{view}:{LineOf(Read(view), comment.Index)}");
        foreach (var stylesheet in TrackedFilesEndingIn(".css"))
            foreach (var line in CssCommentLines(Read(stylesheet)))
                commented.Add($"{stylesheet}:{line}");
        foreach (var buildFile in Repository.TrackedFiles.Where(file => XmlBuildFiles.Contains(Path.GetExtension(file))))
            foreach (var comment in XDocument.Parse(Read(buildFile), LoadOptions.SetLineInfo).DescendantNodes().OfType<XComment>())
                commented.Add($"{buildFile}:{((IXmlLineInfo)comment).LineNumber}");

        AssertNone(commented, NameItInstead);
    }

    [Test]
    public void json_and_web_forms_files_carry_no_comment()
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var json in TrackedFilesEndingIn(".json"))
            foreach (var line in JsonCommentLines(File.ReadAllBytes(Path.Combine(Repository.Root, json))))
                commented.Add($"{json}:{line}");
        foreach (var webForm in TrackedFilesEndingIn(".asax"))
            foreach (Match comment in WebFormsComment.Matches(Read(webForm)))
                commented.Add($"{webForm}:{LineOf(Read(webForm), comment.Index)}");

        AssertNone(commented, NameItInstead);
    }

    [Test]
    public void scripts_workflows_and_ignore_files_carry_no_comment()
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in Repository.TrackedFiles.Where(file => Path.GetExtension(file) is ".sh" or ".yml" or ".yaml" || Path.GetFileName(file) == ".gitignore"))
            foreach (var line in HashCommentLines(Read(file), isShell: file.EndsWith(".sh", StringComparison.Ordinal)))
                commented.Add($"{file}:{line}");

        AssertNone(commented, NameItInstead + " A shebang, a heredoc's body and the version Dependabot reads beside a pinned action are not comments.");
    }

    [Test]
    public void editorconfig_carries_no_comment()
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        var editorconfigs = Repository.TrackedFiles.Where(file => Path.GetFileName(file) == ".editorconfig").ToList();
        Assert.That(editorconfigs, Is.Not.Empty, "No tracked .editorconfig was found");
        foreach (var file in editorconfigs)
        {
            var lines = Read(file).Split('\n');
            for (var index = 0; index < lines.Length; index++)
                if (lines[index].TrimStart() is ['#' or ';', ..])
                    commented.Add($"{file}:{index + 1}");
        }

        AssertNone(commented, NameItInstead);
    }

    private static List<string> TrackedFilesEndingIn(string extension) =>
        Repository.TrackedFiles.Where(file => file.EndsWith(extension, StringComparison.Ordinal)).ToList();

    private static string Read(string file) => File.ReadAllText(Path.Combine(Repository.Root, file));

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

    private static void AssertNone(IReadOnlyCollection<string> commented, string message) =>
        Assert.That(commented, Is.Empty, message + "\n" + string.Join("\n", commented));

    private static bool DocumentsThePublicSurface(SyntaxTrivia doc, SemanticModel? model)
    {
        var declaration = doc.Token.Parent?.AncestorsAndSelf()
            .FirstOrDefault(node => node is MemberDeclarationSyntax or EnumMemberDeclarationSyntax && node.GetFirstToken() == doc.Token);
        var symbol = declaration == null || model == null ? null : DeclaredBy(model, declaration);
        return symbol != null && IsPublicSurface(symbol);
    }

    private static ISymbol? DeclaredBy(SemanticModel model, SyntaxNode declaration) => declaration switch
    {
        FieldDeclarationSyntax field => model.GetDeclaredSymbol(field.Declaration.Variables[0]),
        EventFieldDeclarationSyntax eventField => model.GetDeclaredSymbol(eventField.Declaration.Variables[0]),
        _ => model.GetDeclaredSymbol(declaration),
    };

    private static bool IsPublicSurface(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingType)
        {
            var reachable = current.DeclaredAccessibility switch
            {
                Accessibility.Public => true,
                Accessibility.Protected or Accessibility.ProtectedOrInternal =>
                    current.ContainingType is { IsSealed: false, IsStatic: false, TypeKind: TypeKind.Class },
                _ => false,
            };
            if (!reachable) return false;
        }

        return true;
    }

    private static IEnumerable<int> CssCommentLines(string css)
    {
        var line = 1;
        char? quote = null;
        for (var index = 0; index < css.Length; index++)
        {
            var current = css[index];
            if (current == '\n') line++;
            if (quote != null)
            {
                if (current == '\\') index++;
                else if (current == quote) quote = null;
                continue;
            }

            if (current is '"' or '\'')
            {
                quote = current;
                continue;
            }

            if (current != '/' || index + 1 >= css.Length || css[index + 1] != '*') continue;
            yield return line;
            var end = css.IndexOf("*/", index + 2, StringComparison.Ordinal);
            var stop = end < 0 ? css.Length : end + 2;
            line += css.AsSpan(index, stop - index).Count('\n');
            index = stop - 1;
        }
    }

    private static List<int> JsonCommentLines(byte[] file)
    {
        var lines = new List<int>();
        var json = file.AsSpan(file.AsSpan().StartsWith(Encoding.UTF8.Preamble) ? Encoding.UTF8.Preamble.Length : 0);
        var reader = new Utf8JsonReader(json, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Allow, AllowTrailingCommas = true });
        while (reader.Read())
            if (reader.TokenType == JsonTokenType.Comment)
                lines.Add(json[..(int)reader.TokenStartIndex].Count((byte)'\n') + 1);
        return lines;
    }

    private static IEnumerable<int> HashCommentLines(string text, bool isShell)
    {
        string? heredoc = null;
        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (heredoc != null)
            {
                if (line.Trim() == heredoc) heredoc = null;
                continue;
            }

            if ((index == 0 && line.StartsWith("#!", StringComparison.Ordinal)) || DependabotPinnedVersion.IsMatch(line)) continue;
            if (StartsAComment(line)) yield return index + 1;
            if (isShell && HeredocStart.Match(line) is { Success: true } start) heredoc = start.Groups[1].Value;
        }
    }

    private static bool StartsAComment(string line)
    {
        char? quote = null;
        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];
            if (quote != null)
            {
                if (current == quote) quote = null;
                continue;
            }

            if (current is '"' or '\'') quote = current;
            else if (current == '#' && (index == 0 || line[index - 1] is ' ' or '\t')) return true;
        }

        return false;
    }
}
