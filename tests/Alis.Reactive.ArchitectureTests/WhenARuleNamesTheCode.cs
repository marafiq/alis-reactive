using System.IO.Enumeration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.FileSystemGlobbing;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenARuleNamesTheCode
{
    private const string ThisFile = "tests/Alis.Reactive.ArchitectureTests/" + nameof(WhenARuleNamesTheCode) + ".cs";

    private static readonly string[] Branches = ["origin/main", "feature/", "feature/<name>", "feature/rewrite", "fix/", "chore/"];
    private static readonly string[] OutsideWhatGitTracks = [".worktrees/<name>"];
    private static readonly string[] Placeholders = ["vX", "file:line"];
    private static readonly string[] WhatTheRulesForbid =
        ["page.evaluate()", "writes_a_let_statement", "set_prop_mutation_writes_value_to_ej2_instance"];
    private static readonly string[] VendorTypes = ["EmitType", "ChangeEventArgs"];
    private static readonly string[] PlatformMembers = ["Year", "Application_Start", "FlagsAttribute", "Clock"];
    private static readonly string[] OperatorsTheSpecDecides = ["eqDecimal"];
    private static readonly string[] NotInTheRepo =
        [.. Branches, .. OutsideWhatGitTracks, .. Placeholders, .. WhatTheRulesForbid, .. VendorTypes, .. PlatformMembers, .. OperatorsTheSpecDecides];

    private static readonly CSharpParseOptions ForNet10 = new CSharpParseOptions(LanguageVersion.CSharp14).WithPreprocessorSymbols(Hosts.SymbolsOfNet10);
    private static readonly CSharpParseOptions ForNet48 = new CSharpParseOptions(LanguageVersion.CSharp14).WithPreprocessorSymbols(Hosts.SymbolsOfNet48);
    private static readonly string[] SourceExtensions = [".cs", ".cshtml", ".ts", ".mjs", ".js", ".sh", ".ps1", ".yml", ".yaml", ".json", ".csproj", ".props", ".targets", ".slnx", ".css"];
    private static readonly string[] ShellCommands = ["dotnet", "npm", "node", "git", "sh", "bash"];
    private static readonly string[] ReadFences = ["bash", "csharp", "csharp illustration"];

    private static readonly Regex Span = new(@"`((?:[^`\n]|\n(?!\s*\n))+)`", RegexOptions.Compiled);
    private static readonly Regex Glob = new(@"^\s*-\s*""([^""]+)""\s*$", RegexOptions.Compiled);
    private static readonly Regex FileName = new(@"^[\w.<>-]+\.(md|cs|ts|cshtml|json|sh|mjs|js|txt|slnx|csproj|props|targets|yml|css)(:\d+)?(#[\w-]+)?$", RegexOptions.Compiled);
    private static readonly Regex CitedPath = new(@"^(?<path>[^#]+?)(?::(?<line>\d+))?(?:#(?<anchor>[\w-]+))?$", RegexOptions.Compiled);
    private static readonly Regex Script = new(@"\.(sh|mjs|js)$", RegexOptions.Compiled);
    private static readonly Regex EnvironmentAssignment = new(@"^([A-Z][A-Z0-9_]*)=", RegexOptions.Compiled);
    private static readonly Regex Heading = new(@"^#{1,6}\s+(.+?)\s*#*\s*$", RegexOptions.Compiled);
    private static readonly Regex Identifier = new(@"^[A-Za-z_]\w*$", RegexOptions.Compiled);

    private static readonly Lazy<IReadOnlyList<RuleFile>> Rules = new(ReadRules);
    private static readonly Lazy<(HashSet<string> Files, HashSet<string> Folders)> Tree = new(ReadTree);
    private static readonly Lazy<CodeIndex> Code = new(IndexCode);
    private static readonly Lazy<HashSet<string>> NpmScripts = new(ReadNpmScripts);
    private static readonly Lazy<Audit> Findings = new(RunAudit);

    [Test]
    public void every_rule_file_is_read_whole()
        => AssertNone(Findings.Value.Unread, "A rule file this guard cannot read is a rule it cannot keep true:");

    [Test]
    public void every_path_a_rule_names_is_in_the_repo()
        => AssertNone(Findings.Value.Paths, "A rule names a path the repo does not hold. Name the file as it is now, by its path from the rule's folder or the root:");

    [Test]
    public void every_glob_a_rule_file_loads_on_matches_a_file()
        => AssertNone(Findings.Value.Scopes, "A rule file's paths glob matches no file, so the rule never loads:");

    [Test]
    public void every_name_a_rule_cites_is_one_the_code_uses()
        => AssertNone(Findings.Value.Names, "A rule cites a name no C#, TypeScript or file in the repo uses. Name it as the code does now:");

    [Test]
    public void every_text_a_rule_quotes_occurs_in_the_source()
        => AssertNone(Findings.Value.Texts, "A rule quotes text that occurs in no source file. Quote it as the code writes it now:");

    [Test]
    public void every_member_a_rule_quotes_is_declared_as_quoted()
        => AssertNone(Findings.Value.Members, "A rule quotes a member no declaration in the code matches. Quote the declaration as it is now, or mark the block `csharp illustration`:");

    [Test]
    public void every_option_a_rule_passes_to_a_script_is_one_the_script_reads()
        => AssertNone(Findings.Value.Options, "A rule passes an option or names an npm script the repo does not have:");

    [Test]
    public void every_listed_exception_is_cited_and_needed()
        => AssertNone(
            ListProblems(Findings.Value, NotInTheRepo, WhatTheRulesForbid),
            "The list of what rules cite from outside the repo holds only what a rule cites and the repo lacks:");

    [RefusedCase]
    public void a_fence_the_guard_does_not_read_and_a_rule_file_with_no_code_are_refused()
    {
        var fenced = AuditOf(Rule("CASE.md", "Run `scripts/playwright.sh`.", "```python", "print(1)", "```"), NotInTheRepo);
        var bare = AuditOf(Rule("CASE.md", "A rule with no code in it."), NotInTheRepo);

        Assert.That(fenced.Unread, Is.EqualTo(new[] { "CASE.md:2: a ```python block; this guard reads bash, csharp, csharp illustration" }));
        Assert.That(bare.Unread, Is.EqualTo(new[] { "CASE.md: no code span or block was found in it" }));
    }

    [RefusedCase]
    public void a_path_the_repo_does_not_hold_a_cited_line_and_a_missing_heading_are_refused()
    {
        var audit = AuditOf(Rule("CASE.md", "Run `scripts/gone.sh`, read `CLAUDE.md:12` and `CLAUDE.md#no-such-heading`."), NotInTheRepo);

        Assert.That(audit.Paths, Is.EqualTo(new[]
        {
            "CASE.md:1: `scripts/gone.sh` is in neither the root nor any folder under it",
            "CASE.md:1: `CLAUDE.md:12` cites a line; cite the file, since lines move",
            "CASE.md:1: `CLAUDE.md#no-such-heading`: CLAUDE.md has no heading #no-such-heading",
        }));
    }

    [RefusedCase]
    public void a_glob_that_matches_no_file_is_refused()
        => Assert.That(
            AuditOf(Rule("CASE.md", "---", "paths:", "  - \"Nowhere/**\"", "---", "Run `scripts/playwright.sh`."), NotInTheRepo).Scopes,
            Is.EqualTo(new[] { "CASE.md: \"Nowhere/**\" matches no tracked file" }));

    [RefusedCase]
    public void a_name_no_code_uses_and_text_no_source_holds_are_refused()
    {
        var audit = AuditOf(Rule("CASE.md", "`NoSuchTypeAnywhere` writes `\"no such text anywhere\"`."), NotInTheRepo);

        Assert.That(audit.Names, Is.EqualTo(new[] { "CASE.md:1: `NoSuchTypeAnywhere` (in `NoSuchTypeAnywhere`)" }));
        Assert.That(audit.Texts, Is.EqualTo(new[] { "CASE.md:1: `\"no such text anywhere\"`" }));
    }

    [RefusedCase]
    public void a_member_no_declaration_matches_is_refused()
        => Assert.That(
            AuditOf(Rule("CASE.md", "```csharp", "public int NoSuchMember => 0;", "```"), NotInTheRepo).Members,
            Is.EqualTo(new[] { "CASE.md:2: `public int NoSuchMember => 0;`" }));

    [RefusedCase]
    public void an_option_a_script_does_not_read_and_an_npm_script_the_manifest_lacks_are_refused()
        => Assert.That(
            AuditOf(Rule("CASE.md", "```bash", "scripts/playwright.sh --no-such-option", "npm run nothing", "```"), NotInTheRepo).Options,
            Is.EqualTo(new[] { "CASE.md:2: scripts/playwright.sh reads no option --no-such-option", "CASE.md:3: package.json has no script \"nothing\"" }));

    [RefusedCase]
    public void a_listed_exception_no_rule_cites_or_the_repo_holds_is_refused()
    {
        string[] listed = ["NeverCited", "WhenCodeIsWritten", "Repository"];
        var audit = AuditOf(Rule("CASE.md", "`WhenCodeIsWritten` and `Repository` are cited."), listed);

        Assert.That(ListProblems(audit, listed, ["Repository"]), Is.EqualTo(new[]
        {
            "`NeverCited` is cited by no rule; delete it from the list",
            "`WhenCodeIsWritten` is in the repo; delete it from the list",
            "`Repository` is in the code, which a rule forbids; change the code, not the list",
        }));
    }

    [AcceptedCase]
    public void a_rule_that_names_the_code_as_it_is_is_accepted()
    {
        string[] listed = ["NeverInTheRepo"];
        var audit = AuditOf(
            Rule(
                "CASE.md", "---", "paths:", "  - \"tests/**\"", "---",
                "`WhenCodeIsWritten` reads `tests/Alis.Reactive.ArchitectureTests/Repository.cs` and `CLAUDE.md#spec-first`,",
                "writes `\"build_property.AlisSliceTree\"` and cites `NeverInTheRepo`.",
                "```bash", "scripts/playwright.sh --unit U2", "```",
                "```csharp", "internal static IReadOnlyList<string> TrackedFiles => Tracked.Value;", "```",
                "```csharp illustration", "public int NoSuchMember => 0;", "```"),
            listed);

        Assert.That(audit.Unread.Concat(audit.Paths).Concat(audit.Scopes).Concat(audit.Names).Concat(audit.Texts).Concat(audit.Members).Concat(audit.Options), Is.Empty);
        Assert.That(ListProblems(audit, listed, []), Is.Empty);
    }

    private static void AssertNone(IReadOnlyCollection<string> problems, string header)
        => Assert.That(problems, Is.Empty, header + "\n" + string.Join("\n", problems));

    private static List<string> ListProblems(Audit audit, IReadOnlyList<string> notInTheRepo, IReadOnlyList<string> forbidden)
        => notInTheRepo.Where(entry => !audit.Listed.Contains(entry))
            .Select(entry => $"`{entry}` is cited by no rule; delete it from the list")
            .Concat(notInTheRepo.Except(forbidden).Where(IsInTheRepo).Select(entry => $"`{entry}` is in the repo; delete it from the list"))
            .Concat(forbidden.Where(IsInTheRepo).Select(entry => $"`{entry}` is in the code, which a rule forbids; change the code, not the list"))
            .ToList();

    private static RuleFile[] Rule(string path, params string[] lines) => [ReadRule(path, lines)];

    private sealed record Citation(string At, string Text, bool IsShellLine);

    private sealed record QuotedMember(string At, string Text);

    private sealed record RuleFile(string Path, string Folder, IReadOnlyList<string> Globs, IReadOnlyList<Citation> Citations,
        IReadOnlyList<QuotedMember> QuotedMembers, IReadOnlyList<string> UnreadFences);

    private sealed record CodeIndex(HashSet<string> Names, Dictionary<string, HashSet<string>> Members, string Source);

    private sealed class Audit(IReadOnlyList<string> notInTheRepo)
    {
        public readonly List<string> Unread = [], Paths = [], Scopes = [], Names = [], Texts = [], Members = [], Options = [];
        public readonly HashSet<string> Listed = new(StringComparer.Ordinal);

        public bool IsListed(string cited)
        {
            if (!notInTheRepo.Contains(cited)) return false;
            Listed.Add(cited);
            return true;
        }
    }

    private static IReadOnlyList<RuleFile> ReadRules()
        => Repository.TrackedFiles
            .Where(path => Path.GetFileName(path) == "CLAUDE.md" || Regex.IsMatch(path, @"^\.claude/((rules|agents)/[^/]+|skills/[^/]+/SKILL)\.md$"))
            .Select(path => ReadRule(path, File.ReadAllLines(Path.Combine(Repository.Root, path))))
            .ToList();

    private static RuleFile ReadRule(string path, string[] lines)
    {
        var globs = new List<string>();
        var citations = new List<Citation>();
        var quoted = new List<QuotedMember>();
        var unread = new List<string>();
        var prose = new string[lines.Length];
        var body = 0;
        if (lines.Length > 0 && lines[0] == "---")
        {
            var end = Array.IndexOf(lines, "---", 1);
            globs.AddRange(lines[1..end].Select(line => Glob.Match(line)).Where(match => match.Success).Select(match => match.Groups[1].Value));
            body = end + 1;
        }

        string? fence = null;
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var at = $"{path}:{index + 1}";
            prose[index] = "";
            if (index < body) continue;
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fence = fence == null ? line.Trim()[3..].Trim() : null;
                if (fence != null && !ReadFences.Contains(fence)) unread.Add($"{at}: a ```{fence} block; this guard reads {string.Join(", ", ReadFences)}");
                continue;
            }

            if (fence == null) prose[index] = line;
            else if (line.Trim().Length == 0) continue;
            else if (fence == "bash") citations.Add(new Citation(at, line.Trim(), IsShellLine: true));
            else if (fence == "csharp") quoted.Add(new QuotedMember(at, line.Trim()));
        }

        var text = string.Join('\n', prose);
        foreach (Match span in Span.Matches(text))
        {
            var line = text.AsSpan(0, span.Index).Count('\n') + 1;
            citations.Add(new Citation($"{path}:{line}", Regex.Replace(span.Groups[1].Value, @"\s*\n\s*", " ").Trim(), IsShellLine: false));
        }

        if (citations.Count == 0 && quoted.Count == 0) unread.Add($"{path}: no code span or block was found in it");
        var folder = path.StartsWith(".claude/", StringComparison.Ordinal) ? "" : (Path.GetDirectoryName(path) ?? "").Replace('\\', '/');
        return new RuleFile(path, folder, globs, citations, quoted, unread);
    }

    private static (HashSet<string> Files, HashSet<string> Folders) ReadTree()
    {
        var folders = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in Repository.TrackedFiles)
        {
            for (var slash = file.IndexOf('/'); slash > 0; slash = file.IndexOf('/', slash + 1))
                folders.Add(file[..slash]);
        }

        return (Repository.TrackedFiles.ToHashSet(StringComparer.Ordinal), folders);
    }

    private static CodeIndex IndexCode()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var members = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var unparsed = new List<string>();
        var source = new StringBuilder();

        foreach (var file in Repository.TrackedFiles.Where(file => file != ThisFile))
        {
            foreach (var segment in file.Split('/'))
            {
                names.Add(segment);
                names.Add(Path.GetFileNameWithoutExtension(segment));
            }

            var extension = Path.GetExtension(file);
            if (!SourceExtensions.Contains(extension) || Path.GetFileName(file) == "package-lock.json") continue;
            if (extension is ".ts" or ".mjs" or ".js") continue;

            var text = File.ReadAllText(Path.Combine(Repository.Root, file));
            if (extension != ".cs")
            {
                source.Append(text).Append('\0');
                continue;
            }

            foreach (var options in text.Contains("#if", StringComparison.Ordinal) ? [ForNet10, ForNet48] : new[] { ForNet10 })
            {
                var tree = CSharpSyntaxTree.ParseText(text, options, file);
                unparsed.AddRange(tree.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => $"{file}:{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}"));
                var root = tree.GetRoot();
                source.Append(WithoutComments(root)).Append('\0');
                var directiveTokens = root.DescendantTrivia().Where(trivia => trivia.IsDirective).SelectMany(trivia => trivia.GetStructure()!.DescendantTokens());
                foreach (var token in root.DescendantTokens().Concat(directiveTokens))
                {
                    if (token.IsKind(SyntaxKind.IdentifierToken)) names.Add(token.ValueText);
                    else if (token.IsKind(SyntaxKind.StringLiteralToken) && Identifier.IsMatch(token.ValueText)) names.Add(token.ValueText);
                }

                foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>()
                             .Where(member => member is PropertyDeclarationSyntax or MethodDeclarationSyntax))
                {
                    if (!members.TryGetValue(NameOf(member), out var declared)) members[NameOf(member)] = declared = new HashSet<string>(StringComparer.Ordinal);
                    declared.Add(Normal(member));
                }
            }
        }

        if (unparsed.Count > 0)
        {
            throw new InvalidOperationException("The guard reads the code through Roslyn and could not parse it, so a name in it would go unseen:\n"
                                                + string.Join("\n", unparsed));
        }

        names.UnionWith(Scripts.Tracked.Identifiers);
        foreach (var code in Scripts.Tracked.Code) source.Append(code).Append('\0');
        return new CodeIndex(names, members, source.ToString());
    }

    private static string WithoutComments(SyntaxNode root)
    {
        var code = new StringBuilder();
        foreach (var token in root.DescendantTokens())
        {
            AppendLayout(token.LeadingTrivia);
            code.Append(token.Text);
            AppendLayout(token.TrailingTrivia);
        }

        return code.ToString();

        void AppendLayout(SyntaxTriviaList trivia)
        {
            foreach (var piece in trivia.Where(piece => piece.IsKind(SyntaxKind.WhitespaceTrivia) || piece.IsKind(SyntaxKind.EndOfLineTrivia)))
                code.Append(piece.ToFullString());
        }
    }

    private static HashSet<string> ReadNpmScripts()
    {
        using var package = JsonDocument.Parse(File.ReadAllText(Path.Combine(Repository.Root, "package.json")));
        return package.RootElement.TryGetProperty("scripts", out var scripts)
            ? scripts.EnumerateObject().Select(script => script.Name).ToHashSet(StringComparer.Ordinal)
            : [];
    }

    private static Audit RunAudit()
    {
        var audit = AuditOf(Rules.Value, NotInTheRepo);
        if (!Rules.Value.Any(rule => rule.Path == "CLAUDE.md")) audit.Unread.Add("the root CLAUDE.md was not found among the tracked files");
        if (!Rules.Value.Any(rule => rule.Path.StartsWith(".claude/rules/", StringComparison.Ordinal))) audit.Unread.Add("no file under .claude/rules was found");
        return audit;
    }

    private static Audit AuditOf(IReadOnlyList<RuleFile> rules, IReadOnlyList<string> notInTheRepo)
    {
        var audit = new Audit(notInTheRepo);
        foreach (var rule in rules)
        {
            audit.Unread.AddRange(rule.UnreadFences);
            foreach (var glob in rule.Globs.Where(glob => !MatchesATrackedFile(glob)))
                audit.Scopes.Add($"{rule.Path}: \"{glob}\" matches no tracked file");
            foreach (var citation in rule.Citations) Check(citation, rule.Folder, audit);
            foreach (var member in rule.QuotedMembers) CheckQuoted(member, audit);
        }

        return audit;
    }

    private static bool MatchesATrackedFile(string glob)
    {
        var matcher = new Matcher(StringComparison.Ordinal);
        matcher.AddInclude(glob);
        return matcher.Match(Repository.Root, Repository.TrackedFiles.Select(file => Path.Combine(Repository.Root, file))).HasMatches;
    }

    private static void Check(Citation citation, string folder, Audit audit)
    {
        if (audit.IsListed(citation.Text)) return;
        var words = citation.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words.Where(IsPathShaped)) CheckPath(word, folder, citation.At, audit);

        if (citation.IsShellLine || IsShellCommand(words))
        {
            CheckShell(words, folder, citation.At, audit);
            return;
        }

        var code = string.Join(' ', words.Where(word => !IsPathShaped(word)));
        var isQuotedText = code.Length > 1 && code.StartsWith('"') && code.EndsWith('"');
        var isLowercaseText = !code.Any(char.IsUpper) && code.IndexOfAny(['.', ':', '_', '-', '[']) >= 0;
        if (isQuotedText || isLowercaseText)
        {
            if (!Code.Value.Source.Contains(code, StringComparison.Ordinal)) audit.Texts.Add($"{citation.At}: `{code}`");
        }
        else if (code.Any(char.IsUpper))
        {
            CheckNames(code, citation.At, audit);
        }
    }

    private static bool IsPathShaped(string word)
        => !word.StartsWith('/') && !word.StartsWith('-') && !word.StartsWith('"') && (word.Contains('/') || FileName.IsMatch(word));

    private static bool IsShellCommand(string[] words)
        => ShellCommands.Contains(words[0]) || EnvironmentAssignment.IsMatch(words[0]) || words.Any(word => IsPathShaped(word) && Script.IsMatch(word));

    private static bool IsInTheRepo(string entry)
        => IsPathShaped(entry) ? Resolve(CitedPath.Match(entry).Groups["path"].Value, "").Count > 0
        : Identifier.IsMatch(entry) ? Code.Value.Names.Contains(entry)
        : Code.Value.Source.Contains(entry, StringComparison.Ordinal);

    private static void CheckPath(string word, string folder, string at, Audit audit)
    {
        if (audit.IsListed(word)) return;
        var cited = CitedPath.Match(word);
        var path = cited.Groups["path"].Value;
        var from = folder.Length == 0 ? "the root" : $"the root or {folder}";
        if (cited.Groups["line"].Success) audit.Paths.Add($"{at}: `{word}` cites a line; cite the file, since lines move");

        var found = Resolve(path, folder);
        if (found.Count == 0)
            audit.Paths.Add($"{at}: `{path}` is in neither {from} nor any folder under it");
        else if (found.Count > 1 && !IsPattern(path))
            audit.Paths.Add($"{at}: `{path}` names {found.Count} paths ({string.Join(", ", found.Order(StringComparer.Ordinal).Take(3))}); name it from {from}");
        else if (cited.Groups["anchor"].Success && !SlugsOf(found[0]).Contains(cited.Groups["anchor"].Value))
            audit.Paths.Add($"{at}: `{word}`: {found[0]} has no heading #{cited.Groups["anchor"].Value}");
    }

    private static bool IsPattern(string path) => path.Contains('*') || path.Contains('<');

    private static List<string> Resolve(string path, string folder)
    {
        var (files, folders) = Tree.Value;
        var candidates = path.EndsWith('/') ? folders.ToList() : files.Concat(folders).ToList();
        var pattern = path.TrimEnd('/').Split('/');
        if (pattern[0] is "." or "..")
        {
            var joined = Join(folder, pattern);
            return candidates.Where(candidate => candidate == joined).ToList();
        }

        var prefix = folder.Length == 0 ? "" : folder + "/";
        var anchored = candidates.Where(candidate => Matches(pattern, candidate.Split('/'))
                                                     || (prefix.Length > 0 && candidate.StartsWith(prefix, StringComparison.Ordinal)
                                                                           && Matches(pattern, candidate[prefix.Length..].Split('/'))))
            .ToList();
        if (anchored.Count > 0) return anchored;

        string[] anywhereUnder = ["**", .. pattern];
        return candidates.Where(candidate => candidate.StartsWith(prefix, StringComparison.Ordinal)
                                             && Matches(anywhereUnder, candidate[prefix.Length..].Split('/')))
            .ToList();
    }

    private static string Join(string folder, string[] relative)
    {
        var segments = folder.Length == 0 ? new List<string>() : folder.Split('/').ToList();
        foreach (var segment in relative)
        {
            if (segment == "..") segments.RemoveAt(segments.Count - 1);
            else if (segment != ".") segments.Add(segment);
        }

        return string.Join('/', segments);
    }

    private static bool Matches(ReadOnlySpan<string> pattern, ReadOnlySpan<string> path)
    {
        if (pattern.IsEmpty) return path.IsEmpty;
        if (pattern[0] == "**")
        {
            for (var skipped = 0; skipped <= path.Length; skipped++)
                if (Matches(pattern[1..], path[skipped..])) return true;
            return false;
        }

        return !path.IsEmpty
               && FileSystemName.MatchesSimpleExpression(Regex.Replace(pattern[0], "<[^>]+>", "*"), path[0], ignoreCase: false)
               && Matches(pattern[1..], path[1..]);
    }

    private static HashSet<string> SlugsOf(string file)
    {
        var slugs = new HashSet<string>(StringComparer.Ordinal);
        var inFence = false;
        foreach (var line in File.ReadLines(Path.Combine(Repository.Root, file)))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal)) inFence = !inFence;
            else if (!inFence && Heading.Match(line) is { Success: true } heading)
                slugs.Add(Regex.Replace(heading.Groups[1].Value.ToLowerInvariant(), @"[^\p{L}\p{Nd} _-]", "").Replace(' ', '-'));
        }

        return slugs;
    }

    private static void CheckShell(string[] words, string folder, string at, Audit audit)
    {
        string? script = null;
        for (var index = 0; index < words.Length; index++)
        {
            var word = words[index];
            if (IsPathShaped(word) && Script.IsMatch(word))
            {
                var found = Resolve(word, folder);
                script = found.Count == 1 ? found[0] : null;
            }
            else if (word.StartsWith("--", StringComparison.Ordinal) && script != null)
            {
                var option = word.Split('=')[0];
                if (!File.ReadAllText(Path.Combine(Repository.Root, script)).Contains(option, StringComparison.Ordinal))
                    audit.Options.Add($"{at}: {script} reads no option {option}");
            }
            else if (word == "npm" && index + 1 < words.Length)
            {
                var name = words[index + 1] == "run" && index + 2 < words.Length ? words[index + 2] : words[index + 1];
                if (!NpmScripts.Value.Contains(name)) audit.Options.Add($"{at}: package.json has no script \"{name}\"");
            }
            else if (EnvironmentAssignment.Match(word) is { Success: true } assignment
                     && !audit.IsListed(assignment.Groups[1].Value)
                     && !Code.Value.Names.Contains(assignment.Groups[1].Value))
            {
                audit.Names.Add($"{at}: `{assignment.Groups[1].Value}` (in `{string.Join(' ', words)}`)");
            }
        }
    }

    private static void CheckNames(string code, string at, Audit audit)
    {
        var names = SyntaxFactory.ParseTokens(code, options: ForNet10)
            .SelectMany(token => token.LeadingTrivia.Concat(token.TrailingTrivia)
                .Where(trivia => trivia.HasStructure)
                .SelectMany(trivia => trivia.GetStructure()!.DescendantTokens())
                .Append(token))
            .Where(token => token.IsKind(SyntaxKind.IdentifierToken))
            .Select(token => token.ValueText)
            .Where(name => name.Length > 1 && name.Any(char.IsUpper));
        foreach (var name in names)
        {
            if (!audit.IsListed(name) && !Code.Value.Names.Contains(name))
                audit.Names.Add($"{at}: `{name}` (in `{code}`)");
        }
    }

    private static void CheckQuoted(QuotedMember quoted, Audit audit)
    {
        var parsed = SyntaxFactory.ParseMemberDeclaration(quoted.Text, options: ForNet10);
        if (parsed is not (PropertyDeclarationSyntax or MethodDeclarationSyntax) || parsed.ContainsDiagnostics)
        {
            audit.Members.Add($"{quoted.At}: `{quoted.Text}` is not one property or method declaration");
            return;
        }

        if (!Code.Value.Members.TryGetValue(NameOf(parsed), out var declared) || !declared.Contains(Normal(parsed)))
            audit.Members.Add($"{quoted.At}: `{quoted.Text}`");
    }

    private static string NameOf(MemberDeclarationSyntax member) => member switch
    {
        PropertyDeclarationSyntax property => property.Identifier.ValueText,
        MethodDeclarationSyntax method => method.Identifier.ValueText,
        _ => "",
    };

    private static string Normal(MemberDeclarationSyntax member)
        => member.WithAttributeLists(default).WithoutTrivia().NormalizeWhitespace().ToFullString();
}
