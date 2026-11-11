---
paths:
  - "Alis.Reactive.Analyzers/**"
  - "poc/Alis.Reactive.Poc.Analyzers/**"
  - "tests/Alis.Reactive.AnalyzerTests/**"
  - "tests/Alis.Reactive.Poc.AnalyzerTests/**"
---

# Analyzers, suppressors and test rules

An analyzer is a line of the spec that the compiler runs: it refuses at build what no type can stage (root CLAUDE.md,
Spec-first). It is written as .NET's own analyzers are, dotnet/roslyn's, and the build and the cases below hold it
there. The analyzers of main, of the redesign and of `poc/Alis.Reactive.Poc.Analyzers` are evidence of which rules
exist and of the cases they met, never of how a rule is written: the unit that lands a rule writes it to this file.
A test rule, TEST001 to TEST013 of `spec/REWRITE.md` section 2.10, is an analyzer and is held by every line here.

## The rule

1. **One id, one rule, one line of the spec.** An ALIS id's line is its row in `spec/Analyzers/module.md` and the
   meaning that row names; a test rule's is its row in `spec/REWRITE.md` section 2.10. A rule with no line is not
   written. The id is a constant on the analyzer and opens its file's name. Its row, with its category and severity,
   is in the project's `AnalyzerReleases.Unshipped.md`, and the build compares that file with every descriptor: an id
   without its row fails the build (RS2000), as a public symbol without its PublicAPI line does. A project's rules
   share one category, the project's name.
2. **The title states the rule as a fact; the message names the member and the cure.** One sentence, the developer's
   own names as its arguments, no final period. A rule of the spec is an error and is on by default; the one
   other severity is the one its line in `spec/Analyzers/module.md` names. A message its spec line quotes is written
   word for word; every other message is its author's, written to this rule. No rule carries a help link: the message
   holds the cure.
3. **Semantic, never textual.** The framework types a rule reads are resolved once for each compilation, by
   `GetTypeByMetadataName` inside `RegisterCompilationStartAction`, and a compilation that lacks one registers
   nothing. The rule reads symbols and `IOperation` nodes and compares symbols with `SymbolEqualityComparer`. It
   decides what code means from a name's text, a file's path or an assembly's name in one case only: its spec line is
   about that name or path, as the slice tree's lines are. What a rule needs beside the code (a slice table, the
   vendor contract, a build property) reaches it as an additional file or a compiler-visible property of the build. A
   rule that reads the compilation alone, and no symbol or operation of it, uses the compilation action, which the
   compiler's own rules ask for (RS1013).
4. **It reports what it proves.** Code the rule does not follow gets no report; where its spec line names a refusal
   at render, that holds it (D18.6). The location is the smallest text the developer edits, a member's name or one
   argument. A rule about the compilation itself (a reference, a table the build did not hand over) has no text to
   mark: it reports with no location, and its case expects none.
5. **No state, any order, no throw.** `Initialize` opens with `ConfigureGeneratedCodeAnalysis` and
   `EnableConcurrentExecution`. The first is chosen by what the rule reads: a rule that reads views analyzes generated
   code and reports in it, since a view reaches the compiler through Razor's generator; every other rule passes
   `GeneratedCodeAnalysisFlags.None`. An analyzer has no instance field: what one compilation gathers lives in its
   start action. It registers the symbol kinds and operation kinds its rule reads and no other, and reports from them:
   a report at the compilation's end is kept for what the whole compilation alone shows (a project that hands the
   compiler no table), since the editor shows it only after a build, and it carries the CompilationEnd tag. Every call that
   takes a cancellation token gets the context's. Code that does not compile ends a check with no report and no
   exception: the editor runs every rule on it.
6. **A suppressor is one compiler warning at the place its spec line names.** It finds that place through the
   semantic model's operation for the warning's location, a `DiagnosticSuppressor` with one descriptor.

## The project

An analyzer project targets netstandard2.0 with the build property EnforceExtendedAnalyzerRules and warnings as
errors, so the compiler's own rules for analyzers hold it: file or environment access in an analyzer fails the build
(RS1035). It references Roslyn at the floor `spec/REWRITE.md` section 2.7 pins, the lowest compiler the packed
analyzers load in: a compiler below an analyzer's Roslyn refuses to load it (CS9057). That reference is private to
the project, so a project that hosts the compiler takes its own. Its two release files,
`AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md`, are additional files of the project.

## The cases

A rule's cases run on Microsoft.CodeAnalysis.Testing, `CSharpAnalyzerTest` with `DefaultVerifier` under NUnit, never
on a compilation a fixture builds by hand: the library fails a case that does not compile, a case whose analyzer
throws, and a report the case does not expect.

1. A case is whole C# 14 source. It compiles against assemblies its test host already holds: the target
   framework's, and each package or project its source reads, found by a type's own assembly. An ALIS case adds
   ASP.NET Core and the core; a test rule's case takes what a Playwright project takes and no more. One kind of case
   reads a package feed while it runs: a rule that reads an MVC 5 type has a second case against net48's reference
   assemblies and Microsoft.AspNet.Mvc, at section 2.7's versions, which the library resolves through the
   repository's nuget.config.
2. **The refused case** marks the reported text in its source and expects the id, the severity and the message as a
   literal sentence, the one the developer reads. Every message of the rule has one.
3. **The accepted case** differs from a refused case by one fact, the fact the rule turns on, and expects no report.
   Every condition of the rule has one.
4. A rule that reads an additional file has a case without that file.
5. One accepted case of every rule holds source that does not compile, with the compiler's error marked and no
   report expected.
6. A suppressor's case expects the warning suppressed at the place its line names and kept one fact away.
7. A rule that reads views has a refused case in a source file at the path Razor's generator gives a view, where
   the compiler meets one; a rule whose spec line reads every file has a refused case in a hand-written file too.
8. A case is a test marked `[RefusedCase]` or `[AcceptedCase]`, its project's two marks, as an architecture test's
   is; `spec/REWRITE.md` section 2.10 names the test that holds every analyzer and suppressor to one of each.
