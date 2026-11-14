# Alis.Reactive.Analyzers

The compiler's rules, packed inside AlisReactive: each analyzer is one line of the spec that the build runs. A folder
is named for the module whose lines its rules enforce; `Analyzers/` holds module 20's own, `ALIS009` to `ALIS013`,
`TypeKnowledge/` holds `ALIS023` to `ALIS025`, a declaration door's body and constants and an override's JSON name
and ignore, `PartialsAndSlots/` holds `ALIS041`, the plan a view opens and renders, and `ValuesAndSources/` holds
`ALIS043`, a source .NET makes a text of.

## The standard

`.claude/rules/analyzers.md` holds how an analyzer, a suppressor and a test rule are written and proven, and loads
with any file here. What this project adds:
- A rule lands in the unit of the module whose line it enforces, with its row in `AnalyzerReleases.Unshipped.md` and
  its cases under `tests/Alis.Reactive.AnalyzerTests`. A rule with no line in `spec/Analyzers/module.md` is not written.
- The analyzers of main, of the redesign and of `poc/Alis.Reactive.Poc.Analyzers` are evidence of which rules exist
  and of the cases they met, never code this project takes.
- The project compiles against the Roslyn floor of `spec/REWRITE.md` section 2.7, the lowest compiler the packed
  analyzers load in.
- It carries no slice table, since an analyzer cannot load into its own build, and it is the one project
  `WhenAProjectHoldsFolders` lists so. A file's namespace is still the project's root namespace and its first folder.

## Commands

```bash
dotnet build Alis.Reactive.Analyzers -c Release
dotnet test tests/Alis.Reactive.AnalyzerTests
```

## Refused here

| Forbidden | Refused by |
|---|---|
| A descriptor with no release row, or a row with no descriptor | the build (RS2000, RS2001) |
| File or environment access; a banned Roslyn API | the build (RS1035, RS1036) |
| An analyzer or a suppressor with no refused or no accepted case that runs it | `WhenAnArchitectureTestIsWritten` |
