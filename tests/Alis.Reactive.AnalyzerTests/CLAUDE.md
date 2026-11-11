# Alis.Reactive.AnalyzerTests

Every analyzer's cases. A case is whole C# source run through one analyzer by `Case.cs`, on
Microsoft.CodeAnalysis.Testing, which fails a case that does not compile, a case whose analyzer throws and a report
the case does not expect. `.claude/rules/analyzers.md` holds the standard of a case and loads with any file here.

- A case is a test marked `RefusedCaseAttribute` or `AcceptedCaseAttribute`. A refused case marks the reported text
  and expects the id, the severity and the message as a literal sentence; an accepted case differs from a refused
  one by one fact and expects no report. Every message has a refused case and every condition an accepted one.
- A fixture is named `When` and its situation, in the folder of the module whose rule it proves.
- A rule is done when each defect planted in it fails a case named for that defect. A planted defect that does not
  compile proves nothing, and one that no case fails on names a missing case or code no case reaches. The sweep
  runs on a copy of the projects outside the worktree.

```bash
dotnet test tests/Alis.Reactive.AnalyzerTests
```
