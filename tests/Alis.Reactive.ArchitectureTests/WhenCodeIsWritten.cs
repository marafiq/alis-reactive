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
        new CSharpParseOptions(LanguageVersion.CSharp14, DocumentationMode.Parse).WithPreprocessorSymbols(Hosts.SymbolsOfNet10),
        new CSharpParseOptions(LanguageVersion.CSharp14, DocumentationMode.Parse).WithPreprocessorSymbols(Hosts.SymbolsOfNet48),
    ];

    private static readonly Regex RazorComment = new(@"@\*[\s\S]*?\*@|<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex WebFormsComment = new(@"<%--[\s\S]*?--%>|<!--[\s\S]*?-->", RegexOptions.Compiled);
    private static readonly Regex HeredocStart = new(@"<<-?\s*['""]?(\w+)['""]?", RegexOptions.Compiled);
    private static readonly Regex DependabotPinnedVersion = new(@"uses:\s*\S+@[0-9a-f]{40}\s+#\s*v[\d.]+\s*$", RegexOptions.Compiled);
    private static readonly string[] XmlBuildFiles = [".csproj", ".props", ".targets", ".slnx", ".config"];

    private sealed record Written(string File, string Text);

    [Test]
    public void csharp_carries_no_comment_and_an_xml_doc_only_on_the_public_surface_of_a_shipped_package()
    {
        var shippedPackages = Repository.TrackedFiles
            .Where(file => file.Split('/') is [_, "PublicAPI", ..])
            .Select(file => file[..file.IndexOf('/')])
            .ToHashSet(StringComparer.Ordinal);
        var sources = TrackedFiles(file => file.EndsWith(".cs", StringComparison.Ordinal));

        Assert.That(shippedPackages, Is.Not.Empty, "No folder holding a PublicAPI folder was found");
        Assert.That(sources, Is.Not.Empty, "No tracked C# was found");
        AssertNone(CSharpComments(sources, shippedPackages),
            "An XML doc is IntelliSense for the members a shipped package's PublicAPI text lists, and nothing else is a comment. " + NameItInstead);
    }

    [RefusedCase]
    public void a_csharp_comment_is_refused_at_its_line()
        => Assert.That(
            CSharpComments([new("Ward/Nurse.cs", "namespace Ward;\n// the night nurse\npublic sealed class Nurse { /* none */ }\n")], ["Ward"]),
            Is.EqualTo(new[] { "Ward/Nurse.cs:2", "Ward/Nurse.cs:3" }));

    [RefusedCase]
    public void an_xml_doc_on_a_member_outside_the_public_surface_is_refused()
        => Assert.That(
            CSharpComments([new("Ward/Nurse.cs", "namespace Ward;\n/// <summary>A nurse.</summary>\ninternal sealed class Nurse;\n")], ["Ward"]),
            Is.EqualTo(new[] { "Ward/Nurse.cs:2" }));

    [RefusedCase]
    public void an_xml_doc_outside_a_shipped_package_is_refused()
        => Assert.That(
            CSharpComments([new("tests/Ward/Nurse.cs", "namespace Ward;\n/// <summary>A nurse.</summary>\npublic sealed class Nurse;\n")], ["Ward"]),
            Is.EqualTo(new[] { "tests/Ward/Nurse.cs:2" }));

    [AcceptedCase]
    public void an_xml_doc_on_a_shipped_packages_public_member_is_accepted()
        => Assert.That(
            CSharpComments([new("Ward/Nurse.cs", "namespace Ward;\n/// <summary>A nurse.</summary>\npublic sealed class Nurse;\n")], ["Ward"]),
            Is.Empty);

    [RefusedCase]
    public void a_comment_in_a_file_one_host_alone_compiles_is_refused_at_its_line()
        => Assert.That(
            CSharpComments(
                [
                    new("Ward/Nurse.Net48.cs", "#if NET48\nnamespace Ward;\n// the day nurse\ninternal sealed class Nurse;\n#endif\n"),
                    new("Ward/Nurse.Net10.cs", "#if NET10_0_OR_GREATER\nnamespace Ward;\n// the night nurse\ninternal sealed class Nurse;\n#endif\n"),
                ],
                ["Ward"]),
            Is.EqualTo(new[] { "Ward/Nurse.Net10.cs:3", "Ward/Nurse.Net48.cs:3" }));

    [AcceptedCase]
    public void a_nullable_directive_and_a_host_split_are_accepted()
        => Assert.That(
            CSharpComments([new("Ward/Nurse.cs", "#nullable enable\nnamespace Ward;\n#if NET48\ninternal sealed class Nurse;\n#else\ninternal sealed class Carer;\n#endif\n")], ["Ward"]),
            Is.Empty);

    [Test]
    public void typescript_and_javascript_carry_no_comment()
    {
        Assert.That(Scripts.Tracked.Code, Is.Not.Empty, "No tracked TypeScript or JavaScript was read");
        AssertNone(Scripts.Tracked.Comments, NameItInstead);
    }

    [RefusedCase]
    public void a_typescript_comment_is_refused_at_its_line()
    {
        using var folder = new WrittenFolder();
        var file = folder.Write("round.ts", "export const round = 1;\n// the night round\nexport const next = /* soon */ 2;\n");

        Assert.That(Scripts.Read([file]).Comments, Is.EquivalentTo(new[] { file + ":2", file + ":3" }));
    }

    [AcceptedCase]
    public void a_shebang_and_text_that_only_looks_like_a_comment_are_accepted()
    {
        using var folder = new WrittenFolder();
        var file = folder.Write("round.mjs", "#!/usr/bin/env node\nexport const ward = \"http://north//wing\";\nexport const shape = /\\/\\*/;\n");

        Assert.That(Scripts.Read([file]).Comments, Is.Empty);
    }

    [Test]
    public void views_stylesheets_and_build_files_carry_no_comment()
        => AssertNone(
            MarkupComments(TrackedFiles(file => Path.GetExtension(file) is ".cshtml" or ".css" || XmlBuildFiles.Contains(Path.GetExtension(file)))),
            NameItInstead);

    [RefusedCase]
    public void a_comment_in_a_view_a_stylesheet_or_a_build_file_is_refused_at_its_line()
        => Assert.That(
            MarkupComments(
            [
                new("Views/Round.cshtml", "<h1>Round</h1>\n@* the night round *@\n<!-- markup -->\n"),
                new("wwwroot/round.css", ".round {\n  /* wide */\n  width: 4rem;\n}\n"),
                new("Ward.csproj", "<Project>\n  <!-- pinned -->\n</Project>\n"),
            ]),
            Is.EqualTo(new[] { "Views/Round.cshtml:2", "Views/Round.cshtml:3", "Ward.csproj:2", "wwwroot/round.css:2" }));

    [AcceptedCase]
    public void text_that_only_looks_like_a_comment_in_a_stylesheet_is_accepted()
        => Assert.That(
            MarkupComments(
            [
                new("Views/Round.cshtml", "<h1>Round</h1>\n"),
                new("wwwroot/round.css", ".round::after {\n  content: \"/* none */\";\n}\n"),
                new("Ward.csproj", "<Project>\n  <PropertyGroup />\n</Project>\n"),
            ]),
            Is.Empty);

    [Test]
    public void json_and_web_forms_files_carry_no_comment()
        => AssertNone(DataComments(TrackedFiles(file => Path.GetExtension(file) is ".json" or ".asax")), NameItInstead);

    [RefusedCase]
    public void a_comment_in_json_or_a_web_forms_file_is_refused_at_its_line()
        => Assert.That(
            DataComments(
            [
                new("round.json", "{\n  // the night round\n  \"ward\": \"North\"\n}\n"),
                new("Global.asax", "<%@ Application %>\n<%-- start --%>\n"),
            ]),
            Is.EqualTo(new[] { "Global.asax:2", "round.json:2" }));

    [AcceptedCase]
    public void text_that_only_looks_like_a_comment_in_json_is_accepted()
        => Assert.That(DataComments([new("round.json", "{\n  \"ward\": \"http://north//wing\"\n}\n")]), Is.Empty);

    [Test]
    public void scripts_workflows_and_ignore_files_carry_no_comment()
        => AssertNone(
            HashComments(TrackedFiles(file => Path.GetExtension(file) is ".sh" or ".yml" or ".yaml" || Path.GetFileName(file) == ".gitignore")),
            NameItInstead + " A shebang, a heredoc's body and the version Dependabot reads beside a pinned action are not comments.");

    [RefusedCase]
    public void a_comment_in_a_script_a_workflow_or_an_ignore_file_is_refused_at_its_line()
        => Assert.That(
            HashComments(
            [
                new("scripts/round.sh", "#!/usr/bin/env bash\n# the night round\necho round # said\n"),
                new(".github/workflows/round.yml", "name: Round\n# nightly\n"),
                new(".gitignore", "bin/\n# outputs\n"),
            ]),
            Is.EqualTo(new[] { ".github/workflows/round.yml:2", ".gitignore:2", "scripts/round.sh:2", "scripts/round.sh:3" }));

    [AcceptedCase]
    public void a_shebang_a_heredocs_body_and_a_pinned_actions_version_are_accepted()
        => Assert.That(
            HashComments(
            [
                new("scripts/round.sh", "#!/usr/bin/env bash\ncat <<'USAGE'\n# every round\nUSAGE\necho \"# of rounds: ${#rounds[@]}\"\n"),
                new(".github/workflows/round.yml", "steps:\n  - uses: actions/checkout@8e8c483db84b4bee98b60c0593521ed34d9990e8 # v6.0.1\n"),
                new(".gitignore", "bin/\n"),
            ]),
            Is.Empty);

    [Test]
    public void editorconfig_carries_no_comment()
    {
        var editorconfigs = TrackedFiles(file => Path.GetFileName(file) == ".editorconfig");

        Assert.That(editorconfigs, Is.Not.Empty, "No tracked .editorconfig was found");
        AssertNone(EditorconfigComments(editorconfigs), NameItInstead);
    }

    [RefusedCase]
    public void a_comment_in_an_editorconfig_is_refused_at_its_line()
        => Assert.That(
            EditorconfigComments([new(".editorconfig", "root = true\n# every file\n[*]\n; spaces\nindent_style = space\n")]),
            Is.EqualTo(new[] { ".editorconfig:2", ".editorconfig:4" }));

    [AcceptedCase]
    public void an_editorconfig_of_sections_and_settings_is_accepted()
        => Assert.That(EditorconfigComments([new(".editorconfig", "root = true\n[*]\nindent_style = space\n")]), Is.Empty);

    private static List<Written> TrackedFiles(Func<string, bool> read)
        => Repository.TrackedFiles.Where(read).Select(file => new Written(file, File.ReadAllText(Path.Combine(Repository.Root, file)))).ToList();

    private static void AssertNone(IReadOnlyCollection<string> commented, string message)
        => Assert.That(commented, Is.Empty, message + "\n" + string.Join("\n", commented));

    private static SortedSet<string> CSharpComments(IReadOnlyList<Written> sources, IReadOnlyCollection<string> shippedPackages)
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var target in Targets)
        {
            foreach (var package in sources.GroupBy(source => shippedPackages.FirstOrDefault(name => source.File.StartsWith(name + "/", StringComparison.Ordinal))))
            {
                var trees = package.Select(source => CSharpSyntaxTree.ParseText(source.Text, target, source.File)).ToList();
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
        }

        return commented;
    }

    private static SortedSet<string> MarkupComments(IReadOnlyList<Written> files)
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            if (file.File.EndsWith(".cshtml", StringComparison.Ordinal))
            {
                foreach (Match comment in RazorComment.Matches(file.Text))
                    commented.Add($"{file.File}:{LineOf(file.Text, comment.Index)}");
            }
            else if (file.File.EndsWith(".css", StringComparison.Ordinal))
            {
                foreach (var line in CssCommentLines(file.Text))
                    commented.Add($"{file.File}:{line}");
            }
            else
            {
                foreach (var comment in XDocument.Parse(file.Text, LoadOptions.SetLineInfo).DescendantNodes().OfType<XComment>())
                    commented.Add($"{file.File}:{((IXmlLineInfo)comment).LineNumber}");
            }
        }

        return commented;
    }

    private static SortedSet<string> DataComments(IReadOnlyList<Written> files)
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            if (file.File.EndsWith(".json", StringComparison.Ordinal))
            {
                foreach (var line in JsonCommentLines(Encoding.UTF8.GetBytes(file.Text)))
                    commented.Add($"{file.File}:{line}");
            }
            else
            {
                foreach (Match comment in WebFormsComment.Matches(file.Text))
                    commented.Add($"{file.File}:{LineOf(file.Text, comment.Index)}");
            }
        }

        return commented;
    }

    private static SortedSet<string> HashComments(IReadOnlyList<Written> files)
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            foreach (var line in HashCommentLines(file.Text, isShell: file.File.EndsWith(".sh", StringComparison.Ordinal)))
                commented.Add($"{file.File}:{line}");
        }

        return commented;
    }

    private static SortedSet<string> EditorconfigComments(IReadOnlyList<Written> files)
    {
        var commented = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var lines = file.Text.Split('\n');
            for (var index = 0; index < lines.Length; index++)
            {
                if (lines[index].TrimStart() is ['#' or ';', ..])
                    commented.Add($"{file.File}:{index + 1}");
            }
        }

        return commented;
    }

    private static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;

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
        for (var current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingType)
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
        {
            if (reader.TokenType == JsonTokenType.Comment)
                lines.Add(json[..(int)reader.TokenStartIndex].Count((byte)'\n') + 1);
        }

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
